using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using ShareFile.Api.Client.Extensions;
using ShareFile.Api.Client.Logging;

namespace ShareFile.Api.Client
{
    public class Configuration
    {
        /// <summary>
        /// Ensure all API requests are executed as POST reqeusts with an override header specified
        /// </summary>
        public bool UseHttpMethodOverride { get; set; }

        /// <summary>
        /// Automatically compose object uri before returning result
        /// </summary>
        public bool AutoComposeUri { get; set; }

        /// <summary>
        /// Timeout, in milliseconds, for API requests (excluding uploads transfer)
        /// </summary>
        public int HttpTimeout { get; set; }

        /// <summary>
        /// Register ProxyConfiguration to be used for all requests
        /// </summary>
        public IWebProxy ProxyConfiguration { get; set; }

        public ILogger Logger { get; set; }

        /// <summary>
        /// If true, then personal information (e.g. name and email) will be logged.
        /// <para>This should probably only be changed to true in a development environment.</para>
        /// </summary>
        public bool LogPersonalInformation { get; set; }

        /// <summary>
        /// If true, then the full json object will be logged doing API calls. Otherwise, collections on the json object
        /// will not be logged. Metadata about the collection will be logged instead.
        /// </summary>
        public bool LogFullResponse { get; set; }

        /// <summary>
        /// If true, all cookies and headers associated with a request will be logged.
        /// </summary>
        public bool LogCookiesAndHeaders { get; set; }

        /// <summary>
        /// Header value to send on every API call.
        /// </summary>
        public IEnumerable<CultureInfo> SupportedCultures { get; set; }

        private string _toolName;

        public string ToolName
        {
            get
            {
                if (string.IsNullOrEmpty(_toolName))
                {
                    return DefaultToolName;
                }
                return _toolName;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _toolName = DefaultToolName;
                    return;
                }
                _toolName = value;
            }
        }

        public string ToolVersion { get; set; }

        public static Configuration Default()
        {
            return new Configuration
            {
                UseHttpMethodOverride = false,
                AutoComposeUri = true,
                ToolName = DefaultToolName,
                ToolVersion = GetDefaultToolVersion(),
                HttpTimeout = 100000,
                Logger = new DefaultLoggingProvider { LogLevel = LogLevel.Error },
                LogPersonalInformation = false,
                LogFullResponse = false
            };
        }

        public const string DefaultToolName = "SF Client SDK";

        private static string _defaultToolVersion;
        public static string GetDefaultToolVersion()
        {
            if (!string.IsNullOrEmpty(_defaultToolVersion)) return _defaultToolVersion;

            var fileVersion =
                Assembly.GetExecutingAssembly()
                    .GetCustomAttributes(true)
                    .OfType<AssemblyFileVersionAttribute>()
                    .FirstOrDefault();
            if (fileVersion == null)
            {
                _defaultToolVersion = "3.0.0";
            }
            else
            {
                _defaultToolVersion = fileVersion.Version;
            }

            return _defaultToolVersion;
        }
    }
}
