using NUnit.Framework;

namespace ShareFile.Api.Client.Core.Tests.Requests.Filters
{
    using FluentAssertions;

    public class BinaryFilterTests
    {
        [Test]
        public void VerifyAndBinaryOperator()
        {
            // Arrange
            var leftHandSide = new Client.Requests.Filters.StartsWithFilter("Name", "test");
            var rightHandSide = new Client.Requests.Filters.EndsWithFilter("Name", "test");
            var andBinaryFilter = new Client.Requests.Filters.AndFilter(leftHandSide, rightHandSide);

            // Act
            var computedFilter = andBinaryFilter.ToString();
            var leftHandSideComputed = leftHandSide.ToString();
            var rightHandSideComputed = rightHandSide.ToString();

            // Assert
            computedFilter.Should().Be(leftHandSideComputed + " and " + rightHandSideComputed);
        }

        [Test]
        public void VerifyOrBinaryOperator()
        {
            // Arrange
            var leftHandSide = new Client.Requests.Filters.StartsWithFilter("Name", "test");
            var rightHandSide = new Client.Requests.Filters.EndsWithFilter("Name", "test");
            var orBinaryFilter = new Client.Requests.Filters.OrFilter(leftHandSide, rightHandSide);

            // Act
            var computedFilter = orBinaryFilter.ToString();
            var leftHandSideComputed = leftHandSide.ToString();
            var rightHandSideComputed = rightHandSide.ToString();

            // Assert
            computedFilter.Should().Be(leftHandSideComputed + " or " + rightHandSideComputed);
        }

        [Test]
        public void VerifyCompoundBinaryOperator()
        {
            // Arrange
            var orLeftHandSide = new Client.Requests.Filters.StartsWithFilter("Name", "test");
            var orRightHandSide = new Client.Requests.Filters.EndsWithFilter("Name", "test");
            var orBinaryFilter = new Client.Requests.Filters.OrFilter(orLeftHandSide, orRightHandSide);

            var andLeftHandSide = new Client.Requests.Filters.StartsWithFilter("Name", "test");
            var andRightHandSide = new Client.Requests.Filters.EndsWithFilter("Name", "test");
            var andBinaryFilter = new Client.Requests.Filters.AndFilter(andLeftHandSide, andRightHandSide);

            var compoundBinaryFilter = new Client.Requests.Filters.OrFilter(orBinaryFilter, andBinaryFilter);

            // Act
            var computedFilter = compoundBinaryFilter.ToString();

            // Assert
            computedFilter.Should().Be(orBinaryFilter + " or " + andBinaryFilter);
        }
    }
}
