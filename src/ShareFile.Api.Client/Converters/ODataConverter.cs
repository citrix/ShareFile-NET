using System;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ShareFile.Api.Client.Extensions;
using ShareFile.Api.Client.Models;

namespace ShareFile.Api.Client.Converters
{
    public class ODataConverter : CustomCreationConverter<ODataObject>
    {
        readonly ODataFactory _factory = ODataFactory.GetInstance();

        public override ODataObject Create(Type objectType)
        {
            return _factory.Create(objectType);
        }

        /// <summary>
        /// Creates a ShareFile ODataObject instance given a jObject instance. 
        /// </summary>
        /// <remarks>
        /// This method will look first for the odata.metadata property in the jObject instance. If
        /// not found, it will fallback to the URL representation, and finally, to the object ID. If
        /// all of these fail, the method will instantiate the requested type from JSON.NET, which 
        /// matches the type of the attribute. 
        /// The ODataFactory class provide create methods for each of these matches. 
        /// Metadata and URL are the "proper" ways to handle ODATA deserialization; the ID fallback is
        /// convinient for certain operations, but uses the ShareFile ID prefixes and should be avoided.
        /// </remarks>
        /// <param name="objectType"></param>
        /// <param name="oDataObject"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        protected ODataObject Create(Type objectType, ODataObject oDataObject, JsonSerializer serializer)
        {
            // Default result to null
            ODataObject result = null;

            if (!string.IsNullOrEmpty(oDataObject.__type))
            {
                result = _factory.CreateFromType(oDataObject.__type, objectType, oDataObject, serializer);
            }
            if (result == null && !string.IsNullOrEmpty(oDataObject.MetadataUrl))
            {
                result = _factory.CreateFromMetadata(oDataObject.MetadataUrl, objectType, oDataObject, serializer);
            }
            if (result == null && oDataObject.url != null)
            {
                result = _factory.CreateFromUrl(oDataObject.url.ToString(), oDataObject, serializer);
            }

            // if result still null, fallback to the provided oDataObject value
            return result ?? oDataObject;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var target = (ODataObject)base.ReadJson(reader, objectType, existingValue, serializer);
            return Create(target.GetType(), target, serializer);
        }

        private static readonly Type ODataObjectType = typeof(ODataObject);
        public override bool CanConvert(Type objectType)
        {
            return ODataObjectType.IsAssignableFrom(objectType);
        }
    }
}
