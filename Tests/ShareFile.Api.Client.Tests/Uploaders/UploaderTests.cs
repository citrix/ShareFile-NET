using System.Diagnostics;
using System.Security.Cryptography;

using ShareFile.Api.Client.FileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using FakeItEasy;

using FluentAssertions;
using NUnit.Framework;
using Newtonsoft.Json;

using ShareFile.Api.Client.Exceptions;
using ShareFile.Api.Client.Logging;
using ShareFile.Api.Client.Requests.Executors;
using ShareFile.Api.Client.Security.Authentication.OAuth2;
using ShareFile.Api.Client.Transfers.Uploaders;
using ShareFile.Api.Models;
using ShareFile.Api.Client.Transfers;

namespace ShareFile.Api.Client.Core.Tests.Uploaders
{
    public class UploaderTests : BaseTests
    {
        private object oauthTokenLock = new object();
        private OAuthToken token = null;

        protected IShareFileClient GetShareFileClient()
        {
            try
            {
                using (var fileStream = System.IO.File.OpenRead("TestConfig.json"))
                using (var streamReader = new StreamReader(fileStream))
                {
                    var info = streamReader.ReadToEnd();
                    var userInfo = JsonConvert.DeserializeObject<UserInfo>(info);

                    var sfClient = new ShareFileClient(userInfo.GetBaseUri().ToString());
                    lock (oauthTokenLock)
                    {
                        if (token == null)
                        {
                            var oauthService = new OAuthService(sfClient, userInfo.ClientId, userInfo.ClientSecret);
                            token = oauthService.GetPasswordGrantRequestQuery(userInfo.Email, userInfo.Password, userInfo.Subdomain, userInfo.Domain).Execute();
                        }
                    }

                    sfClient.BaseUri = token.GetUri();
                    sfClient.AddOAuthCredentials(token);
                    return sfClient;
                }
            }
            catch (Exception exception)
            {
                Assert.Inconclusive(string.Format("No UserInfo found in TestConfig.json. Exception: {0}", exception));
                throw;
            }
        }

        protected PlatformFileStream GetFileToUpload(int size, bool useNonAsciiFilename)
        {
            var bytes = new byte[size];

            RandomNumberGenerator.Create().GetBytes(bytes);

            return new PlatformFileStream(new MemoryStream(bytes), (long)size,
                useNonAsciiFilename ? GetNonAsciiFilename() : RandomString(20));
        }

