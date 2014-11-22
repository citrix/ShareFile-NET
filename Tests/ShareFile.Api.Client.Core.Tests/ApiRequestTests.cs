using System;
using NUnit.Framework;
using ShareFile.Api.Client.Enums;
using ShareFile.Api.Client.Extensions;
using ShareFile.Api.Client.Requests;

namespace ShareFile.Api.Client.Core.Tests
{
    [TestFixture]
    public class ApiRequestTests : BaseTests
    {
        [TestCase("key=value")]
        public void UriWithQueryString(string kvp)
        {
            var shareFileClient = GetShareFileClient();
            var initialUri = shareFileClient.Items.GetAlias(ItemAlias.Root);
            var uriWithQueryString = new Uri(initialUri + "?" + kvp);
            var query = shareFileClient.Items.Get(uriWithQueryString)
                .Expand("Children")
                .Select("*")
                .QueryString("anotherKey", "anotherValue");

            var request = ApiRequest.FromQuery(query);
            var uri = request.GetComposedUri();

            Assert.IsTrue(uri.Query.Contains(kvp));
        }
    }
}
