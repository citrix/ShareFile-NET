using System;

using FluentAssertions;

using NUnit.Framework;

using ShareFile.Api.Client.Core.Tests;
using ShareFile.Api.Client.Transfers.Uploaders;

namespace ShareFile.Api.Client.Tests.Uploaders
{
    [TestFixture]
    public class CompleteBytesTests : BaseTests
    {
        [Test]
        public void Add_InOrder()
        {
            // Arrange
            var state = new CompletedBytes();

            // Act
            state.Add(0, 10);
            state.Add(10, 10);
            state.Add(20, 10);

            // Assert
            state.CompletedThroughPosition.Should().Be(30);
        }

        [Test]
        public void Add_SkipSomeBytes()
        {
            // Arrange
            var state = new CompletedBytes();

            // Act
            state.Add(0, 10);
            state.Add(40, 10);

            // Assert
            state.CompletedThroughPosition.Should().Be(10);
        }

        [Test]
        public void Add_NonZeroStart()
        {
            // Arrange
            var state = new CompletedBytes();

            // Act
            state.Add(40, 10);

            // Assert
            state.CompletedThroughPosition.Should().Be(0);
        }

        [Test]
        public void Add_OutOfOrderAndSkipped()
        {
            // Arrange
            var state = new CompletedBytes();

            // Act
            state.Add(0, 10);
            state.Add(40, 10);
            state.Add(20, 10);
            state.Add(10, 10);

            // Assert
            state.CompletedThroughPosition.Should().Be(30);
        }

        [Test]
        public void Add_OutOfOrder()
        {
            // Arrange
            var state = new CompletedBytes();

            // Act
            state.Add(0, 10);
            state.Add(30, 10);
            state.Add(20, 10);
            state.Add(10, 10);

            // Assert
            state.CompletedThroughPosition.Should().Be(40);
        }
    }
}
