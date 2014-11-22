using System;
using System.Collections.Generic;
using ShareFile.Api.Client.Requests;

namespace ShareFile.Api.Client.Extensions
{
    public static class UriExtensions
    {
        public static string[] GetSegments(this Uri uri)
        {
            string[] segments = null; // used to be a class cached result
            if (segments == null)
            {

                string path = uri.ToString();

                if (path.Length == 0)
                {
                    segments = new string[0];
                }
                else
                {
                    var pathSegments = new List<string>();
                    int current = 0;
                    while (current < path.Length)
                    {
                        int next = path.IndexOf('/', current);
                        if (next == -1)
                        {
                            next = path.Length - 1;
                        }
                        pathSegments.Add(path.Substring(current, (next - current) + 1));
                        current = next + 1;
                    }
                    segments = pathSegments.ToArray();
                }
            }
            return segments;
        }

        public static string GetAuthority(this Uri uri)
        {
            return string.Format("{0}://{1}", uri.GetComponents(UriComponents.Scheme, UriFormat.Unescaped), uri.GetComponents(UriComponents.Host, UriFormat.Unescaped));
        }


        private static readonly char[] EqualsChar = { '=' };
        private static readonly char[] AmpersandChar = { '&' };
        /// <summary>
        /// Convert Uri.Query into collection of <see cref="ODataParameter"/>
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static IEnumerable<ODataParameter> GetQueryAsODataParameters(this Uri uri)
        {
            foreach (var parameter in uri.Query.Substring(1).Split(AmpersandChar, StringSplitOptions.RemoveEmptyEntries))
            {
                var kvp = parameter.Split(EqualsChar);
                if (kvp.Length == 1)
                {
                    yield return new ODataParameter(kvp[0]);
                }
                else
                {
                    yield return new ODataParameter(kvp[0], kvp[1]);
                }
            }
        }
    }
}
