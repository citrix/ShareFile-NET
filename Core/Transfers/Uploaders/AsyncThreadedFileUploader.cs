using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ShareFile.Api.Client.Exceptions;
using ShareFile.Api.Client.FileSystem;
using ShareFile.Api.Client.Security.Cryptography;
using ShareFile.Api.Models;

namespace ShareFile.Api.Client.Transfers.Uploaders
{
#if Async
    public class AsyncThreadedFileUploader : TransfererBase
    {
        private AsyncThreadedFileUploader(ShareFileClient client, IPlatformFile file, FileUploaderConfig config = null)
        {
            _itemsToUpload = new Queue<FilePart>();
            _itemsToFill = new Queue<FilePart>();

            Config = config ?? new FileUploaderConfig();

            _effectivePartSize = Config.PartSize;
            HashProvider = MD5HashProviderFactory.GetHashProvider().CreateHash();
            Progress = new TransferProgress
            {
                TransferId = Guid.NewGuid().ToString(),
                BytesTransferred = 0
            };

            Client = client;
            File = file;
        }

        public AsyncThreadedFileUploader(ShareFileClient client, UploadSpecification uploadSpecification,
            IPlatformFile file, FileUploaderConfig config = null)
            : this (client, file, config)
        {
            UploadSpecification = uploadSpecification;
            Progress.TotalBytes = uploadSpecification.IsResume
                ? (file.Length - uploadSpecification.ResumeOffset)
                : file.Length;

            Progress.BytesRemaining = Progress.TotalBytes;
        }

        public AsyncThreadedFileUploader(ShareFileClient client, UploadSpecificationRequest uploadSpecificationRequest, IPlatformFile file, FileUploaderConfig config = null)
            : this(client, file, config)
        {   
            UploadSpecificationRequest = uploadSpecificationRequest;

            Progress.BytesRemaining = Progress.TotalBytes = uploadSpecificationRequest.FileSize;
        }

        public FileUploaderConfig Config { get; private set; }
        public TransferProgress Progress { get; set; }
        protected IMD5HashProvider HashProvider { get; set; }
        protected ShareFileClient Client { get; set; }
        protected UploadSpecificationRequest UploadSpecificationRequest { get; set; }

        private readonly Queue<FilePart> _itemsToFill;
        private readonly Queue<FilePart> _itemsToUpload;
        private AsyncSemaphore _maxConsumersSemaphore;
        private AsyncSemaphore _pendingPartSemaphore;
        private IPlatformFile File { get; set; }
        private Dictionary<string, object> TransferMetadata { get; set; }
        private CancellationToken? _cancellationToken { get; set; }

        private int _effectivePartSize;

        protected bool Prepared { get; set; }
        protected UploadSpecification UploadSpecification { get; set; }

        ///<summary>
        /// Object used to lock for reporting progress.
        ///</summary>
        public readonly object CounterLock = new object();

        public async Task PrepareAsync()
        {
            if (!Prepared)
            {
                if (UploadSpecification == null)
                {
                    UploadSpecification = await CreateUpload(UploadSpecificationRequest);
                }

                await CheckResumeAsync();
                BuildFileParts();

                Prepared = true;
            }
        }

        public async Task<UploadSpecification> CreateUpload(UploadSpecificationRequest uploadSpecificationRequest)
        {
            var query = Client.Items.Upload(uploadSpecificationRequest.Parent, uploadSpecificationRequest.Method,
                uploadSpecificationRequest.Raw, uploadSpecificationRequest.FileName, uploadSpecificationRequest.FileSize,
                uploadSpecificationRequest.BatchId,
                uploadSpecificationRequest.BatchLast, uploadSpecificationRequest.CanResume,
                uploadSpecificationRequest.StartOver, uploadSpecificationRequest.Unzip, uploadSpecificationRequest.Tool,
                uploadSpecificationRequest.Overwrite, uploadSpecificationRequest.Title,
                uploadSpecificationRequest.Details, uploadSpecificationRequest.IsSend,
                uploadSpecificationRequest.SendGuid, null, uploadSpecificationRequest.ThreadCount,
                uploadSpecificationRequest.ResponseFormat, uploadSpecificationRequest.Notify, 
                uploadSpecificationRequest.ClientCreatedDateUtc, uploadSpecificationRequest.ClientModifiedDateUtc);

            return await query.ExecuteAsync(_cancellationToken);
        }

