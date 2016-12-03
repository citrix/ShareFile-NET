using NUnit.Framework;
using ShareFile.Api.Client.Tests.Transfers;
using ShareFile.Api.Client.Transfers;
using ShareFile.Api.Client.Transfers.Downloaders;
using System;
using System.IO;
using System.Linq;

namespace ShareFile.Api.Client.Core.Tests
{
    [TestFixture]
    public class DownloaderTests : TransferBaseTests
    {
        private IShareFileClient shareFileClient;
        private ShareFile.Api.Models.Folder testFolder;
        private Models.Item file;

        [SetUp]
        public void Setup()
        {
            shareFileClient = GetShareFileClient();
            var rootFolder = shareFileClient.Items.Get().Execute();
            testFolder = new ShareFile.Api.Models.Folder { Name = RandomString(30) };
            testFolder = shareFileClient.Items.CreateFolder(rootFolder.url, testFolder).Execute();

            var fileToUpload = GetFileToUpload(4 * 1024 * 1024, false);
            var uploadSpec = new UploadSpecificationRequest
            {
                FileName = fileToUpload.Name,
                FileSize = fileToUpload.Length,
                Parent = testFolder.url
            };

            var uploader = shareFileClient.GetFileUploader(uploadSpec, fileToUpload);
            UploadResponse uploadResponse = uploader.Upload();
            file = shareFileClient.Items.Get(shareFileClient.Items.GetEntityUriFromId(uploadResponse.First().Id)).Execute();
        }

        [TearDown]
        public void TearDown()
        {
            shareFileClient.Items.Delete(testFolder.url).Execute();
        }

        [TestCase(false)]
        [TestCase(true)]
        public async void DownloadRangeRequest_Async(bool supportsDLSpec)
        {
            // Arrange
            var downloader = shareFileClient.GetAsyncFileDownloader(file);
            var destinationStream = new MemoryStream(4 * 1024 * 1024);
            var rangeRequests = Enumerable.Range(1, 4).Select(x => new RangeRequest
            {
                Begin = (x - 1) * 1024 * 1024,
                End = x * 1024 * 1024
            });
            
            await downloader.PrepareDownloadAsync();

            foreach (var rangeRequest in rangeRequests)
            {
                // Act
                await downloader.DownloadToAsync(destinationStream, rangeRequest: rangeRequest);

                // Assert
                Assert.GreaterOrEqual(destinationStream.Position, rangeRequest.End.GetValueOrDefault());
            }
        }

        [Test]
        public void DownloadRangeRequest_Async_ThrowsIfNotPrepared()
        {
            // Arrange
            var downloader = shareFileClient.GetAsyncFileDownloader(file);
            var destinationStream = new MemoryStream(4 * 1024 * 1024);
            var rangeRequests = Enumerable.Range(1, 4).Select(x => new RangeRequest
            {
                Begin = (x - 1) * 1024 * 1024,
                End = x * 1024 * 1024
            });
            var rangeRequest = rangeRequests.First();

            // Act / Assert
            Assert.Throws<InvalidOperationException>(async () => await downloader.DownloadToAsync(destinationStream, rangeRequest: rangeRequest));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void DownloadRangeRequest_Sync(bool supportsDLSpec)
        {
            // Arrange
            var downloader = shareFileClient.GetFileDownloader(file);
            var destinationStream = new MemoryStream(4 * 1024 * 1024);
            var rangeRequests = Enumerable.Range(1, 4).Select(x => new RangeRequest
            {
                Begin = (x - 1) * 1024 * 1024,
                End = x * 1024 * 1024
            });

            downloader.PrepareDownload();

            foreach (var rangeRequest in rangeRequests)
            {
                // Act
                downloader.DownloadTo(destinationStream, rangeRequest: rangeRequest);

                // Assert
                Assert.GreaterOrEqual(destinationStream.Position, rangeRequest.End.GetValueOrDefault());
            }
        }

        [Test]
        public void DownloadRangeRequest_Sync_ThrowsIfNotPrepared()
        {
            // Arrange
            var downloader = shareFileClient.GetFileDownloader(file);
            var destinationStream = new MemoryStream(4 * 1024 * 1024);
            var rangeRequests = Enumerable.Range(1, 4).Select(x => new RangeRequest
            {
                Begin = (x - 1) * 1024 * 1024,
                End = x * 1024 * 1024
            });
            var rangeRequest = rangeRequests.First();

            // Act / Assert
            Assert.Throws<InvalidOperationException>(() => downloader.DownloadTo(destinationStream, rangeRequest: rangeRequest));
        }
    }
}
