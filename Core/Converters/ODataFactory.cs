using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using ShareFile.Api.Client.Extensions;
using ShareFile.Api.Client.Helpers;
using ShareFile.Api.Models;
using Group = System.Text.RegularExpressions.Group;

namespace ShareFile.Api.Client.Converters
{
    /// <summary>
    /// This class handles the creation of OData Objects from string-based specifications, mainly
    /// for deserialization operations. It uses several method to identify the serialized classes:
    /// the Odata metadata, the Odata Url, and the ID in certain cases - based on ShareFile ID 
    /// prefixes
    /// </summary>
    public class ODataFactory
    {
        public static ODataFactory GetInstance()
        {
            if (_instance == null) _instance = new ODataFactory();
            return _instance;
        }

        private static ODataFactory _instance = null;

        private ODataFactory()
        {
            _constructorCache = new Dictionary<Type, ConstructorInfo>();
            _typeMap = new Dictionary<Type, Type>();
            var odataObjectType = typeof(ODataObject);
            var types = TypeHelpers.GetTypes(TypeHelpers.GetAssembly(odataObjectType));

            foreach (var t in types.Where(x => TypeHelpers.IsAssignableFrom(odataObjectType, x)))
            {
                TryAddType(t);
            }

            _entityTypeMap = EntityTypeMap.GetEntityTypeMap();
        }

        private void TryAddType(Type modelType, Type overrideType = null)
        {
            var effectiveType = overrideType ?? modelType;

            var ctor = effectiveType.GetConstructor(new Type[] { });
            if (ctor != null)
            {
                _constructorCache[modelType] = ctor;
            }
        }

        private ODataObject InvokeConstructor(Type type, string id = "")
        {
            ODataObject oDataObject = null;
            if (_constructorCache.ContainsKey(type))
            {
                oDataObject = (ODataObject)_constructorCache[type].Invoke(new Object[] {});
            }
            else if (type != null)
            {
                var ctor = type.GetConstructor(new Type[] { });
                if (ctor != null)
                {
                    oDataObject = (ODataObject)ctor.Invoke(new Object[] { });
                }
            }

            if (oDataObject == null)
            {
                oDataObject = new ODataObject();
            }

            oDataObject.Id = id;
            return oDataObject;
        }

        private readonly Dictionary<Type, ConstructorInfo> _constructorCache;
        private readonly Dictionary<Type, Type> _typeMap;
        private readonly Dictionary<string, Type> _entityTypeMap;

        /// <summary>
        /// Allow consumers to regsiter type substitutions.
        /// </summary>
        /// <typeparam name="TNew">Type constructor to use in place of TReplace</typeparam>
        /// <typeparam name="TReplace">Type you want to replace</typeparam>
        public void RegisterType<TNew, TReplace>()
            where TNew : TReplace
            where TReplace : ODataObject
        {
            var overrideType = typeof(TNew);
            var modelType = typeof(TReplace);
            _typeMap[overrideType] = modelType;

            TryAddType(modelType, overrideType);
        }

        public void RegisterEntity(string EntityName, Type EntityType)
        {
            _entityTypeMap[EntityName] = EntityType;
        }

        /// <summary>
        /// Creates a Model class based on the type name, as a string.
        /// </summary>
        /// <remarks>
        /// This is not a terribly efficient method, should be used as catch-22 if you want a generic
        /// handler. If you know the return type, or at least the superclass, use Create passing the
        /// .NET Type object, which is more efficient.
        /// </remarks>
        /// <param name="cast">Type name. Either full namespace or just name will work. Entity set names 
        /// are also supported (e.g., Items for Item)</param>
        /// <param name="context">Optional request context</param>
        /// <param name="id">Optional id if you already know it. This method will use the ShareFile ID 
        /// prefixes to handle superclasses (e.g., cast=Items and id=fi* instantiate a File instance)</param>
        /// <returns>A subtype of ODataObject, matching the requested type/id. Returns null if type doesnt
        /// match any known type</returns>
        public ODataObject Create(string cast, string id = null)
        {
            var type = FindModelType(null, cast, id);
            return InvokeConstructor(type, id);
        }

        public Type FindModelType(Type knownType, string cast, string id = null)
        {
            Type type = knownType;
            // Normalize cast, remove namespaces
            if (cast != null)
            {
                string namesp = typeof(ODataObject).Namespace;
                if (cast.StartsWith(namesp)) cast = cast.Substring(namesp.Length + 1);
            }
            // If knownType is unknown, type to infer from the Cast string
            if ((type == null || type == typeof(ODataObject)) && cast != null)
            {
                type = _entityTypeMap.ContainsKey(cast) ? _entityTypeMap[cast] : null;
            }
            if (type != null && type != typeof(ODataObject))
            {
                if (cast != null || id != null)
                {
                    // Entities with subtypes
                    // Try the Cast string
                    if (cast != null && (type == typeof(Item) || type == typeof(Principal)))
                    {
                        type = _entityTypeMap.ContainsKey(cast) ? _entityTypeMap[cast] : type;
                    }
                    // Try the ID
                    if (id != null && (type == typeof(Item) || type == typeof(Principal)))
                    {
                        if (id.StartsWith("fi")) type = typeof(File);
                        else if (id.StartsWith("for")) type = typeof(SymbolicLink);
                        else if (id.StartsWith("fo")) type = typeof(Folder);
                        else if (id.StartsWith("n")) type = typeof(Note);
                        else if (id.StartsWith("l")) type = typeof(Link);
                        else if (id.StartsWith("a")) type = typeof(Folder);
                        else if (id.StartsWith("g")) type = typeof(Group);
                        // User has no prefix; so assume it's an user at this point (if superclass is Principal)
                        else if (type == typeof(Principal)) type = typeof(User);
                    }
                }
            }
            else type = typeof(ODataObject);

            if (_typeMap.ContainsKey(type))
            {
                type = _typeMap[type];
            }
            return type;
        }

