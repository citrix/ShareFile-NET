using System;
using ShareFile.Api.Client.Entities;
using ShareFile.Api.Client.Enums;
using ShareFile.Api.Client.Requests;
using ShareFile.Api.Models;

namespace ShareFile.Api.Client.Extensions
{
    public static class ItemsEntityExtensions
    {
        /// <summary>
        /// Will return a composed Uri that will point to Items(alias) for the BaseUri
        /// </summary>
        /// <returns></returns>
        public static Uri GetAlias(this IItemsEntity items, ItemAlias alias)
        {
            return items.GetAlias(alias.ToString().ToLower());
        }

        /// <summary>
        /// Will return a composed Uri that will point to Items(alias) for the BaseUri
        /// </summary>
        /// <returns></returns>
        public static Uri GetAlias(this IItemsEntity items, string aliasOrId)
        {
            string url;
            if (items.Client.NextRequestBaseUri != null)
            {
                url = items.Client.NextRequestBaseUri.ToString();
            }
            else url = items.Client.BaseUri.ToString();

            return new Uri(url.TrimEnd(new[] { '/' }) + "/" + items.Entity + "(" + aliasOrId + ")", UriKind.RelativeOrAbsolute);
        }
    }
}
