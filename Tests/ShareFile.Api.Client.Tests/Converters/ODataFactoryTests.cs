using FluentAssertions;
using NUnit.Framework;
using ShareFile.Api.Client.Converters;
using ShareFile.Api.Client.Core.Tests;
using ShareFile.Api.Client.Core.Tests.Converters;
using ShareFile.Api.Client.Models;

namespace ShareFile.Api.Client.Tests.Converters
{
    public class ODataFactoryTests : BaseTests
    {
        [Test]
        public void Create_WithPlatformNamespace()
        {
            // Act
            var instance = ODataFactory.GetInstance().Create("ShareFile.Api.Models.File");

            // Assert
            instance.Should().BeOfType<File>();
        }

        [Test]
        public void Create_WithLocalNamespace()
        {
            // Act
            var instance = ODataFactory.GetInstance().Create("ShareFile.Api.Client.Models.File");

            // Assert
            instance.Should().BeOfType<File>();
        }

        [Test]
        public void Create_WithNull()
        {
            // Act
            var instance = ODataFactory.GetInstance().Create(null);

            // Assert
            instance.Should().BeOfType<ODataObject>();
        }

        [Test]
        public void Create_NoNamespace()
        {
            // Act
            var instance = ODataFactory.GetInstance().Create("Note");

            // Assert
            instance.Should().BeOfType<Note>();
        }
    }
}
