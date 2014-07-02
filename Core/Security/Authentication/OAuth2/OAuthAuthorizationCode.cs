using System.Collections.Generic;
using ShareFile.Api.Client.Extensions;

namespace ShareFile.Api.Client.Security.Authentication.OAuth2
{
    public class OAuthAuthorizationCode : OAuthResponseBase
    {
        public string Code { get; set; }
        public string State { get; set; }

        public override void Fill(IDictionary<string, string> values)
        {
            string value;
            if (values.TryRemoveValue("code", out value))
            {
                Code = value;
            }
            if (values.TryRemoveValue("state", out value))
            {
                State = value;
            }

            base.Fill(values);
        }

        public static OAuthAuthorizationCode CreateFromDictionary(IDictionary<string, string> values)
        {
            var authorizationCode = new OAuthAuthorizationCode();
            authorizationCode.Fill(values);
            return authorizationCode;
        }
    }
}
