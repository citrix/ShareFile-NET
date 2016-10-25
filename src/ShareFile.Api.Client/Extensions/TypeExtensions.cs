using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ShareFile.Api.Client.Extensions
{
    public static class TypeExtensions
    {

        public static bool IsGenericTypeOf(this Type t, Type genericDefinition)
        {
            Type[] parameters = null;
            return IsGenericTypeOf(t, genericDefinition, out parameters);
        }

        public static bool IsGenericTypeOf(this Type t, Type genericDefinition, out Type[] genericParameters)
        {
            genericParameters = new Type[] { };
            if (!genericDefinition.IsGenericType())
            {
                return false;
            }

            var isMatch = t.IsGenericType() && t.GetGenericTypeDefinition() == genericDefinition.GetGenericTypeDefinition();
            if (!isMatch && t.GetBaseType() != null)
            {
                isMatch = IsGenericTypeOf(t.GetBaseType(), genericDefinition, out genericParameters);
            }
            if (!isMatch && genericDefinition.IsInterface() && t.GetInterfaces().Any())
            {
                foreach (var i in t.GetInterfaces())
                {
                    if (i.IsGenericTypeOf(genericDefinition, out genericParameters))
                    {
                        isMatch = true;
                        break;
                    }
                }
            }

            if (isMatch && !genericParameters.Any())
            {
                genericParameters = t.GetGenericArguments();
            }
            return isMatch;
        }

        public static IEnumerable<PropertyInfo> GetPublicProperties(this Type type)
        {
#if NETFX_CORE || NETSTANDARD1_3
			return type.GetTypeInfo().DeclaredProperties;
#else
            return type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
#endif
        }

        public static bool IsPrimitive(this Type type)
        {
#if NETFX_CORE || NETSTANDARD1_3
			return type.GetTypeInfo().IsPrimitive;
#else
            return type.IsPrimitive;
#endif
        }

        public static bool IsEnum(this Type type)
        {
#if NETFX_CORE || NETSTANDARD1_3
			return type.GetTypeInfo().IsEnum;
#else
            return type.IsEnum;
#endif
        }

        public static bool IsGenericType(this Type type)
        {
#if NETFX_CORE || NETSTANDARD1_3
			return type.GetTypeInfo().IsGenericType;
#else
            return type.IsGenericType;
#endif
        }

        public static Type GetBaseType(this Type type)
        {
#if NETFX_CORE || NETSTANDARD1_3
			return type.GetTypeInfo().BaseType;
#else
            return type.BaseType;
#endif
        }

        public static bool IsInterface(this Type type)
        {
#if NETFX_CORE || NETSTANDARD1_3
			return type.GetTypeInfo().IsInterface;
#else
            return type.IsInterface;
#endif  
        }

        public static Assembly GetAssembly(this Type type)
        {
#if NETFX_CORE || NETSTANDARD1_3
			return type.GetTypeInfo().Assembly;
#else
            return type.Assembly;
#endif
        }

#if NETFX_CORE || NETSTANDARD1_3
		public static IEnumerable<Type> GetInterfaces(this Type type)
        {
            return type.GetTypeInfo().ImplementedInterfaces;
        }
		
        public static Type[] GetGenericArguments(this Type type)
        {
            return type.GetTypeInfo().GenericTypeArguments;
        }

        public static ConstructorInfo GetConstructor(this Type type, params Type[] constructorParameters)
        {
            ConstructorInfo constructorInfo = null;
            var possibleMatches = type.GetTypeInfo().DeclaredConstructors.Where(x => x.GetParameters().Count() == constructorParameters.Length).ToList();

            if (!possibleMatches.Any()) return null;

            foreach (var possibleMatch in possibleMatches)
            {
                for (int i = 0; i < constructorParameters.Length; i++)
                {
                    if (possibleMatch.GetParameters()[i].ParameterType != constructorParameters[i])
                    {
                        break;
                    }
                }

                constructorInfo = possibleMatch;
                break;
            }

            return constructorInfo;
        }

        public static IEnumerable<Type> GetTypes(this Assembly assembly)
        {
            return assembly.DefinedTypes.Select(x => x.AsType());
        }
        
        public static bool IsAssignableFrom(this Type type, Type from)
        {
            return type.GetTypeInfo().IsAssignableFrom(from.GetTypeInfo());
        }

        public static bool IsSubclassOf(this Type type, Type ofType)
        {
            return type.GetTypeInfo().IsSubclassOf(ofType);
        }
#endif

    }
}
