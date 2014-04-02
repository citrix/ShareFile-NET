using System.Collections.Generic;

namespace ShareFile.Api.Client.Security.Authentication.OAuth2
{
    public class OAuthAuthorizationCode : OAuthResponseBase
    {
        public string Code { get; set; }
        public string State { get; set; }

        public override void Fill(IDictionary<string, string> values)
        {
            base.Fill(values);
            Code = values.ContainsKey("code") ? values["code"] : "";
            State = values.ContainsKey("state") ? values["state"] : "";
        }

        public static OAuthAuthorizationCode CreateFromDictionary(IDictionary<string, string> values)
        {
            var authorizationCode = new OAuthAuthorizationCode();
            authorizationCode.Fill(values);
            return authorizationCode;
        }
    }
}
