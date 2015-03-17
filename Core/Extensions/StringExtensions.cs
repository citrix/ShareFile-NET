using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ShareFile.Api.Client.Extensions
{
    public static class StringExtensions
    {
        private static char[] _ampersand = new[] {'&'};
        private static char[] _equals = new[] { '=' };

        internal static Dictionary<string, string> ToQueryStringCollection(this string val)
        {
            if (string.IsNullOrWhiteSpace(val)) return null;
            if (val.StartsWith("?"))
            {
                val = val.Substring(1);
            }

            var queryString = new Dictionary<string, string>();

            var parameters = val.Split(_ampersand, StringSplitOptions.RemoveEmptyEntries);
            foreach (var parameter in parameters)
            {
                var kvp = parameter.Split(_equals);
                if (kvp.Length == 2)
                {
                    queryString.Add(kvp[0], kvp[1]);
                }
                else
                {
                    queryString.Add(kvp[0], null);
                }
            }

            return queryString;
        }

        public static bool IsBase64(this string val)
        {
            val = val.Trim();
            return (val.Length % 4 == 0) && Regex.IsMatch(val, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None);
        }

        /// <summary>
        /// Convert string to Base64 iff <paramref name="val"/> is not already encoded
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static string ToBase64(this string val)
        {
            if (val.IsBase64())
            {
                return val.Trim();
            }

            return Convert.ToBase64String(Encoding.UTF8.GetBytes(val));
        }
    }
}
