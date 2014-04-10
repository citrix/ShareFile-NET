using System;
using Newtonsoft.Json;
using ShareFile.Api.Models;

namespace ShareFile.Api.Client.Converters
{
    public class SafeEnumConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var safeEnum = value as ISafeEnum;

            if (safeEnum == null) return;

            var jsonValue = safeEnum.Value;

            if (string.IsNullOrWhiteSpace(jsonValue)) return;

            serializer.Serialize(writer, jsonValue);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.String)
            {
                return null;
            }

            var instance = Activator.CreateInstance(objectType) as ISafeEnum;

            if (instance == null) return null;

            var enumType = objectType.GetGenericArguments()[0];
            var value = reader.ReadAsString();

            try
            {
                instance.Object = Enum.Parse(enumType, value, true);
            }
            catch (Exception)
            {
                instance.Value = value;
            }

            return instance;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType.IsEnum || (objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(SafeEnum<>));
        }
    }
}
