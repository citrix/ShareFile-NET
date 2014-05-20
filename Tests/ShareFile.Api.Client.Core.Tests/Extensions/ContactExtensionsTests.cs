using NUnit.Framework;
using ShareFile.Api.Client.Extensions;
using ShareFile.Api.Models;

namespace ShareFile.Api.Client.Core.Tests.Extensions
{
    [TestFixture]
    public class ContactExtensionsTests
    {
        [TestCase("g123456", Result = true, TestName = "ContactExtensions_IsDistributionGroup_Yes")]
        [TestCase("a123456", Result = false, TestName = "ContactExtensions_IsDistributionGroup_No")]
        public bool IsDistributionGroup(string id)
        {
            var contact = new Contact {Id = id};

            return contact.IsDistributionGroup();
        }
    }
}