        /// <summary>
        /// Creates a new instance of ODataObject of the specified type. 
        /// </summary>
        /// <remarks>
        /// If Type is Item or
        /// Principal, the appropriate sub-type is instantiated based on ID. This is not kosher from
        /// ODATA perspective: the Type Cast parameter in metadata should be used instead.. 
        /// The method will use reflection to find the type constructor. Objects must have a
        /// constructor (string, context) or (string, context, string) to be found. Otherwise,
        /// a ODataObject instance is created instead.
        /// </remarks>
        /// <param name="type"></param>
        /// <param name="cast"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public ODataObject Create(Type type, string cast = null, string id = null)
        {
            type = FindModelType(type, cast, id);

            if (TypeHelpers.IsGenericType(type))
            {
                var genericODataFeed = typeof(ODataFeed<>);
                var genericType = type.GetGenericTypeDefinition();
                if (genericType == genericODataFeed)
                {
                    var ctor = type.GetConstructors().FirstOrDefault();
                    if (ctor != null)
                    {
                        var oDataObject = (ODataObject)Activator.CreateInstance(type, new object[] { });
                        oDataObject.Id = id;

                        return oDataObject;
                    }
                }
            }
            return InvokeConstructor(type, id);
        }

        public ODataObject CreateFromUrl(string Url)
        {
            var odataExpression = @"^(?<cast>[^$\(]+)\((?<id>[^\)]+)\)";

            Uri uri = new Uri(Url);
            string type = null;
            string id = null;
            var uriSegments = uri.GetSegments();

            foreach (var segment in uriSegments)
            {
                var match = Regex.Match(segment, odataExpression, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    type = match.Groups["cast"].Value;
                    id = match.Groups["id"].Value;
                }
            }
            if (type == null && uriSegments.Length > 0) { type = uriSegments[uriSegments.Length - 1]; }

            if (type != null)
            {
                var o = Create(type, id);
                return o;
            }
            return new ODataObject
            {
                Id = id
            };
        }

        protected string[] metadataExpressions = new string[] {
            @"(?<url>[^#\$]+)\$metadata#(?<entity>\w+)(?<element>@Element)",
            @"(?<url>[^#\$]+)\$metadata#(?<entity>\w+)/(?<cast>[^@]+)(?<element>@Element)",
            @"(?<url>[^#\$]+)\$metadata#(?<entity>\w+)/(?<cast>.+)",
            string.Format(@"(?<url>[^#\$]+)\$metadata#{0}.(?<cast>.+)", typeof(ODataObject).Namespace),
            @"(?<url>[^#\$]+)\$metadata#(?<feedentity>.+)"
        };

        public ODataObject CreateFromMetadata(string metadata, Type knownType = null)
        {
            foreach (var expression in metadataExpressions)
            {
                var match = Regex.Match(metadata, expression, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    ODataObject o = null;
                    if (match.Groups["feedentity"].Success)
                    {
                        var type = FindModelType(knownType, match.Groups["feedentity"].Value, null);
                        Type specificType;
                        if (TypeHelpers.IsGenericType(type) && type.GetGenericTypeDefinition() == typeof(ODataFeed<>))
                        {
                            specificType = type;
                        }
                        else specificType = typeof(ODataFeed<>).MakeGenericType(new System.Type[] { type });
                        o = (ODataObject)Activator.CreateInstance(specificType, new object[] { });
                  
                        o.SetMetadata(new Uri(match.Groups["url"].Value), match.Groups["feedentity"].Value, null, ODataObjectType.ComplexType);
                    }
                    else
                    {
                        string metadataType = match.Groups["cast"].Success ? match.Groups["cast"].Value : match.Groups["entity"].Value;
                        o = Create(knownType, metadataType);
                        if (match.Groups["entity"].Success)
                        {
                            o.SetMetadata(new Uri(match.Groups["url"].Value), match.Groups["entity"].Value, match.Groups["cast"].Value, ODataObjectType.Entity);
                        }
                        else
                        {
                            o.SetMetadata(new Uri(match.Groups["url"].Value), null, o.GetType().FullName, ODataObjectType.ComplexType);
                        }
                    }
                    return o;
                }
            }
            return null;
        }
    }
}
