using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using ShareFile.Api.Client.Security.Cryptography;

namespace ShareFile.Api.Client.Security
{
    public class ClientHashAuthentication : CustomAuthentication
    {
        public string ClientId { get; private set; }
        public string ClientSecret { get; private set; }
        public string UserId { get; set; }
        private ShareFileClient Client { get; set; }

        public ClientHashAuthentication(IShareFileClient client, string oauthClientId, string oauthClientSecret)
        {
            ClientId = oauthClientId;
            ClientSecret = oauthClientSecret;
            Client = (ShareFileClient)client;
        }

        public ClientHashAuthentication(IShareFileClient client, string oauthClientId, string oauthClientSecret, string userId)
            : this(client, oauthClientId, oauthClientSecret)
        {
            UserId = userId;
        }

        public string Hash(object body)
        {
            var bodyString = GetRequestBody(body);
            var secret = Convert.FromBase64String(ClientSecret);
            var hmac = HmacSha256ProviderFactory.GetProvider(secret);
            var hash = hmac.ComputeHash(new UTF8Encoding().GetBytes(bodyString));

            return Convert.ToBase64String(hash);
        }

        public bool HasUserId
        {
            get
            {
                return !string.IsNullOrWhiteSpace(UserId);   
            }
        }

        public override Uri SignUri(Uri uri)
        {
            return uri;
        }

        public override HttpRequestMessage SignBody(object body, HttpRequestMessage requestMessage)
        {
            var requestHash = Hash(body);
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("SFClientHash", requestHash);
            requestMessage.Headers.Add("X-SFAPI-ClientId", requestHash);

            if (HasUserId)
            {
                requestMessage.Headers.Add("X-SFAPI-UserId", UserId);
            }

            return requestMessage;
        }

        private string GetRequestBody(object body)
        {
            if (body is string)
            {
                return (string) body;
            }

            using (var stringWriter = new StringWriter())
            using (var textWriter = new JsonTextWriter(stringWriter))
            {
                Client.Serializer.Serialize(textWriter, body);
                return textWriter.ToString();
            }
        }
    }
}
