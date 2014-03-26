using System;
using ShareFile.Api.Models;

namespace ShareFile.Api.Client.Extensions
{
    public static class ODataObjectExtensions
    {
        public static void SetMetadata(this ODataObject oDataObject, Uri baseUri, string entitySet, string typeCast = null, ODataObjectType? type = null)
        {
            oDataObject.Properties.Add("MetadataBaseUri", baseUri.ToString());
            oDataObject.Properties.Add("EntitySet", entitySet);
            oDataObject.Properties.Add("TypeCast", typeCast);
            oDataObject.Properties.Add("Type", type.GetValueOrDefault(ODataObjectType.Entity).ToString());
        }

        public static Uri GetObjectUri(this ODataObject oDataObject, bool useStreamId = false)
        {
            return oDataObject.url;
        }

        public static Uri GetMetadataBaseUri(this ODataObject oDataObject)
        {
            string metadata;

            if (oDataObject.Properties.TryGetValue("MetadataBaseUri", out metadata))
            {
                return new Uri(metadata, UriKind.RelativeOrAbsolute);
            }
            return null;
        }

        public static string GetEntitySet(this ODataObject oDataObject)
        {
            string entitySet;

            if (oDataObject.Properties.TryGetValue("EntitySet", out entitySet))
            {
                return entitySet;
            }
            return null;
        }

        public static string GetTypeCast(this ODataObject oDataObject)
        {
            string typeCast;

            if (oDataObject.Properties.TryGetValue("TypeCast", out typeCast))
            {
                return typeCast;
            }
            return null;
        }

        public static ODataObjectType GetODataObjectType(this ODataObject oDataObject)
        {
            string type;

            if (oDataObject.Properties.TryGetValue("TypeCast", out type))
            {
                ODataObjectType @enum;
                if (Enum.TryParse(type, out @enum))
                {
                    return @enum;
                }
            }
            return ODataObjectType.Entity;
        }
    }
}
