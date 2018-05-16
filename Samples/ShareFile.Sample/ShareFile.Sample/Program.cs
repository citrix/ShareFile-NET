using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ShareFile.Api.Client;
using ShareFile.Api.Client.Extensions;
using ShareFile.Api.Client.Logging;
using ShareFile.Api.Client.Models;
using ShareFile.Api.Client.Security.Authentication.OAuth2;
using ShareFile.Api.Client.Transfers;

namespace ShareFile.Sample
{
    public struct SampleUser
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Subdomain { get; set; }
        public string ControlPlane { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var user = new SampleUser
            {
                ControlPlane = "sharefile.com",
                Username = "",
                Password = "",
                Subdomain = ""
            };

            string oauthClientId = "";
            string oauthClientSecret = "";

            if (string.IsNullOrEmpty(oauthClientId) || string.IsNullOrEmpty(oauthClientSecret))
            {
                Console.WriteLine("You must provide oauthClientId and oauthClientSecret");
                return;
            }

            if (string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Password) ||
                string.IsNullOrEmpty(user.Subdomain))
            {
                Console.WriteLine("You must provide username, password and subdomain");
                return;
            }

            RunSample(user, oauthClientId, oauthClientSecret).Wait();
        }

        public static async Task RunSample(SampleUser user, string clientId, string clientSecret)
        {
            // Authenticate with username/password
            var sfClient = await PasswordAuthentication(user, clientId, clientSecret);

            // Create a Session
            await StartSession(sfClient);

            // Load Folder and Contents
            var defaultUserFolder = await LoadFolderAndChildren(sfClient);
            Console.WriteLine("Loaded - " + defaultUserFolder.Name);

            // Create a Folder
            var createdFolder = await CreateFolder(sfClient, defaultUserFolder);
            Console.WriteLine("Created a new folder - " + createdFolder.Name);

            // Upload a file
            var uploadedFileId = await Upload(sfClient, createdFolder);
            var itemUri = sfClient.Items.GetAlias(uploadedFileId);
            var uploadedFile = await sfClient.Items.Get(itemUri).ExecuteAsync();
            Console.WriteLine("Uploaded - " + uploadedFile.Name);

            // Download a file
            await Download(sfClient, uploadedFile);
            Console.WriteLine("Downloaded - " + uploadedFile.Name);

            // Share a file using a Link
            var share = await ShareViaLink(sfClient, uploadedFile);
            Console.WriteLine("Successfully created a share, it be be accessed using: " + share.Uri);

            // Share a file via ShareFile
            string recipientEmailAddress = "[EnterEmailAddress]";
            await ShareViaShareFileEmail(sfClient, uploadedFile, recipientEmailAddress);

            Console.ReadKey();
        }

        public static async Task<ShareFileClient> PasswordAuthentication(SampleUser user, string clientId, string clientSecret)
        {
            // Initialize ShareFileClient.
            var configuration = Configuration.Default();
            configuration.Logger = new DefaultLoggingProvider();

            var sfClient = new ShareFileClient("https://secure.sf-api.com/sf/v3/", configuration);
            var oauthService = new OAuthService(sfClient, clientId, clientSecret);

            // Perform a password grant request.  Will give us an OAuthToken
            var oauthToken = await oauthService.PasswordGrantAsync(user.Username, user.Password, user.Subdomain, user.ControlPlane);

            // Add credentials and update sfClient with new BaseUri
            sfClient.AddOAuthCredentials(oauthToken);
            sfClient.BaseUri = oauthToken.GetUri();

            return sfClient;
        }

        public static async Task<Folder> CreateFolder(ShareFileClient sfClient, Folder parentFolder)
        {
            // Create instance of the new folder we want to create.  Only a few properties 
            // on folder can be defined, others will be ignored.
            var newFolder = new Folder
            {
                Name = "Sample Folder",
                Description = "Created by SF Client SDK"
            };

            return await sfClient.Items.CreateFolder(parentFolder.url, newFolder, overwrite: true).ExecuteAsync();
        }
        
        public static async Task<string> Upload(ShareFileClient sfClient, Folder destinationFolder)
        {
            var file = System.IO.File.Open("SampleFileUpload.txt", FileMode.OpenOrCreate);
            var uploadRequest = new UploadSpecificationRequest
            {
                FileName = "SampleFileUpload.txt",
                FileSize = file.Length,
                Details = "Sample details",
                Parent = destinationFolder.url
            };

            var uploader = sfClient.GetAsyncFileUploader(uploadRequest, file);

            var uploadResponse = await uploader.UploadAsync();

            return uploadResponse.First().Id;
        }

        public static async Task Download(ShareFileClient sfClient, Item itemToDownload)
        {
            var downloadDirectory = new DirectoryInfo("DownloadedFiles");
            if (!downloadDirectory.Exists)
            {
                downloadDirectory.Create();
            }

            var downloader = sfClient.GetAsyncFileDownloader(itemToDownload);
            var file = System.IO.File.Open(Path.Combine(downloadDirectory.Name, itemToDownload.Name), FileMode.Create);
            
            await downloader.DownloadToAsync(file);
        }

        public static async Task StartSession(ShareFileClient sfClient)
        {
            var session = await sfClient.Sessions.Login().Expand("Principal").ExecuteAsync();
            Console.WriteLine("Authenticated as " + session.Principal.Email);
        }

        public static async Task<Folder> LoadFolderAndChildren(ShareFileClient sfClient)
        {
            var folder = (Folder)await sfClient.Items.Get().Expand("Children").ExecuteAsync();

            return folder;
        }

        public static async Task<Share> ShareViaLink(ShareFileClient sfClient, Item fileToShare)
        {
            var share = new Share
            {
                Items = new List<Item>
                {
                    fileToShare
                }
            };

            return await sfClient.Shares.Create(share).ExecuteAsync();
        }

        public static async Task ShareViaShareFileEmail(ShareFileClient sfClient, Item fileToShare, string recipientEmailAddress)
        {
            var sendShare = new ShareSendParams
            {
                Emails = new[] { recipientEmailAddress },
                Items = new[] {fileToShare.Id},
                Subject = "Sample SDK Share",
                MaxDownloads = -1, // Allow unlimited downloads
                ExpirationDays = 10 // Expires in 10 days
            };

            await sfClient.Shares.CreateSend(sendShare).ExecuteAsync();

            Console.WriteLine("Sent email to: " + string.Join(", ", sendShare.Emails));
        }
    }
}
