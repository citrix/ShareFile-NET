using System;
using FluentAssertions;
using NUnit.Framework;
using ShareFile.Api.Client.Extensions;
using ShareFile.Api.Models;

namespace ShareFile.Api.Client.Core.Tests.Extensions
{
    [TestFixture]
    public class ODataObjectExtensionsTests : BaseTests
    {
        [Test]
        public void GetObjectUri_WithStream()
        {
            // Arrange
            var folderId = GetId();
            var folderStreamId = "st" + GetId().Substring(2);
            var folder = new Folder
            {
                Id = folderId,
                url = new Uri(BaseUriString + "Items(" + folderId + ")"),
                StreamID = folderStreamId
            };

            // Act
            var streamObjectUri = folder.GetObjectUri(true);

            // Assert
            streamObjectUri.ToString().Should().Be(BaseUriString + "Items(" + folderStreamId + ")");
        }

        [Test]
        public void GetObjectUri_WithNullStream()
        {
            // Arrange
            var folderId = GetId();
            var folder = new Folder
            {
                Id = folderId,
                url = new Uri(BaseUriString + "Items(" + folderId + ")")
            };

            // Act
            var streamObjectUri = folder.GetObjectUri(true);

            // Assert
            streamObjectUri.ToString().Should().Be(BaseUriString + "Items(" + folderId + ")");
        }

        [Test]
        public void GetObjectUri_WithId()
        {
            // Arrange
            var folderId = GetId();
            var folder = new Folder
            {
                Id = folderId,
                url = new Uri(BaseUriString + "Items(" + folderId + ")"),
            };

            // Act
            var streamObjectUri = folder.GetObjectUri();

            // Assert
            streamObjectUri.ToString().Should().Be(BaseUriString + "Items(" + folderId + ")");
        }
    }
}
