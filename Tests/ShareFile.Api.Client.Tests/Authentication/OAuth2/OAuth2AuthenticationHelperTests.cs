using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using ShareFile.Api.Client.Security.Authentication.OAuth2;

namespace ShareFile.Api.Client.Core.Tests.Authentication.OAuth2
{
    [TestFixture]
    public class OAuth2AuthenticationHelperTests : WebAuthenticationHelperTests
    {
        [TestCase(true, "code", "state", typeof(OAuthAuthorizationCode), ExpectedResult = true, TestName = "GetAuthorizationCode_Success")]
        [TestCase(false, "code", "state", typeof(OAuthAuthorizationCode), ExpectedResult = false, TestName = "GetAuthorizationCode_Fail")]
        [TestCase(true, "access_token", "refresh_token", typeof(OAuthToken), ExpectedResult = true, TestName = "GetOAuthToken_Success")]
        [TestCase(false, "access_token", "refresh_token", typeof(OAuthToken), ExpectedResult = false, TestName = "GetOAuthToken_Fail")]
        [TestCase(true, "error", "error_description", typeof(OAuthError), ExpectedResult = true, TestName = "GetOAuthError_Success")]
        [TestCase(false, "error", "error_description", typeof(OAuthError), ExpectedResult = false, TestName = "GetOAuthError_Fail")]
        [TestCase(true, "random1", "random2", typeof(OAuthResponseBase), ExpectedResult = true, TestName = "OAuthResponseBase_Success")]
        [TestCase(false, "random1", "random2", typeof(OAuthResponseBase), ExpectedResult = false, TestName = "OAuthResponseBase_Fail")]
        public bool GetOAuthResponse(bool includeCompleteUri, string key1, string key2, Type expectedType)
        {
            //Arrange
            var oauth2AuthHelper = new OAuth2AuthenticationHelper(GetOAuthCompleteUri());
            var list = GetNavigationUris();
            
            var dictionary = new Dictionary<string, string>();
            dictionary.Add(key1, key1 + 123);
            dictionary.Add(key2, key2 + 123);

            if (includeCompleteUri)
            {
                list.Add(GetOAuthCompleteUriWithParameters(dictionary));
            }

            //Act
            bool found = false;
            IOAuthResponse response = null;

            foreach (var uri in list)
            {
                if (oauth2AuthHelper.IsComplete(uri, out response))
                {
                    found = true;
                    break;
                }
            }

            return found && (response != null && response.GetType() == expectedType);
        }

        [Test]
        public void OAuthResponseBase_PropertiesMapped()
        {
            //Arrange
            var oauth2AuthHelper = new OAuth2AuthenticationHelper(GetOAuthCompleteUri());
            var list = GetNavigationUris(1);

            var dictionary = new Dictionary<string, string>();
            dictionary.Add("appcp", "sharefile.com");
            dictionary.Add("apicp", "sf-api.com");
            dictionary.Add("randomKey", "randomValue");

            list.Add(GetOAuthCompleteUriWithParameters(dictionary));

            //Act
            bool found = false;
            IOAuthResponse response = null;

            foreach (var uri in list)
            {
                if (oauth2AuthHelper.IsComplete(uri, out response))
                {
                    found = true;
                    break;
                }
            }

            found.Should().BeTrue();
            response.GetType().Should().Be(typeof (OAuthResponseBase));
            response.Properties.Should().NotBeNull();
            var oauthResponseBase = response as OAuthResponseBase;
            oauthResponseBase.ApiControlPlane.Should().Be("sf-api.com");
            oauthResponseBase.ApplicationControlPlane.Should().Be("sharefile.com");
            oauthResponseBase.Properties.Should().ContainKey("randomKey");
        }
    }
}