        [TestCase(UploadMethod.Standard, 1, true, false)]
        [TestCase(UploadMethod.Standard, 1, false, false)]
        [TestCase(UploadMethod.Threaded, 1, true, false)]
        [TestCase(UploadMethod.Threaded, 1, false, false)]
        [TestCase(UploadMethod.Standard, 4, true, false)]
        [TestCase(UploadMethod.Standard, 4, false, false)]
        [TestCase(UploadMethod.Threaded, 4, true, false)]
        [TestCase(UploadMethod.Threaded, 4, false, false)]
        [TestCase(UploadMethod.Standard, 1, false, true)]
        [TestCase(UploadMethod.Standard, 1, true, true)]
        [TestCase(UploadMethod.Threaded, 1, false, true)]
        [TestCase(UploadMethod.Threaded, 1, true, true)]
        public async void Upload(UploadMethod uploadMethod, int megabytes, bool useAsync, bool useNonAsciiFilename)
        {
            var shareFileClient = GetShareFileClient();
            var rootFolder = shareFileClient.Items.Get().Execute();
            var testFolder = new Folder { Name = RandomString(30) + ".txt" };

            testFolder = shareFileClient.Items.CreateFolder(rootFolder.url, testFolder).Execute();
            var file = GetFileToUpload(1024 * 1024 * megabytes, useNonAsciiFilename);
            var uploadSpec = new UploadSpecificationRequest(file.Name, file.Length, testFolder.url, uploadMethod);

            UploaderBase uploader;

            if (useAsync)
            {
                uploader = shareFileClient.GetAsyncFileUploader(uploadSpec, file);
            }
            else
            {
                uploader = shareFileClient.GetFileUploader(uploadSpec, file);
            }

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
        [TestCase(10, true, null, ExpectedResult = typeof(AsyncScalingFileUploader))]
        [TestCase(10, false, null, ExpectedResult = typeof(ScalingFileUploader))]
        [TestCase(1, true, new[] { CapabilityName.AdvancedSearch }, ExpectedResult = typeof(AsyncScalingFileUploader))]
        [TestCase(1, false, new[] { CapabilityName.AdvancedSearch }, ExpectedResult = typeof(ScalingFileUploader))]
        [TestCase(1, true, new[] { CapabilityName.StandardUploadRaw, CapabilityName.AdvancedSearch }, ExpectedResult = typeof(AsyncStandardFileUploader))]
        [TestCase(1, false, new[] { CapabilityName.StandardUploadRaw, CapabilityName.AdvancedSearch }, ExpectedResult = typeof(StandardFileUploader))]
        [TestCase(1, true, null, ExpectedResult = typeof(AsyncScalingFileUploader))]
        [TestCase(1, false, null, ExpectedResult = typeof(ScalingFileUploader))]
        public Type VerifyUploader(int megabytes, bool useAsync, CapabilityName[] capabilityNames)
        {
            var shareFileClient = GetShareFileClient();
            var testFolder = new Folder { Name = RandomString(30) + ".txt" };
            var file = GetFileToUpload(1024 * 1024 * megabytes, false);
            var uploadSpec = new UploadSpecificationRequest(file.Name, file.Length, testFolder.url);
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

        private AsyncScalingFileUploader SetupUploader(int serverMaxThreadsd, int userMaxThreads, int fileSize)
        {
            var client = (ShareFileClient)GetShareFileClient(true);
            var testFolder = new Folder { Name = RandomString(30) + ".txt" };
            testFolder.url = new Uri("https://sf-api.com/sf/v3/Items(parent)");
            var file = GetFileToUpload(1024 * 1024 * fileSize, false);
            var uploadState = new ActiveUploadState(new UploadSpecification { MaxNumberOfThreads = serverMaxThreadsd }, 0);
            uploadState.UploadSpecification.ChunkUri = new Uri("http://localhost/");
            var uploadSpec = new UploadSpecificationRequest(file.Name, file.Length, testFolder.url);
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

        [TestCase(0, 0, 1)]
        [TestCase(0, 4, 1)]
        [TestCase(4, 2, 2)]
        [TestCase(4, 8, 4)]
        [TestCase(4, 0, 1)]
        public async void AsyncScalingFileUploader_LimitThreads(int max, int specified, int expected)
        {
            // Arrange
            var uploader = SetupUploader(max, specified, 10);

            // Act
            await uploader.PrepareAsync();

            // Assert
            uploader.PartUploader.NumberOfThreads.Should().Be(expected);
        }

        [TestCase(1, ExpectedResult = 1)]
        [TestCase(7, ExpectedResult = 1)]
        [TestCase(8, ExpectedResult = 2)]
        [TestCase(15, ExpectedResult = 2)]
        [TestCase(16, ExpectedResult = 3)]
        [TestCase(24, ExpectedResult = 4)]
        [TestCase(50, ExpectedResult = 4)]
        public async Task<int> AsyncScalingFileUploader_LimitThreadsBasedOnFileSize(int fileSize)
        {
            // Arrange
            var uploader = SetupUploader(4, 4, fileSize);

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

        [TestCase(0, 0, 1)]
        [TestCase(0, 4, 1)]
        [TestCase(4, 2, 2)]
        [TestCase(4, 8, 4)]
        [TestCase(4, 0, 1)]
        public void ScalingFileUploader_ChangeNumberOfThreads(int max, int specified, int expected)
        {
            // Arrange
            var client = (ShareFileClient)GetShareFileClient();
            var testFolder = new Folder { Name = RandomString(30) + ".txt" };
            testFolder.url = new Uri("https://sf-api.com/sf/v3/Items(parent)");
            var file = GetFileToUpload(1024 * 1024 * 10, false);
            var uploadState = new ActiveUploadState(new UploadSpecification { MaxNumberOfThreads = max }, 0);
            var uploadSpec = new UploadSpecificationRequest(file.Name, file.Length, testFolder.url);
            var config = new FileUploaderConfig { NumberOfThreads = specified };
            var uploader = new ScalingFileUploader(client, uploadState, uploadSpec, file, config);

            // Act
            // The upload part is not fully setup, but we don't need to get that far, so let it throw
            Assert.Throws<NullReferenceException>(() => uploader.Upload());

            // Assert
            uploader.PartUploader.NumberOfThreads.Should().Be(expected);
        }

        private string GetNonAsciiFilename()
        {
            return @"nonascii_貴社ますますご盛栄のこととお慶び申し上げます。平素は格別のご高配を賜り、厚く御礼申し上げます。.txt";
        }
    } 
}
