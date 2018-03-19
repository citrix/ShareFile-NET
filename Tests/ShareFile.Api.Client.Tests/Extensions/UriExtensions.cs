using System;
using System.Linq;
using ShareFile.Api.Client.Extensions;
using FluentAssertions;
using NUnit.Framework;

namespace ShareFile.Api.Client.Tests.Extensions
{
	[TestFixture]
	public class UriExtensions
	{
		[Test]
		public void GetQueryAsODataParameters_ValueEndsWithEqual_IsRetained()
		{
			// Arrange
			var uri = new Uri("https://www.google.com?testValue=value=");

			// Act
			var parameters = uri.GetQueryAsODataParameters().ToList();

			// Assert
			parameters.Count().Should().Be(1);
			parameters.First().Key.Should().Be("testValue");
			parameters.First().Value.Should().Be("value=");
		}

		[Test]
		public void GetQueryAsODataParameters_ValueEndsWithEncodedEqual_IsRetained()
		{
			// Arrange
			var uri = new Uri("https://www.google.com?testValue=value%3D");

			// Act
			var parameters = uri.GetQueryAsODataParameters().ToList();

			// Assert
			parameters.Count().Should().Be(1);
			parameters.First().Key.Should().Be("testValue");
			parameters.First().Value.Should().Be("value=");
		}

        [Test]
        public void GetQueryAsODataParameters_ValueContainsEqual_IsRetained()
        {
            var uri = new Uri("https://www.google.com?testValue=val=ue");

            var parameters = uri.GetQueryAsODataParameters().ToList();

            parameters.Count().Should().Be(1);
            parameters.First().Key.Should().Be("testValue");
            parameters.First().Value.Should().Be("val=ue");
        }

        [Test]
        public void GetQueryAsODataParameters_ValueContainsEncodedEqual_IsRetained()
        {
            var uri = new Uri("https://www.google.com?testValue=val%3Due");
            
            var parameters = uri.GetQueryAsODataParameters().ToList();
            
            parameters.Count().Should().Be(1);
            parameters.First().Key.Should().Be("testValue");
            parameters.First().Value.Should().Be("val=ue");
        }

        [Test]
        public void GetQueryAsODataParameters_ValueSeparator_NoValue_KeyRetained()
        {
            var uri = new Uri("https://www.google.com?testValue=");

            var parameters = uri.GetQueryAsODataParameters().ToList();

            parameters.Count().Should().Be(1);
            parameters.First().Key.Should().Be("testValue");
            parameters.First().Value.Should().Be("");
        }


        [Test]
        public void GetQueryAsODataParameters_NoValueSeparator_NoValue_KeyRetained()
        {
            var uri = new Uri("https://www.google.com?testValue");

            var parameters = uri.GetQueryAsODataParameters().ToList();

            parameters.Count().Should().Be(1);
            parameters.First().Key.Should().Be("testValue");
            parameters.First().Value.Should().Be("");
        }
    }
}
