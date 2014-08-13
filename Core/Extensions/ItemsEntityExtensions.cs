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
            string aliasString;
            switch (alias)
            {
                case ItemAlias.NetworkShareConnectors:
                    aliasString = "c-cifs";
                    break;
                case ItemAlias.SharepointConnectors:
                    aliasString = "c-sp";
                    break;
                default:
                    aliasString = alias.ToString().ToLower();
                    break;
            }

            return items.GetAlias(aliasString);
        }

        /// <summary>
        /// Will return a composed Uri that will point to Items(alias) for the BaseUri
        /// </summary>
        /// <returns></returns>
        public static Uri GetAlias(this IItemsEntity items, string aliasOrId)
        {
            string url = items.Client.BaseUri.ToString();

            return new Uri(url.TrimEnd(new[] { '/' }) + "/" + items.Entity + "(" + aliasOrId + ")", UriKind.RelativeOrAbsolute);
        }
    }
}
