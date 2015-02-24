using System.Diagnostics;
using System.Security.Cryptography;

using ShareFile.Api.Client.FileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Newtonsoft.Json;
using ShareFile.Api.Client.Security.Authentication.OAuth2;
using ShareFile.Api.Client.Transfers.Uploaders;
using ShareFile.Api.Models;
using ShareFile.Api.Client.Transfers;

namespace ShareFile.Api.Client.Core.Tests.Uploaders
{
    public class UploaderTests
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

        protected PlatformFileStream GetFileToUpload(int size)
        {
            var bytes = new byte[size];

            RandomNumberGenerator.Create().GetBytes(bytes);

            return new PlatformFileStream(new MemoryStream(bytes), (long)size, RandomString(20));
        }


        private Random Random = new Random();
        protected string RandomString(int length)
        {
            var builder = new StringBuilder();
            for (var i = 0; i < length; i++)
            {
                var ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * Random.NextDouble() + 65)));
                builder.Append(ch);
            }
            return builder.ToString();
        }

        [TestCase(UploadMethod.Standard, 1, true)]
        [TestCase(UploadMethod.Standard, 1, false)]
        [TestCase(UploadMethod.Threaded, 1, true)]
        [TestCase(UploadMethod.Threaded, 1, false)]
        [TestCase(UploadMethod.Standard, 4, true)]
        [TestCase(UploadMethod.Standard, 4, false)]
        [TestCase(UploadMethod.Threaded, 4, true)]
        [TestCase(UploadMethod.Threaded, 4, false)]
        public async void Upload(UploadMethod uploadMethod, int megabytes, bool useAsync)
        {
            var shareFileClient = GetShareFileClient();
            var rootFolder = shareFileClient.Items.Get().Execute();
            var testFolder = new Folder { Name = RandomString(30) + ".txt" };

            testFolder = shareFileClient.Items.CreateFolder(rootFolder.url, testFolder).Execute();
            var file = GetFileToUpload(1024 * 1024 * megabytes);
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
    }
}
