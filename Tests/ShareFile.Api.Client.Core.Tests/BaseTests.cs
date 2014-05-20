using System;

namespace ShareFile.Api.Client.Core.Tests
{
    public abstract class BaseTests
    {
        protected const string BaseUriString = "https://release-sf-api.com/sf/v3/";
        protected IShareFileClient GetShareFileClient()
        {
            return new ShareFileClient(BaseUriString);
        }

        protected string GetId(int length = 36)
        {
            if (length > 36) length = 36;
            return Guid.NewGuid().ToString().Substring(0, length);
        }
    }
}
