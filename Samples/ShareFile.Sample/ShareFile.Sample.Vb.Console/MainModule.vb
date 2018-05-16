Imports System.IO
Imports ShareFile.Api.Client
Imports ShareFile.Api.Client.Extensions
Imports ShareFile.Api.Client.Logging
Imports ShareFile.Api.Client.Models
Imports ShareFile.Api.Client.Security.Authentication.OAuth2
Imports ShareFile.Api.Client.Transfers

Module MainModule

    Structure SampleUser
        Public Username As String
        Public Password As String
        Public Subdomain As String
        Public ControlPlane As String
    End Structure

    Sub Main()
        Dim user = New SampleUser() With {
            .ControlPlane = "sharefile.com",
            .Username = "",
            .Password = "",
            .Subdomain = ""
        }

        Dim oauthClientId = ""
        Dim oauthClientSecret = ""

        If String.IsNullOrEmpty(oauthClientId) OrElse String.IsNullOrEmpty(oauthClientSecret) Then
            Console.WriteLine("You must provide oauthClientId and oauthClientSecret")
            Return
        End If

        If String.IsNullOrEmpty(user.Username) OrElse String.IsNullOrEmpty(user.Password) OrElse String.IsNullOrEmpty(user.Subdomain) Then
            Console.WriteLine("You must provide username, password and subdomain")
            Return
        End If

        RunSample(user, oauthClientId, oauthClientSecret).Wait()
    End Sub

    Public Async Function RunSample(user As SampleUser, clientId As String, clientSecret As String) As Task
        ' Authenticate with username/password
        Dim sfClient = Await PasswordAuthentication(user, clientId, clientSecret)

        ' Create a Session
        Await StartSession(sfClient)

        ' Load Folder and Contents
        Dim defaultUserFolder = Await LoadFolderAndChildren(sfClient)
        Console.WriteLine("Loaded - " + defaultUserFolder.Name)

        ' Create a Folder
        Dim createdFolder = Await CreateFolder(sfClient, defaultUserFolder)
        Console.WriteLine("Created a new folder - " + createdFolder.Name)

        ' Upload a file
        Dim uploadedFileId = Await Upload(sfClient, createdFolder)
        Dim itemUri = sfClient.Items.GetAlias(uploadedFileId)
        Dim uploadedFile = Await sfClient.Items.[Get](itemUri).ExecuteAsync()
        Console.WriteLine("Uploaded - " + uploadedFile.Name)

        ' Download a file
        Await Download(sfClient, uploadedFile)
        Console.WriteLine("Downloaded - " + uploadedFile.Name)

        ' Share a file using a Link
        Dim share = Await ShareViaLink(sfClient, uploadedFile)
        Console.WriteLine("Successfully created a share, it be be accessed using: " + share.Uri.ToString())

        ' Share a file via ShareFile
        Dim recipientEmailAddress As String = "[EnterEmailAddress]"
        Await ShareViaShareFileEmail(sfClient, uploadedFile, recipientEmailAddress)

        Console.ReadKey()
    End Function

    Public Async Function PasswordAuthentication(user As SampleUser, clientId As String, clientSecret As String) As Task(Of ShareFileClient)
        ' Initialize ShareFileClient.
        Dim configuration1 = Configuration.[Default]()
        configuration1.Logger = New DefaultLoggingProvider()

        Dim sfClient = New ShareFileClient("https://secure.sf-api.com/sf/v3/", configuration1)
        Dim oauthService = New OAuthService(sfClient, clientId, clientSecret)

        ' Perform a password grant request.  Will give us an OAuthToken
        Dim oauthToken = Await oauthService.PasswordGrantAsync(user.Username, user.Password, user.Subdomain, user.ControlPlane)

        ' Add credentials and update sfClient with new BaseUri
        sfClient.AddOAuthCredentials(oauthToken)
        sfClient.BaseUri = oauthToken.GetUri()

        Return sfClient
    End Function

    Public Async Function CreateFolder(sfClient As ShareFileClient, parentFolder As Folder) As Task(Of Folder)
        ' Create instance of the new folder we want to create.  Only a few properties 
        ' on folder can be defined, others will be ignored.
        Dim newFolder = New Folder() With {
            .Name = "Sample Folder",
            .Description = "Created by SF Client SDK"
        }

        Return Await sfClient.Items.CreateFolder(parentFolder.url, newFolder, overwrite:=True).ExecuteAsync()
    End Function

    Public Async Function Upload(sfClient As ShareFileClient, destinationFolder As Folder) As Task(Of String)
        Dim file = IO.File.Open("SampleFileUpload.txt", FileMode.OpenOrCreate)
        Dim uploadRequest = New UploadSpecificationRequest() With {
            .FileName = "SampleFileUpload.txt",
            .FileSize = file.Length,
            .Details = "Sample details",
            .Parent = destinationFolder.url
        }

        Dim uploader = sfClient.GetAsyncFileUploader(uploadRequest, file)

        Dim uploadResponse = Await uploader.UploadAsync()

        Return uploadResponse.First().Id
    End Function

    Public Async Function Download(sfClient As ShareFileClient, itemToDownload As Item) As Task
        Dim downloadDirectory = New DirectoryInfo("DownloadedFiles")
        If Not downloadDirectory.Exists Then
            downloadDirectory.Create()
        End If

        Dim downloader = sfClient.GetAsyncFileDownloader(itemToDownload)
        Dim file = IO.File.Open(Path.Combine(downloadDirectory.Name, itemToDownload.Name), FileMode.Create)

        Await downloader.DownloadToAsync(file)
    End Function

    Public Async Function StartSession(sfClient As ShareFileClient) As Task
        Dim session = Await sfClient.Sessions.Login().Expand("Principal").ExecuteAsync()

        Console.WriteLine("Authenticated as " + session.Principal.Email)
    End Function

    Public Async Function LoadFolderAndChildren(sfClient As ShareFileClient) As Task(Of Folder)
        Dim folder = DirectCast(Await sfClient.Items.[Get]().Expand("Children").ExecuteAsync(), Folder)

        Return folder
    End Function

    Public Async Function ShareViaLink(sfClient As ShareFileClient, fileToShare As Item) As Task(Of Share)
        Dim share = New Share() With {
            .Items = New List(Of Item)() From {
                fileToShare
            }
        }

        Return Await sfClient.Shares.Create(share).ExecuteAsync()
    End Function

    Public Async Function ShareViaShareFileEmail(sfClient As ShareFileClient, fileToShare As Item, recipientEmailAddress As String) As Task

        ' Allow unlimited downloads
        ' Expires in 10 days
        Dim sendShare = New ShareSendParams() With {
            .Emails = New List(Of String)() From {recipientEmailAddress},
            .Items = New List(Of String)() From {fileToShare.Id},
            .Subject = "Sample SDK Share",
            .MaxDownloads = -1,
            .ExpirationDays = 10
        }

        Await sfClient.Shares.CreateSend(sendShare).ExecuteAsync()

        Console.WriteLine("Sent email to: " + String.Join(", ", sendShare.Emails))
    End Function
End Module

