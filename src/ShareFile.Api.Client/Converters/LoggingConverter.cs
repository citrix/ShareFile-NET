using System.Collections;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ShareFile.Api.Client.Extensions;
using ShareFile.Api.Client.Security.Cryptography;
using System.Text.RegularExpressions;

namespace ShareFile.Api.Client.Converters
{
    /// <summary>
    /// This converter is used for logging. It is one-way (write only) and does not necessarily produce valid JSON.
    /// <para>The purpose of the converter is to optionally exclude personal information and 
    /// optionally only log metadata about IEnumerable properties.</para>
    /// </summary>
    public class LoggingConverter : JsonConverter
    {
        public static readonly Regex GuidRegex = new Regex(@"\b[A-Z0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}\b", RegexOptions.IgnoreCase);

        private readonly HashSet<string> _piiBlacklist;
        private readonly ShareFileClient _client;

        public LoggingConverter(ShareFileClient client)
        {
            this._client = client;

            _piiBlacklist = new HashSet<string>();
            _piiBlacklist.Add("FullName");
            _piiBlacklist.Add("FirstName");
            _piiBlacklist.Add("LastName");
            _piiBlacklist.Add("Email");
            _piiBlacklist.Add("Username");
            _piiBlacklist.Add("FullNameShort");
            _piiBlacklist.Add("Name");
            _piiBlacklist.Add("FileName");
            _piiBlacklist.Add("CreatorFirstName");
            _piiBlacklist.Add("CreatorLastName");
            _piiBlacklist.Add("CreatorNameShort");
            _piiBlacklist.Add("Company");
            _piiBlacklist.Add("Emails");
            _piiBlacklist.Add("Body");
            _piiBlacklist.Add("Message");
        }

        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanRead
        {
            get
            {
                return false;
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            InternalWrite(writer, null, value, serializer);
        }

        private void InternalWrite(JsonWriter writer, string name, object value, JsonSerializer serializer)
        {
            JsonConverter converter;
            if (value == null)
            {
                if (serializer.NullValueHandling == NullValueHandling.Include)
                    writer.WriteNull();
                return;
            }

            Type valueType = value.GetType();
            if (HasConverter(serializer, valueType, out converter))
            {
                converter.WriteJson(writer, value, serializer);
            }
            else if (IsSimpleType(valueType))
            {
                // Only re-write values if the flag is switched, and they have an obvious string representation
                if (!_client.Configuration.LogPersonalInformation && 
                    (value is string || value is Uri))
                {
                    if (_piiBlacklist.Contains(name))
                        writer.WriteValue(GetHash(value.ToString()));
                    else
                        writer.WriteValue(GuidRegex.Replace(value.ToString(), GetHash));
                }
                else
                {
                    writer.WriteValue(value);
                }
            }
            else if (value is IEnumerable)
            {
                if (_client.Configuration.LogFullResponse)
                {
                    writer.WriteStartArray();
                    foreach (var o in (IEnumerable) value)
                        InternalWrite(writer, null, o, serializer);
                    writer.WriteEndArray();
                }
                else
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("Type");
                    writer.WriteValue(valueType.ToString());
                    writer.WritePropertyName("Count");
                    writer.WriteValue(((IEnumerable) value).Cast<object>().Count());
                    writer.WriteEndObject();
                }
            }
            else
            {
                var properties = valueType.GetPublicProperties();

                writer.WriteStartObject();
                foreach (var prop in properties)
                {
                    var propValue = prop.GetValue(value, null);
                    if (propValue != null || serializer.NullValueHandling == NullValueHandling.Include)
                    {
                        writer.WritePropertyName(prop.Name);
                        InternalWrite(writer, prop.Name, propValue, serializer);
                    }
                }
                writer.WriteEndObject();
            }
        }

        private bool HasConverter(JsonSerializer serializer, Type objectType, out JsonConverter converter)
        {
            // Check list of converters (but skip this converter since it works on all types)
            converter = serializer.Converters.FirstOrDefault(i => i.GetType() != this.GetType() && i.CanConvert(objectType));
            return converter != null;
        }

        internal static string GetHash(Match match)
        {
            return GetHash(match.Value);
        }

        private static readonly Type[] _primitives =
        {
            typeof (String),
            typeof (Decimal),
            typeof (DateTime),
            typeof (DateTimeOffset),
            typeof (TimeSpan),
            typeof (Guid),
            typeof (Uri)
        };

        private static bool IsSimpleType(Type type)
        {
            return type.IsPrimitive() || _primitives.Contains(type);
        }

        public static string GetHash(string value)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(value);
            var hash = MD5HashProviderFactory.GetHashProvider().CreateHash();
            hash.Append(buffer, 0, buffer.Length);
            hash.Finalize(new byte[1], 0, 0);
            string result = hash.GetComputedHashAsString();
            return result;
        }
    }
}
