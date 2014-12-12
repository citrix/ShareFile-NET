using System.Collections.Generic;

namespace ShareFile.Api.Client.Security.Authentication.OAuth2
{
    public interface IOAuthResponse
    {
        void Fill(IDictionary<string, string> values);
        IDictionary<string, string> Properties { get; }
    }
}
