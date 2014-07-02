#if !Async
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using ShareFile.Api.Client.Core.Transfers.Uploaders;
using ShareFile.Api.Client.Exceptions;
using ShareFile.Api.Client.FileSystem;
using ShareFile.Api.Client.Security.Cryptography;
using ShareFile.Api.Models;
using ShareFile.Api.Client.Requests;
using ShareFile.Api.Client.Extensions.Tasks;

namespace ShareFile.Api.Client.Transfers.Uploaders
{
    public class ThreadedFileUploader : SyncUploaderBase, IDisposable
    {
        public ThreadedFileUploader(ShareFileClient client, UploadSpecificationRequest uploadSpecificationRequest, IPlatformFile file, FileUploaderConfig config = null, int? expirationDays = null)
            : base(client, uploadSpecificationRequest, file, expirationDays)
        {
            _itemsToUpload = new Queue<FilePart>();
            _activeFileParts = new List<FilePart>();

            Config = config ?? new FileUploaderConfig();

            _effectivePartSize = Config.PartSize;
            HashProvider = MD5HashProviderFactory.GetHashProvider().CreateHash();
            Progress = new TransferProgress
            {
                TransferId = Guid.NewGuid().ToString(),
                BytesTransferred = 0,
                BytesRemaining = uploadSpecificationRequest.FileSize,
                TotalBytes = uploadSpecificationRequest.FileSize
            };
        }

        public FileUploaderConfig Config { get; private set; }
        public TransferProgress Progress { get; set; }

        private readonly List<FilePart> _activeFileParts;
        private readonly Queue<FilePart> _itemsToUpload;
        private FilePartUploader[] _partUploaders;
        private int _numberOfItemsProcessed;
        private int _numberOfItemsQueued;
        private int _effectivePartSize;
        private int _threadCount;

        ///<summary>
        /// Object used to lock for reporting progress.
        ///</summary>
        public readonly object CounterLock = new object();

        ///<summary>
        /// Object used to lock for enqueueing and dequeueing an IThreadQueueItem
        ///</summary>
        public readonly object QueueAccessLock = new object();

        ///<summary>
        /// Object used to lock for checking for thread completion.
        ///</summary>
        public readonly object WaitForCompletionLock = new object();

        public override void Prepare()
        {
            if (!Prepared)
            {
                UploadSpecification = CreateUploadSpecificationQuery(UploadSpecificationRequest).Execute();

                CheckResume();
                BuildFileParts();

                Prepared = true;
            }
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

                EnqueueFilePart(part);
                index++;
                offset += _effectivePartSize;
            }
        }

        public override UploadResponse Upload(Dictionary<string, object> transferMetadata = null)
        {
            TransferMetadata = transferMetadata;
            Progress.TransferMetadata = transferMetadata;

            Prepare();
            StartUpload();
            WaitForPartsToComplete();

            return FinishUpload();
        }

        private void StartUpload()
        {
            _threadCount = Config.NumberOfThreads;

            if (GetFilePartQueueCount() < _threadCount)
            {
                _threadCount = GetFilePartQueueCount();
            }

            _partUploaders = new FilePartUploader[_threadCount];

            for (int i = 0; i < _threadCount; i++)
            {
                var thread = new FilePartUploader(i.ToString(), this);
                _partUploaders[i] = thread;

                if (i > 0 && Config.ThreadStartPauseInMS > 0)
                {
                    WaitPauseTime();
                }

                _partUploaders[i].Uploader.Start();
            }
        }

