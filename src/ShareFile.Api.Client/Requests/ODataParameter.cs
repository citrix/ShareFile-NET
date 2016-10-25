using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShareFile.Api.Client.Requests
{
    public class ODataParameter
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public bool EncodeValue { get; set; }

        public ODataParameter()
        {

        }

        public ODataParameter(string key, string value, bool? encodeValue = null)
        {
            Key = key;
            Value = value;

            if (encodeValue == null && string.IsNullOrEmpty(Key))
            {
                EncodeValue = false;
            }
            else EncodeValue = encodeValue.GetValueOrDefault(false);
        }

        public ODataParameter(string value, bool? encodeValue = null)
        {
            Key = null;
            Value = value;
            EncodeValue = encodeValue.GetValueOrDefault();
        }

        public string ToStringForUri()
        {
            return string.IsNullOrEmpty(Key)
                ? (EncodeValue ? Uri.EscapeDataString(Value) : Value)
                : string.Format("{0}={1}", Uri.EscapeDataString(Key), Uri.EscapeDataString(Value));
        }

        public override string ToString()
        {
            return string.IsNullOrEmpty(Key)
                       ? Value
                       : string.Format("{0}={1}", Key, Value);
        }

        public override int GetHashCode()
        {
            return string.IsNullOrEmpty(Key)
                       ? Value.GetHashCode()
                       : Key.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var param = obj as ODataParameter;
            if (param == null)
            {
                return false;
            }
            return Key == param.Key;
        }
    }
}
