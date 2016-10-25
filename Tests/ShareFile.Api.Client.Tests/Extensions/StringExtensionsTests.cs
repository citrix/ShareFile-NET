using System;
using System.Text;

using FluentAssertions;
using ShareFile.Api.Client.Extensions;

using NUnit.Framework;

namespace ShareFile.Api.Client.Core.Tests.Extensions
{
    [TestFixture]
    public class StringExtensionsTests : BaseTests
    {
        [TestCase]
        public void VerifyIsBase64Encoded()
        {
            Convert.ToBase64String(Encoding.UTF8.GetBytes(RandomString(100))).IsBase64().Should().BeTrue();
        }

        [TestCase("YXNkZmdoamtscXdlcnR5dWlvcA==", ExpectedResult = "YXNkZmdoamtscXdlcnR5dWlvcA==")]
        [TestCase("asdfghjklqwertyuiop", ExpectedResult = "YXNkZmdoamtscXdlcnR5dWlvcA==")]
        public string Base64Encode(string val)
        {
            return val.ToBase64();
        }
    }
}
