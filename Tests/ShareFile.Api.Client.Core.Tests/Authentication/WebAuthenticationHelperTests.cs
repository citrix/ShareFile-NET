using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using ShareFile.Api.Client.Security.Authentication;
using ShareFile.Api.Client.Security.Authentication.OAuth2;

namespace ShareFile.Api.Client.Core.Tests.Authentication
{
    [TestFixture]
    public class WebAuthenticationHelperTests
    {
        protected Uri GetOAuthCompleteUri()
        {
            return new Uri("https://secure.sharefile.com/oauth/oauthcomplete.aspx");
        }

        protected Uri GetOAuthCompleteUriWithParameters(Dictionary<string, string> dictionary)
        {
            var baseOAuthComplete = GetOAuthCompleteUri();

            var sb = new StringBuilder(baseOAuthComplete + "?");
            foreach (var kvp in dictionary)
            {
                sb.AppendFormat("{0}={1}&", kvp.Key, kvp.Value);
            }
            return new Uri(sb.ToString().TrimEnd(new char[] { '&' }));
        }

        protected List<Uri> GetNavigationUris(int count = 3)
        {
            var list = new List<Uri>();

            for (int i = 0; i < count; i++)
            {
                list.Add(new Uri(string.Format("https://secure.sharefile.com/oauth/oauthtest{0}.aspx", i)));
            }

            return list;
        }

        [Test]
        public void GetWebAuthenticationResponse_Success()
        {
            //Arrange
            var authHelper = new WebAuthenticationHelper(GetOAuthCompleteUri());
            var list = GetNavigationUris();
            
            var dictionary = new Dictionary<string, string>();
            dictionary.Add("test", "test123");
            dictionary.Add("test2", "test2123");

            list.Add(GetOAuthCompleteUriWithParameters(dictionary));

            //Act
            bool found = false;
            Dictionary<string,string> response = null;

            foreach (var uri in list)
            {
                if (authHelper.IsComplete(uri, out response))
                {
                    found = true;
                    break;
                }
            }

            found.Should().BeTrue();
            response.Should().ContainKey("test");
            response.Should().ContainKey("test2");
        }

        [Test]
        public void GetWebAuthenticationResponse_Fail()
        {
            //Arrange
            var authHelper = new WebAuthenticationHelper(GetOAuthCompleteUri());
            var list = GetNavigationUris(1);

            var dictionary = new Dictionary<string, string>();
            dictionary.Add("test", "test123");
            dictionary.Add("test2", "test2123");

            //Act
            bool found = false;
            Dictionary<string, string> response = null;

            foreach (var uri in list)
            {
                if (authHelper.IsComplete(uri, out response))
                {
                    found = true;
                    break;
                }
            }

            found.Should().BeFalse();
            (response == null).Should().BeTrue();
        }
    }
}
