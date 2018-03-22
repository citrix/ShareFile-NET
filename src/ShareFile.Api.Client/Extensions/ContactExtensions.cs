using ShareFile.Api.Client.Models;

namespace ShareFile.Api.Client.Extensions
{
    public static class ContactExtensions
    {
        public static bool IsDistributionGroup(this Contact contact)
        {
            return contact.Id != null && contact.Id.StartsWith("g");
        }
    }
}