        private void BuildFileParts()
        {
            var numberOfParts =
                (int)Math.Ceiling((double)(File.Length - UploadSpecification.ResumeOffset) / _effectivePartSize);

            if (File.Length == 0) numberOfParts = 0;

            if (numberOfParts > 1 && numberOfParts < Config.NumberOfThreads)
            {
                numberOfParts = Config.NumberOfThreads;
                _effectivePartSize = (int)Math.Ceiling((double)(File.Length - UploadSpecification.ResumeOffset) / Config.NumberOfThreads);
            }

            var offset = UploadSpecification.ResumeOffset;
            var index = (int)UploadSpecification.ResumeIndex;

            for (var i = 0; i < numberOfParts; i++)
            {
                var part = new FilePart
                {
                    Index = index,
                    Length = _effectivePartSize,
                    Offset = offset,
                    UploadUrl = UploadSpecification.ChunkUri.AbsoluteUri,
                    IsLastPart = i + 1 == numberOfParts
                };
                _itemsToFill.Enqueue(part);
                index++;
                offset += _effectivePartSize;
            }
        }

        public async Task<UploadResponse> UploadAsync(Dictionary<string, object> transferMetadata = null, CancellationToken? cancellationToken = null)
        {
            TransferMetadata = transferMetadata ?? new Dictionary<string, object>();
            Progress.TransferMetadata = TransferMetadata;
            _cancellationToken = cancellationToken;

            _maxConsumersSemaphore = new AsyncSemaphore(Config.NumberOfThreads);
            _pendingPartSemaphore = new AsyncSemaphore(0);
            await PrepareAsync();

            if (_itemsToFill.Count > 0)
            {
                await WhenAll(StartReaders(), StartUploaders());
            }
            return await FinishUploadAsync();
        }

        private readonly Action<Task[], TaskCompletionSource<object>> _setResultAction =
            (completedTasks, tcs) => tcs.TrySetResult(null);

        private Task WhenAll(params Task[] tasks)
        {
            if (tasks == null)
                throw new ArgumentNullException("tasks");
            var tcs = new TaskCompletionSource<object>();
            var taskArray = tasks;
            if (taskArray.Length == 0)
                _setResultAction(taskArray, tcs);
            else
                Task.Factory.ContinueWhenAll(taskArray, completedTasks =>
                {
                    List<Exception> exceptions = new List<Exception>();
                    var canceled = false;
                    foreach (var task in completedTasks)
                    {
                        if (task.IsFaulted)
                        {
                            exceptions.Add(task.Exception);
                        }
                        else if (task.IsCanceled)
                            canceled = true;
                    }
                    if (exceptions.Count > 0)
                        tcs.TrySetException(exceptions);
                    else if (canceled)
                        tcs.TrySetCanceled();
                    else
                    {
                        _setResultAction(completedTasks, tcs);
                    }
                }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);

            return tcs.Task;
        }

        private bool IsCancellationRequested()
        {
            if (_cancellationToken == null) return false;

            return _cancellationToken.Value.IsCancellationRequested;
        }

        private async Task StartReaders()
        {
            FilePart part = null;
            while (_itemsToFill.Count > 0 && !IsCancellationRequested())
            {
                await _maxConsumersSemaphore.WaitAsync();
                if (_itemsToFill.Count > 0) part = _itemsToFill.Dequeue(); else part = null;
                if (part != null)
                {
                    await FillFilePartAsync(part);
                    //Console.WriteLine("Filled " + part.Index);
                    HashProvider.Append(part.Bytes, 0, part.Bytes.Length);

                    _itemsToUpload.Enqueue(part);
                    _pendingPartSemaphore.Release();
                }
            }
        }

