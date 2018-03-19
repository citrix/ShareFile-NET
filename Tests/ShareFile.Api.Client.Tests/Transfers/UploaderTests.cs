using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using FakeItEasy;

using FluentAssertions;
using NUnit.Framework;

using ShareFile.Api.Client.Exceptions;
using ShareFile.Api.Client.Requests.Executors;
using ShareFile.Api.Client.Transfers.Uploaders;
using ShareFile.Api.Client.Models;
using ShareFile.Api.Client.Transfers;
using ShareFile.Api.Client.Tests.Transfers;

namespace ShareFile.Api.Client.Core.Tests
{
    [TestFixture(Category = "Transfers")]
    public class UploaderTests : TransferBaseTests
    {
        [TestCase(UploadMethod.Standard, 1, true, false, true)]
        [TestCase(UploadMethod.Standard, 1, false, false, true)]
        [TestCase(UploadMethod.Threaded, 1, true, false, true)]
        [TestCase(UploadMethod.Threaded, 1, false, false, true)]
        [TestCase(UploadMethod.Standard, 4, true, false, true)]
        [TestCase(UploadMethod.Standard, 4, false, false, true)]
        [TestCase(UploadMethod.Threaded, 4, true, false, true)]
        [TestCase(UploadMethod.Threaded, 4, false, false, true)]
        [TestCase(UploadMethod.Standard, 1, false, true, true)]
        [TestCase(UploadMethod.Standard, 1, true, true, true)]
        [TestCase(UploadMethod.Threaded, 1, false, true, true)]
        [TestCase(UploadMethod.Threaded, 1, true, true, true)]
        [TestCase(UploadMethod.Standard, 1, true, false, false)]
        [TestCase(UploadMethod.Standard, 1, false, false, false)]
        [TestCase(UploadMethod.Threaded, 1, true, false, false)]
        [TestCase(UploadMethod.Threaded, 1, false, false, false)]
        [TestCase(UploadMethod.Standard, 4, true, false, false)]
        [TestCase(UploadMethod.Standard, 4, false, false, false)]
        [TestCase(UploadMethod.Threaded, 4, true, false, false)]
        [TestCase(UploadMethod.Threaded, 4, false, false, false)]
        [TestCase(UploadMethod.Standard, 1, false, true, false)]
        [TestCase(UploadMethod.Standard, 1, true, true, false)]
        [TestCase(UploadMethod.Threaded, 1, false, true, false)]
        [TestCase(UploadMethod.Threaded, 1, true, true, false)]
        public async Task Upload(UploadMethod uploadMethod, int megabytes, bool useAsync, bool useNonAsciiFilename, bool useRaw)
        {
            var shareFileClient = GetShareFileClient();
            var rootFolder = shareFileClient.Items.Get().Execute();
            var testFolder = new Folder { Name = RandomString(30) + ".txt" };

            testFolder = shareFileClient.Items.CreateFolder(rootFolder.url, testFolder).Execute();
            var file = GetFileToUpload(1024 * 1024 * megabytes, useNonAsciiFilename);
            //var uploadSpec = new UploadSpecificationRequest(file.Name, file.Length, testFolder.url, uploadMethod);
            var uploadSpec = new UploadSpecificationRequest
            {
                FileName = "name.png",
                FileSize = file.Length,
                Parent = testFolder.url,
                Method = uploadMethod,
                Raw = useRaw
            };
            UploaderBase uploader;

            if (useAsync)
            {
                uploader = shareFileClient.GetAsyncFileUploader(uploadSpec, file);                
            }
            else
            {
                uploader = shareFileClient.GetFileUploader(uploadSpec, file);
            }
            Assert.AreEqual(uploadSpec.Raw, useRaw);

            var progressInvocations = 0;
            var bytesTransferred = 0L;
            uploader.OnTransferProgress += (sender, args) =>
            {
                bytesTransferred = args.Progress.BytesTransferred;
                progressInvocations++;
            };

            UploadResponse uploadResponse;

            if (useAsync)
            {
                uploadResponse = await ((AsyncUploaderBase)uploader).UploadAsync();
            }
            else
            {
                uploadResponse = ((SyncUploaderBase)uploader).Upload();
            }

            shareFileClient.Items.Delete(testFolder.url);

            uploadResponse.FirstOrDefault().Should().NotBeNull();
            var expectedInvocations = Math.Ceiling((double)file.Length / UploaderBase.DefaultBufferLength) + 1;

            bytesTransferred.Should().Be(1024 * 1024 * megabytes);

            if (uploadMethod == UploadMethod.Standard)
            {
                progressInvocations.Should().Be((int)expectedInvocations, "Standard should be predictable for number of progress callbacks");
            }
            else if (uploadMethod == UploadMethod.Threaded)
            {
                progressInvocations.Should()
                    .BeLessOrEqualTo(
                        (int)expectedInvocations,
                        "Threaded scales, therefore byte ranges vary and are less predictable.  We should see no more expectedInvoations");
            }
        }

        
        [TestCase(10, true, new[] { CapabilityName.StandardUploadRaw, CapabilityName.AdvancedSearch }, ExpectedResult = typeof(AsyncScalingFileUploader))]
        [TestCase(10, false, new[] { CapabilityName.StandardUploadRaw, CapabilityName.AdvancedSearch }, ExpectedResult = typeof(ScalingFileUploader))]
        [TestCase(10, true, new[] { CapabilityName.StandardUploadForms, CapabilityName.AdvancedSearch }, ExpectedResult = typeof(AsyncScalingFileUploader))]
        [TestCase(10, false, new[] { CapabilityName.StandardUploadForms, CapabilityName.AdvancedSearch }, ExpectedResult = typeof(ScalingFileUploader))]
        [TestCase(10, true, null, ExpectedResult = typeof(AsyncScalingFileUploader))]
        [TestCase(10, false, null, ExpectedResult = typeof(ScalingFileUploader))]
        [TestCase(1, true, new[] { CapabilityName.AdvancedSearch }, ExpectedResult = typeof(AsyncScalingFileUploader))]
        [TestCase(1, false, new[] { CapabilityName.AdvancedSearch }, ExpectedResult = typeof(ScalingFileUploader))]
        [TestCase(1, true, new[] { CapabilityName.StandardUploadRaw, CapabilityName.AdvancedSearch }, ExpectedResult = typeof(AsyncStandardFileUploader))]
        [TestCase(1, false, new[] { CapabilityName.StandardUploadRaw, CapabilityName.AdvancedSearch }, ExpectedResult = typeof(StandardFileUploader))]
        [TestCase(1, true, new[] { CapabilityName.StandardUploadForms, CapabilityName.AdvancedSearch }, ExpectedResult = typeof(AsyncStandardFileUploader))]
        [TestCase(1, false, new[] { CapabilityName.StandardUploadForms, CapabilityName.AdvancedSearch }, ExpectedResult = typeof(StandardFileUploader))]
        [TestCase(1, true,  null, ExpectedResult = typeof(AsyncScalingFileUploader))]
        [TestCase(1, false, null, ExpectedResult = typeof(ScalingFileUploader))]            
        public Type VerifyUploader(int megabytes, bool useAsync, CapabilityName[] capabilityNames)
        {
            var shareFileClient = GetShareFileClient();
            var testFolder = new Folder { Name = RandomString(30) + ".txt" };
            var file = GetFileToUpload(1024 * 1024 * megabytes, false);
            var useRaw = !(capabilityNames != null && (capabilityNames.Contains(CapabilityName.StandardUploadForms)));
            var uploadSpec = new UploadSpecificationRequest
            {
                FileName = "name.png",
                FileSize = file.Length,
                Parent = testFolder.url,              
                Raw=useRaw,
            };
            if (capabilityNames != null)
            {
                uploadSpec.ProviderCapabilities =
                    new List<Capability>(capabilityNames.Select(x => new Capability { Name = x }));
            }

            UploaderBase uploader;

            if (useAsync)
            {
                uploader = shareFileClient.GetAsyncFileUploader(uploadSpec, file);
            }
            else
            {
                uploader = shareFileClient.GetFileUploader(uploadSpec, file);
            }            
            return uploader.GetType();
        }

