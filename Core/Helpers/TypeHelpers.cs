using System;
using System.Collections.Generic;
using System.Reflection;

namespace ShareFile.Api.Client.Helpers
{
    public class TypeHelpers
    {
        public static bool IsGenericType(Type type)
        {
#if NETFX_CORE
            return type.GetTypeInfo().IsGenericType;
#else
            return type.IsGenericType;
#endif
        }

        public static bool IsAssignableFrom(Type type, Type from)
        {
#if NETFX_CORE
            return type.GetTypeInfo().IsAssignableFrom(from.GetTypeInfo());
#else
            return type.IsAssignableFrom(from);
#endif
        }

        public static Assembly GetAssembly(Type type)
        {
#if NETFX_CORE
            return type.GetTypeInfo().Assembly;
#else
            return type.Assembly;
#endif
        }

        public static IEnumerable<PropertyInfo> GetPublicProperties(Type type)
        {
#if NETFX_CORE
            return type.GetProperties();
#else
            return type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
#endif
        }

        public static PropertyInfo GetPublicProperty(string propertyName, Type type)
        {
#if NETFX_CORE
            return
                type.GetProperty(propertyName);
#else
            return type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
#endif
        }

        public static IEnumerable<Type> GetTypes(Assembly assembly)
        {
#if NETFX_CORE
            return assembly.DefinedTypes.Select(x => x.AsType());
#else
            return assembly.GetTypes();
#endif
        }

        public static IEnumerable<Attribute> GetCustomAttributes(PropertyInfo propertyInfo)
        {
#if NETFX_CORE
            return propertyInfo.GetCustomAttributes();
#else
            return Attribute.GetCustomAttributes(propertyInfo);
#endif
        }
    }
}
