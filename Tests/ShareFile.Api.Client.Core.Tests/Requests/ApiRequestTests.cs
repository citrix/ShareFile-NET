﻿using System;
using FluentAssertions;
using NUnit.Framework;
using ShareFile.Api.Client.Requests;
using ShareFile.Api.Models;

namespace ShareFile.Api.Client.Core.Tests.Requests
{
    [TestFixture]
    public class ApiRequestTests
    {
        protected IShareFileClient GetShareFileClient()
        {
            return new ShareFileClient("https://release.sf-api.com/sf/v3/");
        }

        protected string GetId(int length = 36)
        {
            if (length > 36) length = 36;
            return Guid.NewGuid().ToString().Substring(0, length);
        }

        [Test]
        public void ApiRequest_FromQuery_WithId()
        {
            // Arrange
            var id = GetId();
            var query = new Query<ODataObject>(GetShareFileClient());
            query.From("Items")
                .Id(id)
                .Action("Download");

            // Act
            var apiRequest = ApiRequest.FromQuery(query);

            // Assert
            apiRequest.Body.Should().BeNull();
            apiRequest.HttpMethod.Should().Be("GET");
            var expectedUri = "https://release.sf-api.com/sf/v3/Items(" + id + ")/Download";
            apiRequest.GetComposedUri().ToString().Should().Be(expectedUri);
        }

        [Test]
        public void ApiRequest_FromQuery_WithUri()
        {
            // Arrange
            var id = GetId();
            var query = new Query<ODataObject>(GetShareFileClient());
            query.Uri(new Uri("https://release.sf-api.com/sf/v3/Items(" + id + ")"))
                .Action("Download");

            // Act
            var apiRequest = ApiRequest.FromQuery(query);

            // Assert
            apiRequest.Body.Should().BeNull();
            apiRequest.HttpMethod.Should().Be("GET");
            var expectedUri = "https://release.sf-api.com/sf/v3/Items(" + id + ")/Download";
            apiRequest.GetComposedUri().ToString().Should().Be(expectedUri);
        }

        [Test]
        public void ApiRequest_FromQuery_WithBody_AsPost()
        {
            // Arrange
            var id = GetId();
            var query = new Query<ODataObject>(GetShareFileClient());
            query.Uri(new Uri("https://release.sf-api.com/sf/v3/Items(" + id + ")"))
                .Action("CreateFolder");
            query.HttpMethod = "POST";
            query.Body = new Folder();

            // Act
            var apiRequest = ApiRequest.FromQuery(query);

            // Assert
            apiRequest.Body.Should().Be(query.Body);
            apiRequest.HttpMethod.Should().Be("POST");
            var expectedUri = "https://release.sf-api.com/sf/v3/Items(" + id + ")/CreateFolder";
            apiRequest.GetComposedUri().ToString().Should().Be(expectedUri);
        }

        [Test]
        public void ApiRequest_FromQuery_WithBody_AsGet()
        {
            // Arrange
            var id = GetId();
            var query = new Query<ODataObject>(GetShareFileClient());
            query.Uri(new Uri("https://release.sf-api.com/sf/v3/Items(" + id + ")"))
                .Action("CreateFolder");
            query.Body = new Folder();

            // Act
            var apiRequest = ApiRequest.FromQuery(query);

            // Assert
            apiRequest.Body.Should().Be(query.Body);
            apiRequest.HttpMethod.Should().NotBe("POST", "Just because body is defined, does not mean it should be a POST");
            var expectedUri = "https://release.sf-api.com/sf/v3/Items(" + id + ")/CreateFolder";
            apiRequest.GetComposedUri().ToString().Should().Be(expectedUri);
        }

        [Test]
        public void ApiRequest_FromQuery_WithQueryString()
        {
            // Arrange
            var id = GetId();
            var query = new Query<ODataObject>(GetShareFileClient());
            query.Uri(new Uri("https://release.sf-api.com/sf/v3/Items(" + id + ")"));
            query.QueryString("key1", "value1");
            query.QueryString("key2", "value2");

            // Act
            var apiRequest = ApiRequest.FromQuery(query);

            // Assert
            apiRequest.Body.Should().BeNull();
            var expectedUri = "https://release.sf-api.com/sf/v3/Items(" + id + ")?key1=value1&key2=value2";
            apiRequest.GetComposedUri().ToString().Should().Be(expectedUri);
        }

        [Test]
        public void ApiRequest_FromQuery_WithHeader()
        {
            // Arrange
            var id = GetId();
            var query = new Query<ODataObject>(GetShareFileClient());
            query.Uri(new Uri("https://release.sf-api.com/sf/v3/Items(" + id + ")"));
            query.AddHeader("key1", "value1");

            // Act
            var apiRequest = ApiRequest.FromQuery(query);

            // Assert
            apiRequest.Body.Should().BeNull();
            var expectedUri = "https://release.sf-api.com/sf/v3/Items(" + id + ")";
            apiRequest.GetComposedUri().ToString().Should().Be(expectedUri);
            apiRequest.HeaderCollection.Should().ContainKey("key1");
            apiRequest.HeaderCollection.Should().ContainValue("value1");
        }

        [Test]
        public void ApiRequest_FromQuery_CompositeIds()
        {
            // Arrange
            var id = GetId();
            var id2 = GetId();
            var query = new Query<ODataObject>(GetShareFileClient())
                .From("Items")
                .Ids("id", id)
                .Ids("id2", id2);

            // Act
            var apiRequest = ApiRequest.FromQuery(query);

            // Assert
            apiRequest.Body.Should().BeNull();
            var expectedUri = string.Format("https://release.sf-api.com/sf/v3/Items(id={0},id2={1})", id, id2);
            apiRequest.GetComposedUri().ToString().Should().Be(expectedUri);
        }

        [Test]
        public void ApiRequest_FromQuery_CompositeActionIds()
        {
            // Arrange
            var id = GetId();
            var id2 = GetId();
            var query = new Query<ODataObject>(GetShareFileClient())
                .From("Items")
                .Action("Test")
                .ActionIds("id", id)
                .ActionIds("id2", id2);

            // Act
            var apiRequest = ApiRequest.FromQuery(query);

            // Assert
            apiRequest.Body.Should().BeNull();
            var expectedUri = string.Format("https://release.sf-api.com/sf/v3/Items/Test(id={0},id2={1})", id, id2);
            apiRequest.GetComposedUri().ToString().Should().Be(expectedUri);
        }

        [Test]
        public void ApiRequest_FromQuery_CompositeSubActionIds()
        {
            // Arrange
            var id = GetId();
            var id2 = GetId();
            var subid = GetId(16);
            var subid2 = GetId(16);
            var query = new Query<ODataObject>(GetShareFileClient())
                .From("Items")
                .Action("Test")
                .ActionIds("id", id)
                .ActionIds("id2", id2)
                .SubAction("TestSubAction", "subid", subid)
                .SubAction("TestSubAction", "subid2", subid2);

            // Act
            var apiRequest = ApiRequest.FromQuery(query);

            // Assert
            apiRequest.Body.Should().BeNull();
            var expectedUri = string.Format("https://release.sf-api.com/sf/v3/Items/Test(id={0},id2={1})/TestSubAction(subid={2},subid2={3})", id, id2, subid, subid2);
            apiRequest.GetComposedUri().ToString().Should().Be(expectedUri);
        }
    }
}
