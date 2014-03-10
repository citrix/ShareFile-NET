using System;
using System.Collections.Generic;

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
    }
}