        private async Task StartUploaders()
        {
            FilePart part = null;
            do
            {
                await _pendingPartSemaphore.WaitAsync();

                await TryPause(_cancellationToken);

                part = _itemsToUpload.Dequeue();
#pragma warning disable 4014
                // This is required to allow for concurrent threads to upload file parts.
                UploadPartAsync(part).ContinueWith((task) =>
                {
                    _maxConsumersSemaphore.Release();
                });
#pragma warning restore 4014

            } while (part != null && !part.IsLastPart && !IsCancellationRequested());
            // some uploaders are still going at this point; wait until all finish
            await _maxConsumersSemaphore.WaitIdleAsync();
        }

        private async Task UploadPartAsync(FilePart part)
        {
            var retryCount = 4;
            ShareFileApiResponse<string> result;
            Exception requestException = null;
            do
            {
                try
                {
                    result = await SendAsync(part);
                }
                catch (Exception exception)
                {
                    requestException = exception;
                    result = new ShareFileApiResponse<string> { Error = true };
                }

                if (!result.Error)
                {
                    OnProgress(part.Bytes.Length);
                }
                retryCount--;

            } while (retryCount > 0 && result.Error && !IsCancellationRequested());

            if (IsCancellationRequested()) return;

            if (retryCount <= 0 || result.Error)
            {
                throw new ApplicationException(string.Format("Chunk {0} failed after 3 retries{1} Response: {2}", part.Index, Environment.NewLine, result.ErrorMessage), requestException);
            }

            part.BytesUploaded = part.Bytes.Length;
            part.Bytes = null;
        }

        private async Task<ShareFileApiResponse<string>> SendAsync(FilePart part)
        {
            var client = new HttpClient(GetHttpClientHandler())
            {
                Timeout = new TimeSpan(0, 0, 0, 0, Config.HttpTimeout)
            };
            var message = new HttpRequestMessage(HttpMethod.Post, part.GetComposedUploadUrl())
            {
                Content = new ByteArrayContent(part.Bytes, 0, part.Bytes.Length)
            };

            string retVal;

            HttpResponseMessage response;
            if (_cancellationToken == null)
            {
                response = await client.SendAsync(message);
            }
            else response = await client.SendAsync(message, _cancellationToken.Value);

            var responseStream = await response.Content.ReadAsStreamAsync();
            using (var streamReader = new StreamReader(responseStream))
            {
                retVal = await streamReader.ReadToEndAsync();
            }
            return JsonConvert.DeserializeObject<ShareFileApiResponse<string>>(retVal);
        }

        internal HttpClientHandler GetHttpClientHandler()
        {
            var httpClientHandler = new HttpClientHandler
            {
                AllowAutoRedirect = true,
                CookieContainer = Client.CookieContainer,
                Credentials = Client.CredentialCache,
                Proxy = Client.Configuration.ProxyConfiguration
            };

            if (Client.Configuration.ProxyConfiguration != null && httpClientHandler.SupportsProxy)
            {
                httpClientHandler.UseProxy = true;
            }

            return httpClientHandler;
        }

