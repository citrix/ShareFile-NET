using System;
using System.Collections.Generic;

namespace ShareFile.Api.Client.Extensions
{
    internal static class StringExtensions
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
    }
}
