using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using ShareFile.Api.Client.Extensions;
using ShareFile.Api.Client.Models;

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

        [TestCase("Items(id)", "123", "Items(id)?root=123")]
        [TestCase("Items(id)?test=123", "123", "Items(id)?test=123&root=123")]
        [TestCase("Items(id)?test=123&", "123", "Items(id)?test=123&root=123")]
        [TestCase("Items(id)?root=123", "123", "Items(id)?root=123")]
        [TestCase("Items(id)", null, "Items(id)")]
        [TestCase("Items(id)?test=123", null, "Items(id)?test=123")]
        [TestCase("Items(id)?root=123", null, "Items(id)?root=123")]
        public void RedirectionUri(string uriPath, string rootParameter, string expectedUri)
        {
            var redirection = new Redirection { Uri = new Uri(BaseUriString + uriPath), Root = rootParameter };

            var uri = redirection.GetCalculatedUri();

            uri.ToString().Should().Be(BaseUriString + expectedUri);
        }

        public void SupportsUploadWithRequestParams_Null_False()
        {
            // Arrange
            IEnumerable<Capability> capabilities = null;

            // Act
            var res = capabilities.SupportsUploadWithRequestParams();

            // Assert
            res.Should().BeFalse();
        }

        public void SupportsUploadWithRequestParams_Empty_False()
        {
            // Arrange
            IEnumerable<Capability> capabilities = new List<Capability>();

            // Act
            var res = capabilities.SupportsUploadWithRequestParams();

            // Assert
            res.Should().BeFalse();
        }

        public void SupportsUploadWithRequestParams_False()
        {
            // Arrange
            IEnumerable<Capability> capabilities = new List<Capability>
            {
               new Capability { Name = CapabilityName.ItemDescription },
               new Capability { Name = CapabilityName.Search }
            };

            // Act
            var res = capabilities.SupportsUploadWithRequestParams();

            // Assert
            res.Should().BeFalse();
        }

        public void SupportsUploadWithRequestParams_True()
        {
            // Arrange
            IEnumerable<Capability> capabilities = new List<Capability>
            {
               new Capability { Name = CapabilityName.ItemDescription },
               new Capability { Name = CapabilityName.Search },
               new Capability { Name = CapabilityName.UploadWithRequestParams }
            };

            // Act
            var res = capabilities.SupportsUploadWithRequestParams();

            // Assert
            res.Should().BeFalse();
        }

        public void SupportsDownloadWithSpecificaton_Null_False()
        {
            // Arrange
            IEnumerable<Capability> capabilities = null;

            // Act
            var res = capabilities.SupportsDownloadWithSpecificaton();

            // Assert
            res.Should().BeFalse();
        }

        public void SupportsDownloadWithSpecificaton_Empty_False()
        {
            // Arrange
            IEnumerable<Capability> capabilities = new List<Capability>();

            // Act
            var res = capabilities.SupportsDownloadWithSpecificaton();

            // Assert
            res.Should().BeFalse();
        }

        public void SupportsDownloadWithSpecificaton_False()
        {
            // Arrange
            IEnumerable<Capability> capabilities = new List<Capability>
            {
               new Capability { Name = CapabilityName.ItemDescription },
               new Capability { Name = CapabilityName.Search }
            };

            // Act
            var res = capabilities.SupportsDownloadWithSpecificaton();

            // Assert
            res.Should().BeFalse();
        }

        public void SupportsDownloadWithSpecificaton_True()
        {
            // Arrange
            IEnumerable<Capability> capabilities = new List<Capability>
            {
               new Capability { Name = CapabilityName.ItemDescription },
               new Capability { Name = CapabilityName.Search },
               new Capability { Name = CapabilityName.DownloadSpecification }
            };

            // Act
            var res = capabilities.SupportsDownloadWithSpecificaton();

            // Assert
            res.Should().BeFalse();
        }
    }
}
