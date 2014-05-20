using FluentAssertions;
using NUnit.Framework;
using ShareFile.Api.Client.Extensions;

namespace ShareFile.Api.Client.Core.Tests.Extensions
{
    [TestFixture]
    public class BoolExtensionsTests
    {
        [Test]
        public void BoolExtensions_ToLowerCase()
        {
            bool trueBool = true;

            trueBool.ToLowerString().Should().Be("true");
        }
    }
}
