using System;
using ShareFile.Api.Client.Entities;
using ShareFile.Api.Client.Requests;
using ShareFile.Api.Models;

namespace ShareFile.Api.Client.Extensions
{
    public static class ItemsEntityExtensions
    {
        public static IQuery<Item> Get(this ItemsEntity items, Uri uri)
        {
            return items.Get(uri.ToString());
        }

        public static IQuery Delete(this ItemsEntity items, Uri uri, bool singleVersion = false, bool forceSync = false)
        {
            return items.Delete(uri.ToString(), singleVersion, forceSync);
        }
    }
}