        private UploadResponse FinishUpload()
        {
            var finishUri = GetComposedFinishUri();
            var client = GetHttpClient();

            var message = new HttpRequestMessage(HttpMethod.Get, finishUri);
            message.Headers.Add("Accept", "application/json");

            var response = client.SendAsync(message).WaitForTask();
            if (response.IsSuccessStatusCode)
            {
                using (var responseStream = response.Content.ReadAsStreamAsync().WaitForTask())
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

            throw new UploadException("Error completing upload.", -1);
        }

        private string GetComposedFinishUri()
        {
            var finishUri = new StringBuilder(string.Format("{0}&respformat=json", UploadSpecification.FinishUri.AbsoluteUri));

            if (File.Length > 0)
            {
                finishUri.AppendFormat("&filehash={0}", HashProvider.GetComputedHashAsString());
            }

            if (!string.IsNullOrEmpty(UploadSpecificationRequest.Details))
                finishUri.AppendFormat("&details={0}", Uri.EscapeDataString(UploadSpecificationRequest.Details));
            if (!string.IsNullOrEmpty(UploadSpecificationRequest.Title))
                finishUri.AppendFormat("&title={0}", Uri.EscapeDataString(UploadSpecificationRequest.Title));

            return finishUri.ToString();
        }

        private void WaitPauseTime()
        {
            Thread.Sleep(Config.ThreadStartPauseInMS);
        }

        private void CheckResume()
        {
            if (UploadSpecification.IsResume)
            {
                if (UploadSpecification.ResumeFileHash != CalculateHash(UploadSpecification.ResumeOffset))
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

        private string CalculateHash(long count)
        {
            using (var fileStream = File.OpenRead())
            {
                do
                {
                    var buffer = new byte[65536];

                    if (count < buffer.Length)
                    {
                        buffer = new byte[count];
                    }

                    var bytesRead = fileStream.Read(buffer, 0, buffer.Length);

                    if (bytesRead > 0)
                    {
                        HashProvider.Append(buffer, 0, buffer.Length);
                    }

                    count -= bytesRead;

                } while (count > 0);
            }

            return HashProvider.GetComputedHashAsString();
        }


        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void EnqueueFilePart(FilePart filePart)
        {
            lock (QueueAccessLock)
            {
                _itemsToUpload.Enqueue(filePart);
                _numberOfItemsQueued++;
                Monitor.PulseAll(QueueAccessLock);
            }
        }

        public int GetFilePartQueueCount()
        {
            lock (QueueAccessLock)
            {
                return _itemsToUpload.Count;
            }
        }

        public FilePart DequeueFilePart()
        {
            lock (QueueAccessLock)
            {
                if (_itemsToUpload.Count == 0) return null;

                var filePart = FillFilePart(_itemsToUpload.Dequeue());
                _activeFileParts.Add(filePart);

                return filePart;
            }
        }

        private FilePart FillFilePart(FilePart filePart)
        {
            var buffer = new byte[filePart.Length];

            var file = File.OpenRead();
            file.Seek(filePart.Offset, SeekOrigin.Begin);
            int bytesRead = file.Read(buffer, 0, buffer.Length);

            Array.Resize(ref buffer, bytesRead);

            filePart.Bytes = buffer;

            var partHashProvider = MD5HashProviderFactory.GetHashProvider().CreateHash();

            filePart.Hash = partHashProvider.ComputeHash(filePart.Bytes);

            if (filePart.IsLastPart)
            {
                HashProvider.Finalize(filePart.Bytes, 0, filePart.Bytes.Length);
            }
            else HashProvider.Append(filePart.Bytes, 0, filePart.Bytes.Length);

            return filePart;
        }

        public void ClearThreadQueue()
        {
            lock (QueueAccessLock)
            {
                _numberOfItemsQueued -= _itemsToUpload.Count;
                _itemsToUpload.Clear();
                _activeFileParts.Clear();
            }
        }

        public void FilePartComplete(FilePart filePart)
        {
            lock (WaitForCompletionLock)
            {
                _activeFileParts.Remove(filePart);
                _numberOfItemsProcessed++;
            }
        }

        public void FilePartException()
        {
            throw new NotImplementedException();
        }

        public List<FilePart> GetActiveFileParts()
        {
            lock (QueueAccessLock)
            {
                return _activeFileParts;
            }
        }

        public void WaitForPartsToComplete()
        {
            lock (WaitForCompletionLock)
            {
                while (_numberOfItemsProcessed != _numberOfItemsQueued)
                {
                    Monitor.Wait(WaitForCompletionLock, 100);

                    foreach (var partUploader in _partUploaders)
                    {
                        if (partUploader.GetLastException() != null)
                        {
                            Shutdown();
                            throw partUploader.GetLastException();
                        }
                    }
                }
            }
        }

        public void Shutdown()
        {
            foreach (var partUpload in _partUploaders)
            {
                partUpload.Shutdown();
            }
        }

        internal void OnProgress(int bytesTransferred)
        {
            Progress.BytesTransferred += bytesTransferred;

            NotifyProgress(Progress);
        }

        protected internal override HttpClient GetHttpClient()
        {
            return new HttpClient(GetHttpClientHandler())
            {
                Timeout = new TimeSpan(0, 0, 0, 0, Config.HttpTimeout)
            };
        }
    }

    internal class FilePartUploader
    {
        public ThreadedFileUploader ThreadedFileUploader { get; set; }
        public string Id { get; set; }
        public Thread Uploader { get; private set; }

        private Exception _exception;
        private bool _shutdown;

        public FilePartUploader(string id, ThreadedFileUploader threadedFileUploader)
        {
            Id = id;
            ThreadedFileUploader = threadedFileUploader;
            Uploader = new Thread(UploadParts);
        }

        private void UploadParts()
        {
            FilePart currentPart;

            while ((currentPart = ThreadedFileUploader.DequeueFilePart()) != null)
            {
                UploadPart(currentPart);
                ThreadedFileUploader.FilePartComplete(currentPart);
            }
        }

        private void UploadPart(FilePart part)
        {
            var retryCount = 4;
            ShareFileApiResponse<string> result;
            Exception requestException = null;

            do
            {
                try
                {
                    result = Send(part);
                }
                catch (Exception exception)
                {
                    requestException = exception;
                    result = new ShareFileApiResponse<string> { Error = true };
                }

                if (result.Error)
                {
                    // subtract the progress made
                    ThreadedFileUploader.OnProgress(-part.Bytes.Length);
                }

                retryCount--;
            } while (retryCount > 0 && result.Error);

            if (retryCount <= 0 || result.Error)
            {
                _exception = new ApplicationException(string.Format("Chunk {0} failed after 3 retries{1}Response: {2}", part.Index, Environment.NewLine, result.ErrorMessage), requestException);
            }

            part.BytesUploaded = part.Bytes.Length;

            part.Bytes = null;
        }

        private ShareFileApiResponse<string> Send(FilePart part)
        {
            var client = ThreadedFileUploader.GetHttpClient();

            var message = new HttpRequestMessage(HttpMethod.Post, part.GetComposedUploadUrl())
            {
                Content = new ByteArrayContent(part.Bytes, 0, part.Bytes.Length)
            };

            ThreadedFileUploader.OnProgress(part.Bytes.Length);

            var response = client.SendAsync(message).WaitForTask();
            using(var responseStream = response.Content.ReadAsStreamAsync().WaitForTask())
            using (var textReader = new JsonTextReader(new StreamReader(responseStream)))
            {
                return new JsonSerializer().Deserialize<ShareFileApiResponse<string>>(textReader);
            }
        }

        private void WaitToFinish(int index)
        {
            if (index <= 0)
            {
                return;
            }

            while (!CanThreadFinish(index))
            {
                Thread.Sleep(100);
            }
        }

        private bool CanThreadFinish(int index)
        {
            foreach (var item in ThreadedFileUploader.GetActiveFileParts())
            {
                if (item.Index < index)
                {
                    return false;
                }
            }

            return true;
        }

        public Exception GetLastException()
        {
            return _exception;
        }

        public void Shutdown()
        {
            _shutdown = true;
        }
    }

    public abstract class SyncUploaderBase : UploaderBase
    {
        public abstract UploadResponse Upload(Dictionary<string, object> transferMetadata = null);
        public abstract void Prepare();

        protected SyncUploaderBase(ShareFileClient client, UploadSpecificationRequest uploadSpecificationRequest, IPlatformFile file, int? expirationDays)
            : base(client, uploadSpecificationRequest, file, expirationDays)
        {

        }
    }
}
#endif