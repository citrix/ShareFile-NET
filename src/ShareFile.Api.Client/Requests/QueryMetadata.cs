using System;

namespace ShareFile.Api.Client.Requests
{
    /// <summary>
    /// Used to store metadata about the query.
    /// This metadata is for internal use and is not actually sent as part of the API request.
    /// </summary>
    public class QueryMetadata
    {
        /// <summary>
        /// Gets or sets a value indicating whether or not the request is expected to throw.
        /// </summary>
        public bool IsExpectedToThrow { get; set; }
    }
}
