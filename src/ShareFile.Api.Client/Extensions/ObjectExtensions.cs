using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShareFile.Api.Client.Models;

namespace ShareFile.Api.Client.Extensions
{
    public static class ObjectExtensions
    {
        public static SafeEnum<TEnum> ToSafeEnum<TEnum>(this object @enum)
            where TEnum : struct
        {
            var instance = Activator.CreateInstance<SafeEnum<TEnum>>();
            instance.Object = @enum;

            return instance;
        }

        public static TResult As<TCast, TResult>(this object obj, Func<TCast, TResult> map)
        {
            return As(obj, map, default(TResult));
        }

        public static TResult As<TCast, TResult>(this object obj, Func<TCast, TResult> map, TResult defaultValue)
        {
            //check assignable from instead?
            if (obj is TCast)
            {
                return map((TCast)obj);
            }
            else
            {
                return defaultValue;
            }
        }

        public static T Bound<T>(this T value, T upperBound, T lowerBound) where T : IComparable
        {
            if (value.CompareTo(upperBound) == 1)
                return upperBound;
            else if (value.CompareTo(lowerBound) == -1)
                return lowerBound;
            else
                return value;
        }

    }
}
