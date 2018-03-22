using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using ShareFile.Api.Client.Extensions;
using ShareFile.Api.Client.Models;

namespace ShareFile.Api.Client.Converters
{
    public class SafeEnumConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var safeEnum = value as ISafeEnum;

            if (safeEnum == null)
            {
                writer.WriteNull();
                return;
            }

            var jsonValue = safeEnum.Value;

            if (string.IsNullOrWhiteSpace(jsonValue))
            {
                writer.WriteNull();
                return;
            }

            if (char.IsDigit(jsonValue[0]) || jsonValue[0] == '-')
            {
                writer.WriteValue(long.Parse(jsonValue));
            }
            else
            {
                writer.WriteValue(jsonValue);
            }
        }

        private static readonly HashSet<JsonToken> SupportedTokenTypes = new HashSet<JsonToken>(new [] { JsonToken.Integer, JsonToken.String} );
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value == null || !SupportedTokenTypes.Contains(reader.TokenType))
            {
                return null;
            }

            var instance = Activator.CreateInstance(objectType) as ISafeEnum;
            if (instance == null)
            {
                return null;
            }

            var enumType = objectType.GetGenericArguments()[0];
            var value = reader.Value.ToString();

            try
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    instance.Value = null;
                }
                else
                {
                    instance.Object = Enum.Parse(enumType, value, true);
                }
            }
            catch (Exception)
            {
                instance.Value = value;
            }

            return instance;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType.IsEnum() || (objectType.IsGenericType() && objectType.GetGenericTypeDefinition() == typeof(SafeEnum<>));
        }
    }
}
