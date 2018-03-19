﻿using NUnit.Framework;
using ShareFile.Api.Client.Models;
using ShareFile.Api.Client.Tests.Transfers;
using ShareFile.Api.Client.Transfers;
using ShareFile.Api.Client.Transfers.Downloaders;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ShareFile.Api.Client.Core.Tests
{
    [TestFixture(Category = "Transfers")]
    public class DownloaderTests : TransferBaseTests
    {
        private IShareFileClient shareFileClient;
        private ShareFile.Api.Client.Models.Folder testFolder;
        private Models.Item file;

        [SetUp]
        public void Setup()
        {
            shareFileClient = GetShareFileClient();
            var rootFolder = shareFileClient.Items.Get().Execute();
            testFolder = new ShareFile.Api.Client.Models.Folder { Name = RandomString(30) };
            testFolder = shareFileClient.Items.CreateFolder(rootFolder.url, testFolder).Execute();

            var fileToUpload = GetFileToUpload(4 * 1024 * 1024, false);
            var uploadSpec = new UploadSpecificationRequest
            {
                FileName = "name.png",
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
        
        [Test]
        public async Task DownloadRangeRequest_Async_File_OverrideRangeRequest()
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
        public async Task DownloadRangeRequest_Async_DownloadSpec_OverrideRangeRequest()
        {
            // Arrange
            var downloadSpecification = shareFileClient.Items.Download(file.url, false).Expect<DownloadSpecification>().Execute();
            var downloader = shareFileClient.GetAsyncFileDownloader(downloadSpecification);
            var destinationStream = new MemoryStream(4 * 1024 * 1024);
            var rangeRequests = Enumerable.Range(1, 4).Select(x => new RangeRequest
            {
                Begin = (x - 1) * 1024 * 1024,
                End = x * 1024 * 1024
            });

            foreach (var rangeRequest in rangeRequests)
            {
                // Act
                await downloader.DownloadToAsync(destinationStream, rangeRequest: rangeRequest);

                // Assert
                Assert.GreaterOrEqual(destinationStream.Position, rangeRequest.End.GetValueOrDefault());
            }
        }

        [Test]
        public async Task DownloadRangeRequest_Async_File_ConfigRangeRequest()
        {
            // Arrange
            var destinationStream = new MemoryStream(4 * 1024 * 1024);
            var rangeRequests = Enumerable.Range(1, 4).Select(x => new RangeRequest
            {
                Begin = (x - 1) * 1024 * 1024,
                End = x * 1024 * 1024
            });
            
            foreach (var rangeRequest in rangeRequests)
            {
                var downloader = shareFileClient.GetAsyncFileDownloader(file, new DownloaderConfig
                {
                    RangeRequest = rangeRequest
                });

                // Act
                await downloader.DownloadToAsync(destinationStream);

                // Assert
                Assert.GreaterOrEqual(destinationStream.Position, rangeRequest.End.GetValueOrDefault());
            }
        }

        [Test]
        public async Task DownloadRangeRequest_Async_DownloadSpec_ConfigRangeRequest()
        {
            // Arrange
            var destinationStream = new MemoryStream(4 * 1024 * 1024);
            var rangeRequests = Enumerable.Range(1, 4).Select(x => new RangeRequest
            {
                Begin = (x - 1) * 1024 * 1024,
                End = x * 1024 * 1024
            });

            var downloadSpecification = shareFileClient.Items.Download(file.url, false).Expect<DownloadSpecification>().Execute();
            foreach (var rangeRequest in rangeRequests)
            {
                var downloader = shareFileClient.GetAsyncFileDownloader(downloadSpecification, new DownloaderConfig
                {
                    RangeRequest = rangeRequest
                });

                // Act
                await downloader.DownloadToAsync(destinationStream);

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
            Assert.ThrowsAsync<InvalidOperationException>(() => downloader.DownloadToAsync(destinationStream, rangeRequest: rangeRequest));
        }
        
        [Test]
        public void DownloadRangeRequest_Sync()
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
