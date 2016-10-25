using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShareFile.Api.Client.Core.Tests
{
    public class UserInfo
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Subdomain { get; set; }
        public string Domain { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }

        public Uri GetBaseUri()
        {
            return new Uri(string.Format("https://{0}.{1}", Subdomain, Domain));
        }
    }
}
