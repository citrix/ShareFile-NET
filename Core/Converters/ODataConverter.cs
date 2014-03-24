using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using ShareFile.Api.Client.Helpers;
using ShareFile.Api.Models;

namespace ShareFile.Api.Client.Converters
{
    public class ODataConverter : CustomCreationConverter<ODataObject>
    {
        ODataFactory factory = ODataFactory.GetInstance();

        private Dictionary<string, string> ODataFields;
        private Dictionary<string, string> ODataFieldsForFeeds;

        public ODataConverter()
            : base()
        {
            ODataFields = new Dictionary<string, string>();
            ODataFields.Add("odata.metadata", "");
            ODataFields.Add("odata.count", "");
            ODataFields.Add("odata.nextLink", "NextLink");
            ODataFieldsForFeeds = new Dictionary<string, string>();
            foreach (string key in ODataFields.Keys) ODataFieldsForFeeds.Add(key, ODataFields[key]);
            ODataFieldsForFeeds.Add("value", "Feed");
        }

        public override ODataObject Create(Type objectType)
        {
            return factory.Create(objectType);
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
        /// <param name="jObject"></param>
        /// <returns></returns>
        protected ODataObject Create(Type objectType, JObject jObject)
        {
            ODataObject o = null;

            var metadata = (string)jObject.Property("odata.metadata");
            if (metadata != null) o = factory.CreateFromMetadata(metadata, objectType);

            if (o == null)
            {
                var url = (string)jObject.Property("url");
                if (url != null) o = factory.CreateFromUrl(url);

                if (o == null)
                {
                    var id = (string)jObject.Property("id");
                    o = factory.Create(objectType, id: id);
                }
            }
            return o;
        }

        private object[] defaultConstructorParamValues = new object[] { };

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // Load JObject from stream
            JObject jObject = JObject.Load(reader);

            // Create target object based on JObject
            ODataObject target = Create(objectType, jObject);

            if (target.Properties == null)
            {
                target.Properties = new Dictionary<string, string>();
            }
            
            var targetType = target.GetType();

            // Special handling for known odata fields
            // JsonPropertyNameAttribute annotations has this information, but would require listing all properties looking for the match...
            var oDataFields = GetODataFields(targetType);

            foreach (var jProperty in jObject.Properties())
            {
                var jpName = jProperty.Name;
                var atIndex = jpName.IndexOf('@');
                if (atIndex > -1) jpName = jpName.Substring(0, atIndex);
                if (oDataFields.ContainsKey(jpName) && oDataFields[jpName] != null)
                {
                    if (oDataFields[jpName] == "") continue;
                    jpName = oDataFields[jpName];
                }
                var oProperty = TypeHelpers.GetPublicProperty(jpName, target.GetType());

                if (oProperty != null)
                {
                    // Shouldn't need this bit anymore.  Properties must be List or some IEnumerable derivative now.
                    //if (TypeHelpers.IsAssignableFrom(typeof(ODataFeed<>), oProperty.PropertyType))
                    //{
                    //    // Special handling for feeds.. The properties are defined in the top object, but must be assigned to the feed
                    //    // instance.. Part of ODATA spec
                    //    // A feed that is a child of an element will contain the fields:
                    //    // PropertyName@odata.count: n,
                    //    // PropertyName: [ ... ],
                    //    // PropertyName@odata.nextLink: "https://..."
                    //    var feed = (ODataFeed<>)oProperty.GetValue(target);
                    //    if (feed == null)
                    //    {
                    //        feed = (IODataFeed)oProperty.PropertyType.GetConstructor(GetEmptyTypes()).Invoke(defaultConstructorParamValues);
                    //        oProperty.SetValue(target, feed);
                    //    }
                    //    if (jProperty.Name.EndsWith("@odata.nextLink"))
                    //    {
                    //        feed.SetNextLink((string)jProperty.Value.ToObject(typeof(string), serializer));
                    //    }
                    //    else if (atIndex == -1)
                    //    {
                    //        var feedProperty = TypeHelpers.GetPublicProperty("Feed", oProperty.PropertyType);
                    //        //TODO: Fix this
                    //        //feedProperty.SetValue(feed, jProperty.Value.ToObject(feedProperty.PropertyType, serializer));
                    //    }
                    //}
                    //else
                    //{
                        // Regular non-feed fields: just ask the serializer to convert
                        oProperty.SetValue(target, jProperty.Value.ToObject(oProperty.PropertyType, serializer), null);
                    //}
                }
                else
                {
                    // Hold any unknown property into a property bag for forward compatibility 
                    // Allows adding new properties for client consumption without the client updating the model
                    target.Properties.Add(jProperty.Name, jProperty.Value.ToString());
                }
            }
            return target;
        }

        public override bool CanConvert(Type objectType)
        {
            return TypeHelpers.IsAssignableFrom(typeof(ODataObject), objectType);
        }

        private Dictionary<string, string> GetODataFields(Type targetType)
        {
            if (targetType.IsGenericType)
            {
                if (TypeHelpers.IsAssignableFrom(typeof (ODataFeed<>), targetType.GetGenericTypeDefinition()))
                {
                    return ODataFieldsForFeeds;
                }
            }
            return ODataFields;
        }

        private static Type[] GetEmptyTypes()
        {
            return new Type[] { };
        }
    }
}
