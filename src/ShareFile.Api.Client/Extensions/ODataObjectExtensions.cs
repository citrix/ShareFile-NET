using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using ShareFile.Api.Client.Models;

namespace ShareFile.Api.Client.Extensions
{
    public static class ODataObjectExtensions
    {
        public static void SetMetadata(this ODataObject oDataObject, Uri baseUri, string entitySet, string typeCast = null, ODataObjectType? type = null)
        {
            if (oDataObject.Properties == null)
            {
                oDataObject.Properties = new Dictionary<string, JToken>();
            }

            oDataObject.AddProperty("MetadataBaseUri", baseUri.ToString());
            oDataObject.AddProperty("EntitySet", entitySet);
            oDataObject.AddProperty("TypeCast", typeCast);
            oDataObject.AddProperty("Type", type.GetValueOrDefault(ODataObjectType.Entity).ToString());
        }

        public static Uri GetObjectUri(this ODataObject oDataObject, bool useStreamId = false)
        {
            if (useStreamId && oDataObject is Item)
            {
                var item = oDataObject as Item;
                if (!string.IsNullOrEmpty(item.StreamID))
                {
                    if (oDataObject.url != null)
                    {
                        return new Uri(oDataObject.url.ToString().Replace(item.Id, item.StreamID));
                    }
                    return oDataObject.ComposeUri((oDataObject as Item).StreamID);
                }
            }
            if (oDataObject.url == null)
            {
                oDataObject.url = oDataObject.ComposeUri((oDataObject).Id);
            }
            return oDataObject.url;
        }

        private static Uri ComposeUri(this ODataObject oDataObject, string id)
        {
            var metadataBaseUri = oDataObject.GetMetadataBaseUri();
            if (!(oDataObject is UploadSpecification) && metadataBaseUri != null)
            {
                return new Uri(string.Format("{0}/{1}({2})", metadataBaseUri.ToString().TrimEnd('/'),
                                          oDataObject.GetEntitySet(), id ?? oDataObject.Id));
            }
            return null;
        }

        public static void ComposeUri(this ODataObject oDataObject)
        {
            oDataObject.ComposeUri(oDataObject.Id);
        }

        public static void ComposeUri(this Item item, bool useStreamId = false)
        {
            if (useStreamId && !string.IsNullOrWhiteSpace(item.StreamID))
            {
                item.ComposeUri(item.StreamID);
            }
            else item.ComposeUri(item.Id);
        }

        public static Uri GetMetadataBaseUri(this ODataObject oDataObject)
        {
            string metadata;

            if (oDataObject.TryGetProperty("MetadataBaseUri", out metadata))
            {
                return new Uri(metadata, UriKind.RelativeOrAbsolute);
            }
            return null;
        }

        public static string GetEntitySet(this ODataObject oDataObject)
        {
            string entitySet;

            if (oDataObject.TryGetProperty("EntitySet", out entitySet))
            {
                return entitySet;
            }
            return null;
        }

        public static string GetTypeCast(this ODataObject oDataObject)
        {
            string typeCast;

            if (oDataObject.TryGetProperty("TypeCast", out typeCast))
            {
                return typeCast;
            }
            return null;
        }

        public static ODataObjectType GetODataObjectType(this ODataObject oDataObject)
        {
            string type;

            if (oDataObject.TryGetProperty("TypeCast", out type))
            {
                ODataObjectType @enum;
                if (Enum.TryParse(type, out @enum))
                {
                    return @enum;
                }
            }
            return ODataObjectType.Entity;
        }

        public static void AddProperty(this ODataObject oDataObject, string key, object value)
        {
            if (value == null) return;

            if (oDataObject.Properties == null)
            {
                oDataObject.Properties = new Dictionary<string, JToken>();
            }

            var token = value as JToken;
            if (token != null)
            {
                oDataObject.Properties[key] = token;
            }
            else oDataObject.Properties[key] = JToken.FromObject(value);
        }

        public static bool TryGetProperty<T>(this ODataObject oDataObject, string key, out T value)
        {
            value = default(T);

            JToken token;
            if (oDataObject != null && oDataObject.Properties != null && oDataObject.Properties.TryGetValue(key, out token))
            {
                value = token.ToObject<T>();
                return true;
            }
            return false;
        }

        public static bool SupportsUploadWithRequestParams(this IEnumerable<Capability> capabilities)
        {
            if (capabilities == null) return false;
            return capabilities.Any(x => x.Name == CapabilityName.UploadWithRequestParams);
        }

        public static bool SupportsDownloadWithSpecificaton(this IEnumerable<Capability> capabilities)
        {
            return capabilities != null && capabilities.Any(x => x.Name == CapabilityName.DownloadSpecification);
        }

        public static Uri GetCalculatedUri(this Redirection redirection)
        {
            var uri = redirection.Uri;
            if (!string.IsNullOrEmpty(redirection.Root))
            {
                if (!uri.Query.Contains("root="))
                {
                    var bridgeChar = string.IsNullOrEmpty(uri.Query) ? '?' : '&';
                    uri = new Uri(redirection.Uri.ToString().TrimEnd(bridgeChar) + bridgeChar + "root=" + redirection.Root);
                }
            }

            return uri;
        }
    }
}