        private AsyncScalingFileUploader SetupUploader(int serverMaxThreadsd, int userMaxThreads, int fileSize, bool useRaw)
        {
            var client = (ShareFileClient)GetShareFileClient(true);
            var testFolder = new Folder { Name = RandomString(30) + ".txt" };
            testFolder.url = new Uri("https://sf-api.com/sf/v3/Items(parent)");
            var file = GetFileToUpload(1024 * 1024 * fileSize, false);
            var uploadState = new ActiveUploadState(new UploadSpecification { MaxNumberOfThreads = serverMaxThreadsd }, 0);
            uploadState.UploadSpecification.ChunkUri = new Uri("http://localhost/");
            //var uploadSpec = new UploadSpecificationRequest(file.Name, file.Length, testFolder.url);
            var uploadSpec = new UploadSpecificationRequest
            {
                FileName = "name.png",
                FileSize = file.Length,
                Parent = testFolder.url,
                Raw = useRaw
            };
            Assert.AreEqual(uploadSpec.Raw, useRaw);
            var config = new FileUploaderConfig { NumberOfThreads = userMaxThreads };
            var uploader = new AsyncScalingFileUploader(client, uploadState, uploadSpec, file, config);
            // Setup the call in PrepareAsync that makes sure the folder is available
            A.CallTo(
                () =>
                RequestExecutorFactory.GetAsyncRequestExecutor()
                    .SendAsync(
                        A<HttpClient>.Ignored,
                        A<HttpRequestMessage>.That.Matches(i => i.RequestUri.ToString().StartsWith("https://sf-api.com/sf/v3/Items(parent)")),
                        A<HttpCompletionOption>.Ignored,
                        A<CancellationToken>.Ignored))
                .Returns(GenerateODataObjectResponse(new HttpRequestMessage(),
                @"{ ""Name"":""file.jpg"",
                    ""odata.metadata"":""https://citrix.sf-api.com/sf/v3/$metadata#Items/ShareFile.Api.Models.File@Element"",
                    ""odata.type"":""ShareFile.Api.Models.File"",}"));
            return uploader;
        }

        [TestCase(0, 0, 1, true)]
        [TestCase(0, 4, 1, true)]
        [TestCase(4, 2, 2, true)]
        [TestCase(4, 8, 4, true)]
        [TestCase(4, 0, 1, true)]
        [TestCase(0, 0, 1, false)]
        [TestCase(0, 4, 1, false)]
        [TestCase(4, 2, 2, false)]
        [TestCase(4, 8, 4, false)]
        [TestCase(4, 0, 1, false)]
        public async Task AsyncScalingFileUploader_LimitThreads(int max, int specified, int expected, bool useRaw)
        {
            // Arrange
            var uploader = SetupUploader(max, specified, 10, useRaw);            

            // Act
            await uploader.PrepareAsync();

            // Assert
            uploader.PartUploader.NumberOfThreads.Should().Be(expected);
        }

        [TestCase(true,1, ExpectedResult = 1)]
        [TestCase(true,7, ExpectedResult = 1)]
        [TestCase(true,8, ExpectedResult = 2)]
        [TestCase(true,15, ExpectedResult = 2)]
        [TestCase(true,16, ExpectedResult = 3)]
        [TestCase(true,24, ExpectedResult = 4)]
        [TestCase(true,50, ExpectedResult = 4)]
        [TestCase(false,1, ExpectedResult = 1)]
        [TestCase(false,7, ExpectedResult = 1)]
        [TestCase(false,8, ExpectedResult = 2)]
        [TestCase(false,15, ExpectedResult = 2)]
        [TestCase(false,16, ExpectedResult = 3)]
        [TestCase(false,24, ExpectedResult = 4)]
        [TestCase(false,50, ExpectedResult = 4)]
        public async Task<int> AsyncScalingFileUploader_LimitThreadsBasedOnFileSize(bool useRaw, int fileSize)
        {
            // Arrange
            var uploader = SetupUploader(4, 4, fileSize, useRaw);
                        
            // Act
            try
            {
                await uploader.UploadAsync();
            }
            catch (UploadException)
            {
                // Didn't setup the actual API calls, but we don't need that part to succeed
            }

            // Assert
            return uploader.PartUploader.NumberOfThreads;
        }

        [TestCase(0, 0, 1, true)]
        [TestCase(0, 4, 1, true)]
        [TestCase(4, 2, 2, true)]
        [TestCase(4, 8, 4, true)]
        [TestCase(4, 0, 1, true)]
        [TestCase(0, 0, 1, false)]
        [TestCase(0, 4, 1, false)]
        [TestCase(4, 2, 2, false)]
        [TestCase(4, 8, 4, false)]
        [TestCase(4, 0, 1, false)]
        public void ScalingFileUploader_ChangeNumberOfThreads(int max, int specified, int expected, bool useRaw)
        {
            // Arrange
            var client = (ShareFileClient)GetShareFileClient();
            var testFolder = new Folder { Name = RandomString(30) + ".txt" };
            testFolder.url = new Uri("https://sf-api.com/sf/v3/Items(parent)");
            var file = GetFileToUpload(1024 * 1024 * 10, false);
            var uploadState = new ActiveUploadState(new UploadSpecification { MaxNumberOfThreads = max }, 0);
            //var uploadSpec = new UploadSpecificationRequest(file.Name, file.Length, testFolder.url);
            var uploadSpec = new UploadSpecificationRequest
            {
                FileName = "name.png",
                FileSize = file.Length,
                Parent = testFolder.url,
                Raw = useRaw
            };

            var config = new FileUploaderConfig { NumberOfThreads = specified };
            var uploader = new ScalingFileUploader(client, uploadState, uploadSpec, file, config);
            Assert.AreEqual(uploadSpec.Raw, useRaw);
            // Act
            // The upload part is not fully setup, but we don't need to get that far, so let it throw
            Assert.Throws<NullReferenceException>(() => uploader.Upload());

            // Assert
            uploader.PartUploader.NumberOfThreads.Should().Be(expected);
        }

        [TestCase(UploadMethod.Standard, true, false)]
        [TestCase(UploadMethod.Threaded, true, false)]
        [TestCase(UploadMethod.Standard, false, false)]
        [TestCase(UploadMethod.Threaded, false, false)]
        [TestCase(UploadMethod.Threaded, true, true)]
        public async Task UploadZeroByteFile(UploadMethod uploadMethod, bool useAsync, bool useMemmapForAsyncThreaded)
        {
            string filename = "0byte";
            var shareFileClient = (ShareFileClient)GetShareFileClient();
            var rootFolder = shareFileClient.Items.Get().Execute();
            var testFolder = new Folder { Name = RandomString(30) + ".txt" };

            testFolder = shareFileClient.Items.CreateFolder(rootFolder.url, testFolder).Execute();
            //var uploadSpec = new UploadSpecificationRequest(file.Name, file.Length, testFolder.url, uploadMethod);
            var uploadSpec = new UploadSpecificationRequest
            {
                FileName = filename,
                FileSize = 0,
                Parent = testFolder.url,
                Method = uploadMethod,
                Raw = true,
            };

            UploaderBase uploader;
            if(uploadMethod == UploadMethod.Threaded && useAsync && useMemmapForAsyncThreaded)
            {
                System.IO.FileStream fileStream = null;
                try
                {
                    fileStream = System.IO.File.Create(filename);
                    fileStream.Close();
                    fileStream = System.IO.File.OpenRead(filename);
                    uploader = new AsyncMemoryMappedFileUploader(shareFileClient, uploadSpec, fileStream);
                }
                finally
                {
                    fileStream?.Close();
                }
            }
            else if (useAsync)
            {
                uploader = shareFileClient.GetAsyncFileUploader(uploadSpec, new System.IO.MemoryStream());
            }
            else
            {
                uploader = shareFileClient.GetFileUploader(uploadSpec, new System.IO.MemoryStream());
            }

            UploadResponse uploadResponse;
            if (useAsync)
            {
                uploadResponse = await((AsyncUploaderBase)uploader).UploadAsync();
            }
            else
            {
                uploadResponse = ((SyncUploaderBase)uploader).Upload();
            }

            shareFileClient.Items.Delete(testFolder.url);
            if(System.IO.File.Exists(filename)) { try { System.IO.File.Delete(filename); } catch { } }

            uploadResponse.FirstOrDefault().Should().NotBeNull();
        }
    } 
}
