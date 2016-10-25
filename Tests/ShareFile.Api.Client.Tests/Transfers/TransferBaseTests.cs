using Newtonsoft.Json;
using NUnit.Framework;
using ShareFile.Api.Client.Core.Tests;
using ShareFile.Api.Client.FileSystem;
using ShareFile.Api.Client.Security.Authentication.OAuth2;
using System;
using System.IO;
using System.Security.Cryptography;

namespace ShareFile.Api.Client.Tests.Transfers
{
    public abstract class TransferBaseTests : BaseTests
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

        private string GetNonAsciiFilename()
        {
            return @"nonascii_貴社ますますご盛栄のこととお慶び申し上げます。平素は格別のご高配を賜り、厚く御礼申し上げます。.txt";
        }
    }
}
