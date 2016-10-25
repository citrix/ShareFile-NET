using System;

using NUnit.Framework;
using ShareFile.Api.Client.Requests;
using FakeItEasy;

using FluentAssertions;

namespace ShareFile.Api.Client.Core.Tests.Requests
{
    [TestFixture]
    public class QueryTests : BaseTests
    {
        [Test]
        public void WithBaseUri()
        {
            // Arrange
            var client = A.Fake<IShareFileClient>();
            var query = new Query(client);

            // Act
            query.WithBaseUri(new Uri("https://secure.sf-api.com/sf/v3"));

            // Assert
            query.GetBaseUri().Should().Be(new Uri("https://secure.sf-api.com/sf/v3/"));
        }
    }
}
