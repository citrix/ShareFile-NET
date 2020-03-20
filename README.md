# ShareFile Client SDK Documentation #

Before continuing please familiarize yourself with the API and it's methodology
at https://api.sharefile.com/rest

If you would like to download the Nuget Package for this SDK, you can find it here
https://www.nuget.org/packages/ShareFile.Api.Client/

License
----
All code is licensed under the [MIT
License](https://github.com/citrix/ShareFile-PowerShell/blob/master/ShareFileSnapIn/LICENSE.txt).

## Tooling requirements ##
* Building `ShareFile.Api.Client.DotNet.sln` requires Visual Studio 2017 with .NET Core tooling.

## Definitions ##

* `applicationControlPlane` - Describes the domain that the ShareFile account is available on.  
For example: `sharefile.com`, `securevdr.com`, `sharefile.eu`, etc.
* `authorizationUrl` - The initial url that should be visited to being web authentication.
* `client_id` - The identifier that is uniquely identifies an OAuth client consumer.
* `client_secret` - This is a shared secret that is required to exchange an `OAuthAuthorizationCode` for an `OAuthToken`.
* `completionUri` - Alias for `redirectUri`.  Used primarily in `OAuth2AuthenticationHelper`.
* `OAuthAuthorizationCode` - One-time use code that is returned as part of an oauth `code` grant request.  
We provide a class with the specific properties for this type of response.
* `OAuthToken` - Used to authenticate with ShareFile, specifically using AccessToken - however, this is taken care of for you by the SDK.
* `redirectUri` - Resource that can be used to track when authentication is complete.  Generally, this resource is controlled by the OAuth client consumer.
* `state` - Token created by the OAuth consumer to associate an authorization request
with an authorization response.

## Authentication ##

Authentication with ShareFile v3 API makes use of [OAuth 2.0 protocol](http://api.sharefile.com/rest/oauth2.aspx).
Some helper methods and classes are provided to make authentication easier for consumers.

* **Web Authentication**

        var redirectUri = new Uri("https://secure.sharefile.com/oauth/oauthcomplete.aspx");

        // Recommended this value is held on to to verify the authentication response.
        var state = Guid.NewGuid().ToString();

        var sfClient = new ShareFileClient("https://secure.sf-api.com/sf/v3/");
        var oauthService = new OAuthService(sfClient, "[client_id]", "[client_secret]");

        var authorizationUrl = oauthService.GetAuthorizationUrl("sharefile.com", "code", "clientId", redirectUri.ToString(),
                state);

  Open up a web browser control and use `authorizationUrl` to load the browser. To assist in tracking when authentication has completed,
  create an instance of `OAuth2AuthenticationHelper` using `redirectUri` as the `completionUri` in `OAuth2AuthenticationHelper.ctor()`.

        var authenticationHelper = new OAuth2AuthenticationHelper(redirectUri);

  In the the event raised by your browser control instance for Navigation occurring, check the `Uri` that is being loaded.

        WebBrowser.Navigating += (sender, args) =>
        {
           IOAuthResponse oauthResponse;
           if (authenticationHelper.IsComplete(args.Uri, out oauthResponse))
           {
              if (oauthResponse is OAuthError)
              {
                 // handle error
              }

              if (oauthResponse is OAuthAuthorizationCode)
              {
                 // exchange authorization code for OAuthToken
              }
           }
        };

  To exchange an `OAuthAuthorizationCode` for an `OAuthToken`:

        var oauthToken = await oauthService.
          ExchangeAuthorizationCodeAsync(oauthAuthorizationCodeInstance);

        sfClient.AddOAuthCredentials(oauthToken);
        sfClient.BaseUri = oauthToken.GetUri();

  ** Note ** - If you use your own mechanism for tracking when authentication
  is complete (based on redirectUri), it is still advisable to use `OAuth2AuthenticationHelper`
  to translate the `Uri` to `IOAuthResponse`

* **Password Authentication**: Requires the consumer perform ShareFile account discovery,
which is not currently documented.  In order to complete this authentication
the consumer will must know `username`, `password`, `subdomain`, and `applicationControlPlane`.  In the sample below,
these are assumed to have been obtained already.

        var sfClient = new ShareFileClient("https://secure.sf-api.com/sf/v3/");
        var oauthService = new OAuthService(sfClient, "[clientid]", "[clientSecret]");

        var oauthToken = await oauthService.PasswordGrantAsync(username,
          password, subdomain, applicationControlPlane);

        sfClient.AddOAuthCredentials(oauthToken);
        sfClient.BaseUri = oauthToken.GetUri();

* **SAML Authentication**:  This authentication support assumes you have a mechanism
for obtaining a SAML assertion, `samlAssertion` from the user's IdP.

        var sfClient = new ShareFileClient("https://secure.sf-api.com/sf/v3/");
        var oauthService = new OAuthService(sfClient, "[clientid]", "[clientSecret]");

        var oauthToken = await oauthService.ExchangeSamlAssertionAsync(samlAssertion,
          subdomain, applicationControlPlane);

        sfClient.AddOAuthCredentials(oauthToken);
        sfClient.BaseUri = oauthToken.GetUri();

* **Refreshing an OAuthToken**:  Any `OAuthToken` that is obtained using a `code`
grant type can be refreshed.  This allows a consumer to silently reauthenticate
with the ShareFile API without needing to prompt the user.  This is useful if
you plan on caching the `OAuthToken`.  The sample below assumes you have already
pulled an instance of `OAuthToken` as `cachedOAuthToken` from some local cache.

        var sfClient = new ShareFileClient(cachedOAuthToken.GetUri());
        var oauthService = new OAuthService(sfClient, "[clientid]", "[clientSecret]");

        var oauthToken = await oauthService.RefreshOAuthTokenAsync(samlAssertion,
          subdomain, applicationControlPlane);

        sfClient.AddOAuthCredentials(oauthToken);
        sfClient.BaseUri = oauthToken.GetUri();


## ShareFile Basics ##

Once authenticated, getting information from ShareFile is pretty easy.  
Below are some samples on what you can do, it assumes there is an instance of
ShareFileClient - `sfClient` available.

### Start a Session ###

      var session = await sfClient.Session.Login().ExecuteAsync();

### End session ###

      var session = await sfClient.Session.Logout().ExecuteAsync();

      // Should clean up credentials and cookies if you plan to
      // re-use the sfClient instance.
      sfClient.ClearCredentialsAndCookies();

### Get the current user ###

A User in ShareFile derives from the `Principal` object. For most consumers you
will be interested in `User` and `AccountUser`. The `AccountUser` type designates
the user to be an Employee and will have some additional properties available.  
You can also use `User.IsEmployee()`, to get the additional properties you will
still need to cast the instance to `AccountUser`

      var user = await sfClient.Users.Get().ExecuteAsync();

### Get the default folder for a User ###

This call will return the default folder for the currently authenticated `User`.

      var folder = (Folder) await sfClient.Items.Get().ExecuteAsync();

### Get the contents of a folder ###

      var folderContents = await sfClient.Items.GetChildren(folder.url).ExecuteAsync();

      // will have the contents an ODataFeed<Item>
      // the Feed property will be a List<Item>

      // operate over the feed
      folderContents.Feed

### Create a Folder ###

      var parentFolder = (Folder) await sfClient.Items.Get().ExecuteAsync();

      var newFolder = new Folder
      {
        Name = "New Folder 1"
      };

      newFolder = await sfClient.Items.CreateFolder(parentFolder.url, newFolder).ExecuteAsync();

### Search ###

      var searchResults = await sfClient.Items.Search("query").ExecuteAsync();

To browse search results (currently, there is no `Uri` returned that points to the `Item`):

      var itemUri = sfClient.Items.GetAlias(searchResult.ItemID);
      var item = await sfClient.Items.Get(itemUri).ExecuteAsync();

### Access Aliased Folders ###
There are some folders within ShareFile that are not easily discovered, however
the SDK can help you find them.  These aliases are exposed on an enum `ItemAlias`.

      var itemUri = sfClient.Items.GetAlias(ItemAlias.Top);
      var item = await sfClient.Items.Get(itemUri).ExecuteAsync();

## Upload/Download ##

### Download ###

      var downloader = sfClient.GetAsyncFileDownloader(itemToDownload);
      using(var fileStream = File.Open(@"C:\test\newImage.png", FileMode.OpenOrCreate))
	  {
        await downloader.DownloadToAsync(fileStream);
	  }

### Upload ###

      var parentFolder = (Folder) await sfClient.Items.Get().ExecuteAsync();
      using(var file = File.Open(@"C:\test\image.png", FileMode.OpenOrCreate))
	  {
        var uploadRequest = new UploadSpecificationRequest
        {
          FileName = file.Name,
          FileSize = file.Length,
          Details = "Sample details",
          Parent = parentFolder.url
        };

		var uploader = sfClient.GetAsyncFileUploader(uploadRequest, file);

		var uploadResponse = await uploader.UploadAsync();
	  }
	  
ShareFile supports a concept called `Shares` which are labeled as `Request` and `Send`.
If you have a `Request Share`, you can fulfill the request by uploading directly
to it via the SDK.

      // Assumes you have a Share object or at least a Uri to a Share
      using(var file = File.Open(@"C:\test\image.png", FileMode.OpenOrCreate))
	  {
        var uploadRequest = new UploadSpecificationRequest
        {
          FileName = file.Name,
          FileSize = file.Length,
          Details = "Sample details",
          Parent = shareUri
        };

        var uploader = sfClient.GetAsyncFileUploader(uploadRequest, file);

		var uploadResponse = await uploader.UploadAsync();
	  }
	  
### Get transfer progress ###

On any transfer you can be notified of progress.  On the instance of the uploader/downloader
you can provide a delegate to `OnTransferProgress`.

### Deleting Items
#### Delete a specific version of a file
```
await sfClient.Items.Delete(file.url).ExecuteAsync();
```
### Delete a folder
```
await sfClient.Items.Delete(folder.url).ExecuteAsync();
```
#### Delete all versions of a file
- Requires using `StreamId` instead of `Id` which is the default identifier used in `url`
- GetObjectUri is an extension method that will take care of building a correct `Uri`
```
using ShareFile.Client.Core.Extensions;
...
// Uses the StreamId
await sfClient.Items.Delete(file.GetObjectUri(true)).ExecuteAsync();
```

## Accessing a Share ##

Assuming you have the url that points to the Share API resource (ex. `https://subdomain.sharefile.com/sf/v3/Shares(s0123456789)`), you can easily access the `Items` shared.  Depending on the share you may be required to already be authenticated.

```
var shareUri = new Uri("https://subdomain.sharefile.com/sf/v3/Shares(s0123456789)");
var share = await sfClient.Shares.Get(shareUri);
var shareItems = await sfClient.Shares.GetItems(shareUri, share.AliasID);
```

Items associated with a `Share` cannot be downloaded as you normally might, instead you need to use the `Shares` API to download.

```
// assuming you already have shareItems as noted before

var fileStream = await sfClient.Shares.Download(shareUri, share.AliasID, shareItems.Select(x => x.Id).First());
```


## Leveraging oData ##

ShareFile supports the oData protocol which provides standard ways of handling
common tasks such as:

  * Select specific properties
  * Expand Navigation properties such as `Folder.Children`
  * Perform paging operations

### Select ###

The following `Query` will only select the Name property.  If you execute this,
all other properties will be their default values.  This is convenient for
reducing payloads on the wire.

      var folder = (Folder) await sfClient.Items.Get()
                    .Select("Name")
                    .ExecuteAsync();

### Expand ###

The following `Query` will expand `Children`.  Since we know we are querying for
a `Folder` we can ask ShareFile to go ahead and return the list of Children.  This
helps reduce the number of round trips required.  Note `Chlidren` is presented
as a `List<Item>` instead of an `ODataFeed<Item>`.

      var folder = (Folder) await sfClient.Items.Get()
                    .Expand("Children")
                    .ExecuteAsync();

      // Is now populated.
      folder.Children

### Top/Skip ###

When working with `ODataFeed` responses, you can limit the size of the response
by using `Top` and `Skip`.  The following `Query` will return up to 10 Children
and skip the first 10.

      var folderContents = await sfClient.Items.GetChildren()
                            .Top(10)
                            .Skip(10)
                            .ExecuteAsync();

To support paging `ODataFeed` will also return a nextLink which will compute the
Top and Skip values for you.
