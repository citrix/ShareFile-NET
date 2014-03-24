using System;
using System.Threading.Tasks;

namespace ShareFile.Api.Client.Security.Authentication
{
    public interface IWebAuthentication<T>
        where T : WebAuthenticationResults
    {
        Action<NavigatingEventArgs> OnNavigating { get; set; }
        Uri StartUri { get; }
        Uri FinishUri { get; }
        Task<T> AuthenticateAsync();
        T Authenticate();
        void Initialize(Uri startUri, Uri finishUri);
    }
}
