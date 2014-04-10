using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShareFile.Api.Models;

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
    }
}
