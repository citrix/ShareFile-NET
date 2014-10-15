using FluentAssertions;

using NUnit.Framework;

using ShareFile.Api.Client.Extensions;
using ShareFile.Api.Client.Requests.Filters;

namespace ShareFile.Api.Client.Core.Tests.Requests.Filters
{
    [TestFixture]
    public class EqualityFilterTests
    {
        [TestCase(true, TestName = "EndsWithFilter_IsEqual")]
        [TestCase(false, TestName = "EndsWithFilter_IsNotEqual")]
        public void EndsWithFilter(bool isEqual)
        {
            var filter = new EndsWithFilter("Name", "ShareFile", isEqual);

            filter.ToString().Should().Be("endswith(Name, 'ShareFile') eq " + isEqual.ToLowerString());
        }

        [TestCase(true, TestName = "StartsWithFilter_IsEqual")]
        [TestCase(false, TestName = "StartsWithFilter_IsNotEqual")]
        public void StartsWithFilter(bool isEqual)
        {
            var filter = new StartsWithFilter("Name", "ShareFile", isEqual);

            filter.ToString().Should().Be("startswith(Name, 'ShareFile') eq " + isEqual.ToLowerString());
        }

        [TestCase(true, TestName = "SubstringFilter_IsEqual")]
        [TestCase(false, TestName = "SubstringFilter_IsNotEqual")]
        public void SubstringFilter(bool isEqual)
        {
            var filter = new SubstringFilter("Name", "ShareFile", isEqual);

            filter.ToString().Should().Be("substringof('ShareFile', Name) eq " + isEqual.ToLowerString());
        }

        [Test]
        public void NotEqualToFilter()
        {
            var filter = new NotEqualToFilter("Name", "ShareFile");

            filter.ToString().Should().Be("Name ne 'ShareFile'");
        }

        [Test]
        public void EqualToFilter()
        {
            var filter = new EqualToFilter("Name", "ShareFile");

            filter.ToString().Should().Be("Name eq 'ShareFile'");
        }
    }
}
