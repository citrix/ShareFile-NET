using System;
using System.Windows.Forms;
using ShareFile.Api.Client;
using ShareFile.Api.Client.Security.Authentication.OAuth2;

namespace ShareFile.Sample.WebAuthentication
{
    public partial class ShareFileWebAuthentication : Form
    {
        private readonly ShareFileClient _sfClient;
        private readonly OAuthService _oauthService;
        private readonly OAuth2AuthenticationHelper _authenticationHelper;
        private readonly Uri _completionUri = new Uri("https://secure.sharefile.com/oauth/oauthcomplete.aspx");
        private readonly string _oauthClientId;
        private readonly string _oauthClientSecret;
        
        public ShareFileWebAuthentication()
        {
            InitializeComponent();

            _sfClient = new ShareFileClient("https://secure.sf-api.com/sf/v3/");
            _oauthService = new OAuthService(_sfClient, _oauthClientId, _oauthClientSecret);
            _authenticationHelper = new OAuth2AuthenticationHelper(_completionUri);
        }

        /// <summary>
        /// Required for support SAML logins
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void webBrowser1_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            IOAuthResponse response;
            if (_authenticationHelper.IsComplete(e.Url, out response))
            {
                HandleOAuthResponse(response);
            }
        }

        /// <summary>
        /// Nice to bail out early if we can, for SAML you can't
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void webBrowser1_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            IOAuthResponse response;
            if (_authenticationHelper.IsComplete(e.Url, out response))
            {
                e.Cancel = true;
                HandleOAuthResponse(response);
            }
        }

        private void HandleOAuthResponse(IOAuthResponse response)
        {
            var error = response as OAuthError;
            if (error != null)
            {
                MessageBox.Show(error.ErrorDescription, "Authentication Failed", MessageBoxButtons.OK);
                return;
            }

            var authenticationCode = response as OAuthAuthorizationCode;
            if (authenticationCode != null)
            {
                ExchangeAuthorizationCode(authenticationCode);
            }
        }

        private async void ExchangeAuthorizationCode(OAuthAuthorizationCode code)
        {
            var oauthToken = await _oauthService.ExchangeAuthorizationCodeAsync(code);

            _sfClient.AddOAuthCredentials(oauthToken);
            _sfClient.BaseUri = oauthToken.GetUri();

            var session = await _sfClient.Sessions.Login().Expand("Principal").ExecuteAsync();

            MessageBox.Show("User: " + session.Principal.Email, "Successful");
        }

        private void startLoginBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_oauthClientId) || string.IsNullOrEmpty(_oauthClientSecret))
            {
                MessageBox.Show("You must provide oauthClientId and oauthClientSecret");
                return;
            }

            var authorizationUrl = _oauthService.GetAuthorizationUrl("sharefile.com", "code", _oauthService.ClientId,
                _completionUri.ToString(), "test");

            webBrowser1.Navigate(new Uri(authorizationUrl));
        }
    }
}
