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
                Logger = new DefaultLoggingProvider { LogLevel = LogLevel.Error }
            };
        }
    }
}
