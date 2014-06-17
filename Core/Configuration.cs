using System.Net;
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

        #region DeviceId properties
        public string ToolName { get; set; }
        public string ToolVersion { get; set; }
        public string DeviceId { get; set; }
        public string DeviceName { get; set; }
        public bool UseDeviceId { get; set; }
        #endregion

        public static Configuration Default()
        {
            return new Configuration
            {
                UseHttpMethodOverride = false,
                AutoComposeUri = true,
                ToolName = "SF Client SDK",
                ToolVersion = "3.0",
                HttpTimeout = 100000,
                Logger = new DefaultLoggingProvider { LogLevel = LogLevel.Error },
                LogPersonalInformation = false,
                LogFullResponse = false
            };
        }
    }
}
