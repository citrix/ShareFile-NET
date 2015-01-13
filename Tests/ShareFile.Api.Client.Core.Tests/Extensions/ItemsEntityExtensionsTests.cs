using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using ShareFile.Api.Client.Enums;
using ShareFile.Api.Client.Extensions;

namespace ShareFile.Api.Client.Core.Tests.Extensions
{
    [TestFixture]
    public class ItemsEntityExtensionsTests
    {
        [Test]
        public void GetAliasWithItemAlias()
        {
            // Arrange
            var alias = ItemAlias.Connectors;

            // Act
            var sfClient = new ShareFileClient("https://secure.sf-api.com/sf/v3/");
            var uri = sfClient.Items.GetAlias(alias);

            // Assert
            uri.ToString().Should().Be("https://secure.sf-api.com/sf/v3/Items(connectors)");
        }

        [TestCase(ItemAlias.SharepointConnectors, "c-sp", TestName = "GetAliasWithMappedAlias_SharePoint")]
        [TestCase(ItemAlias.NetworkShareConnectors, "c-cifs", TestName = "GetAliasWithMappedAlias_NetworkShares")]
        public void GetAliasWithMappedAlias(ItemAlias alias, string expectedValue)
        {
            // Act
            var sfClient = new ShareFileClient("https://secure.sf-api.com/sf/v3/");
            var uri = sfClient.Items.GetAlias(alias);

            // Assert
            uri.ToString().Should().Be("https://secure.sf-api.com/sf/v3/Items(" + expectedValue + ")");
        }

        [Test]
        public void GetAliasWithItemId()
        {
            // Arrange
            var alias = "randomId";

            // Act
            var sfClient = new ShareFileClient("https://secure.sf-api.com/sf/v3/");
            var uri = sfClient.Items.GetAlias(alias);

            // Assert
            uri.ToString().Should().Be("https://secure.sf-api.com/sf/v3/Items(randomId)");
        }
    }
}
