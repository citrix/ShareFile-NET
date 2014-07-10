using System.Collections.Generic;
using Newtonsoft.Json;

namespace ShareFile.Api.Client.Transfers
{
    public class UploadResponse : List<UploadedFile>
    {
        /// <summary>
        /// Not all supported upload methods will return upload information.
        /// Use this member to see if upload was successful, but no metadata is returned
        /// from the server.
        /// </summary>
        public static UploadResponse SuccessWithoutInformation = new UploadResponse
        {
            new UploadedFile()
        };
    }

    public class UploadedFile
    {
        public string DisplayName { get; set; }
        public string Filename { get; set; }
        [JsonProperty("ID")]
        public string Id { get; set; }
        public string Hash { get; set; }
        public long Size { get; set; }
        [JsonProperty("UploadID")]
        public string UploadId { get; set; }
    }
}
