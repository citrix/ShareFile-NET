using System;
using ShareFile.Api.Models;

namespace ShareFile.Api.Client.Security
{

    public class ZoneAuthentication
    {
#if !ShareFile
        public Zone Zone { get; set; }

        public string OpId { get; set; }

        public string UserId { get; set; }

        public ZoneAuthentication()
        {
        }

        public ZoneAuthentication(string zoneId, string zoneSecret, string opId = null, string userId = null)
        {
            OpId = opId;
            UserId = userId;

            Zone = new Models.Zone
            {
                Id = zoneId,
                Secret = zoneSecret
            };
        }

        public Uri Sign(Uri request)
        {
            string uriToHash = request.PathAndQuery;
            string uriCheck = uriToHash.ToLower();
            // strip anything after an existing &h parameter
            int hParamPos = uriCheck.IndexOf("&h=");
            if (hParamPos >= 0)
            {
                uriToHash = uriToHash.Substring(0, hParamPos);
            }
            // add a timestamp validation
            if (uriCheck.IndexOf("ht=") < 0)
            {
                uriToHash += string.IsNullOrEmpty(request.Query) ? "?" : "&";
                uriToHash += "ht=" + DateTime.Now.Ticks;
            }
            // add any missing authentication/impersonation parameters
            if (uriCheck.IndexOf("zoneid=") < 0) uriToHash += "&zoneid=" + Zone.Id;
            if (!string.IsNullOrEmpty(OpId) && uriCheck.IndexOf("opid=") < 0) uriToHash += "&opid=" + OpId;
            if (!string.IsNullOrEmpty(UserId) && uriCheck.IndexOf("zuid") < 0) uriToHash += "&zuid=" + UserId;

            byte[] secret = Convert.FromBase64String(Zone.Secret);
            System.Security.Cryptography.HMACSHA256 hmac = new System.Security.Cryptography.HMACSHA256(secret);
            byte[] hash = hmac.ComputeHash(new System.Text.UTF8Encoding().GetBytes(uriToHash));
            return new Uri(request.GetLeftPart(UriPartial.Authority) + uriToHash + "&h=" + Uri.EscapeDataString(Convert.ToBase64String(hash)));
        }
#endif
    }
}
