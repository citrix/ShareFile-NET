using System;
using System.Text;
using System.Linq;
using System.Net.Http;
using ShareFile.Api.Models;
using ShareFile.Api.Client.Security;
using ShareFile.Api.Client.Security.Cryptography;
using Newtonsoft.Json;
using NUnit.Framework;
using FluentAssertions;

namespace ShareFile.Api.Client.Core.Tests.Authentication.ClientHash
{
    [TestFixture]
    public class ClientHashAuthenticationTests: BaseTests
    {
        protected ClientHashAuthentication ClientHashAuthentication;
        protected InternalShareFileClient SfClient;
        protected UserInfo UserInfo;
        protected HttpRequestMessage TestRequest;
        protected string TestRequestBody;


        [TestFixtureSetUp]
        public void SetUp()
        {
            UserInfo = GetUserInfo();
            SfClient = new InternalShareFileClient(string.Format("{0}{1}.{2}{3}","https://", UserInfo.Subdomain, UserInfo.Domain, "/sf/v3/"));
            HmacSha256Provider.Register();
            ClientHashAuthentication = new ClientHashAuthentication(SfClient, UserInfo.ClientId, UserInfo.ClientSecret);
            SfClient.CustomAuthentication = ClientHashAuthentication;

            var url = UserInfo.GetBaseUri() + "/sf/v3/Items";
            TestRequestBody = JsonConvert.SerializeObject(new Item { Id = GetId()});
            TestRequest = GetTestRequest(url, TestRequestBody);
        }

        [Test]
        public void VerifyHashedRequestHeader_Authorization()
        {
            var hashedRequest = ClientHashAuthentication.SignBody(TestRequestBody, TestRequest);

            var clientHashHeader = hashedRequest.Headers.Authorization.ToString();
            clientHashHeader.Should().Be("SFClientHash " + Hash(TestRequest.RequestUri.ToString(), TestRequestBody, UserInfo.ClientSecret));
        }

        [Test]
        public void VerifyHashedRequesHeader_ClientId()
        {
            var hashedRequest = ClientHashAuthentication.SignBody(TestRequestBody, TestRequest);

            var clientIdHeader = hashedRequest.Headers.GetValues("X-SFAPI-ClientId").First();
            clientIdHeader.Should().Be(UserInfo.ClientId);
        }

        [Test]
        public void VerifyHashedRequesHeader_UserId()
        {
            var userId = GetId();
            ClientHashAuthentication.UserId = userId;

            var hashedRequest = ClientHashAuthentication.SignBody(TestRequestBody, TestRequest);

            var clientIdHeader = hashedRequest.Headers.GetValues("X-SFAPI-UserId").First();
            clientIdHeader.Should().Be(userId);
        }

        [Test]
        public void VerifyHashAuthentication()
        {
            var uri = SfClient.OAuthClients.GetEntityUriFromId(UserInfo.ClientId);
            var response = SfClient.OAuthClients.Get(uri).Execute();
            response.Should().BeOfType<OAuthClient>();            
        }

        private HttpRequestMessage GetTestRequest(string url, string body)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Content = new StringContent(body);

            return request;
        }

        /// <summary>
        /// Hash function used by ShareFileAuthenticationAttribute.cs to hash a request
        /// </summary>
        private static string Hash(string url, string body, string secret)
        {
            var sb = new StringBuilder();
            sb.Append(new Uri(url).PathAndQuery);
            if (body != null) sb.Append(body);
            var clientSecret = Encoding.ASCII.GetBytes(secret);
            System.Security.Cryptography.HMACSHA256 hmac = new System.Security.Cryptography.HMACSHA256(clientSecret);
            byte[] hash = hmac.ComputeHash(new UTF8Encoding().GetBytes(sb.ToString()));

            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }
}
