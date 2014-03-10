using System;
using ShareFile.Api.Models;

namespace ShareFile.Api.Client.Extensions
{
    public static class ODataObjectExtensions
    {
        public static void SetMetadata(this ODataObject oDataObject, Uri baseUri, string entitySet, string typeCast = null, ODataObjectType? type = null)
        {
            //oDataObject.MetadataBaseUri = baseUri;
            //oDataObject.EntitySet = entitySet;
            //oDataObject.TypeCast = typeCast;
            //oDataObject.Type = type.GetValueOrDefault(ODataObjectType.Entity);
        }
    }
}
