using System;
using System.Collections.Generic;
using ShareFile.Api.Client.Requests;
using System.Linq;
using System.Text;

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


		private const char EqualsChar = '=';
		private static readonly char[] AmpersandChar = { '&' };
        private const int NotFoundPosition = -1;
		/// <summary>
		/// Convert Uri.Query into collection of <see cref="ODataParameter"/>
		/// </summary>
		/// <param name="uri"></param>
		/// <returns></returns>
		public static IEnumerable<ODataParameter> GetQueryAsODataParameters(this Uri uri)
		{
			foreach (var parameter in uri.Query.Substring(1).Split(AmpersandChar, StringSplitOptions.RemoveEmptyEntries))
			{
                int keyValueSeparatorPosition = parameter.IndexOf(EqualsChar);
                if(keyValueSeparatorPosition == NotFoundPosition)
                {
                    yield return new ODataParameter(key: Uri.UnescapeDataString(parameter), value: "");
                    continue;
                }
                string key = parameter.Substring(0, keyValueSeparatorPosition);
                string value = "";
                if(keyValueSeparatorPosition + 1 < parameter.Length)
                {
                    value = parameter.Substring(keyValueSeparatorPosition + 1);
                }
                yield return new ODataParameter(key: Uri.UnescapeDataString(key), value: Uri.UnescapeDataString(value));
			}
		}
	}
}