        private async Task<UploadResponse> FinishUploadAsync()
        {
            var finishUri = GetComposedFinishUri();

            var client = new HttpClient(GetHttpClientHandler()) { Timeout = new TimeSpan(0, 0, 0, 0, Config.HttpTimeout) };
            var message = new HttpRequestMessage(HttpMethod.Get, finishUri);
            message.Headers.Add("Accept", "application/json");

            var response = await client.SendAsync(message);
            if (response.IsSuccessStatusCode)
            {
                using (var responseStream = await response.Content.ReadAsStreamAsync())
                using (var textReader = new JsonTextReader(new StreamReader(responseStream)))
                {
                    var uploadResponse = new JsonSerializer().Deserialize<ShareFileApiResponse<UploadResponse>>(textReader);

                    if (uploadResponse.Error)
                    {
                        throw new UploadException(uploadResponse.ErrorMessage, uploadResponse.ErrorCode);
                    }

                    return uploadResponse.Value;
                }
            }

            if (response.Content != null)
            {
                Client.Logging.Error(await response.Content.ReadAsStringAsync());
            }

            throw new UploadException("Error completing upload.", -1);
        }

        private string GetComposedFinishUri()
        {
            var finishUri = new StringBuilder(string.Format("{0}&respformat=json", UploadSpecification.FinishUri.AbsoluteUri));

            if (File.Length > 0)
            {
                HashProvider.Finalize(new byte[1], 0, 0);
                finishUri.AppendFormat("&filehash={0}", HashProvider.GetComputedHashAsString());
            }

            if (!string.IsNullOrEmpty(UploadSpecificationRequest.Details))
                finishUri.AppendFormat("&details={0}", Uri.EscapeDataString(UploadSpecificationRequest.Details));
            if (!string.IsNullOrEmpty(UploadSpecificationRequest.Title))
                finishUri.AppendFormat("&title={0}", Uri.EscapeDataString(UploadSpecificationRequest.Title));

            return finishUri.ToString();
        }

        private async Task CheckResumeAsync()
        {
            if (UploadSpecification.IsResume)
            {
                if (UploadSpecification.ResumeFileHash != await CalculateHashAsync(UploadSpecification.ResumeOffset))
                {
                    HashProvider = MD5HashProviderFactory.GetHashProvider().CreateHash();
                    UploadSpecification.ResumeIndex = 0;
                    UploadSpecification.ResumeOffset = 0;
                }
                else
                {
                    UploadSpecification.ResumeIndex += 1;
                }
            }
        }

        private const int MaxBufferLength = 65536;
        private async Task<string> CalculateHashAsync(long count)
        {
            var localHash = MD5HashProviderFactory.GetHashProvider().CreateHash();
            using (var fileStream = await File.OpenReadAsync())
            {
                var buffer = new byte[MaxBufferLength];
                do
                {
                    var bytesToRead = count < MaxBufferLength ? (int)count : MaxBufferLength;
                    var bytesRead = fileStream.Read(buffer, 0, bytesToRead);
                    if (bytesRead > 0)
                    {
                        localHash.Append(buffer, 0, bytesToRead);
                        HashProvider.Append(buffer, 0, bytesToRead);
                    }
                    count -= bytesRead;
                } while (count > 0);
            }
            localHash.Finalize(new byte[1], 0, 0);
            return localHash.GetComputedHashAsString();
        }

        private async Task<FilePart> FillFilePartAsync(FilePart filePart)
        {
            var buffer = new byte[filePart.Length];

            using (var file = await File.OpenReadAsync())
            {
                file.Seek(filePart.Offset, SeekOrigin.Begin);
                int bytesRead = file.Read(buffer, 0, buffer.Length);
                Array.Resize(ref buffer, bytesRead);
                filePart.Bytes = buffer;
            }
            var partHashProvider = MD5HashProviderFactory.GetHashProvider().CreateHash();
            filePart.Hash = partHashProvider.ComputeHash(filePart.Bytes);
            return filePart;
        }

        internal void OnProgress(int bytesTransferred)
        {
            Progress.BytesTransferred += bytesTransferred;
            NotifyProgress(Progress);
        }

        public EventHandler<TransferEventArgs> OnTransferProgress;
        protected void NotifyProgress(TransferProgress progress)
        {
            if (OnTransferProgress != null)
            {
                OnTransferProgress.Invoke(this, new TransferEventArgs { Progress = progress });
            }
        }
    }
#endif
}
