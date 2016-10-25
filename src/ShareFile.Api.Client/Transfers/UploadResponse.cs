using System.Collections.Generic;
using Newtonsoft.Json;

namespace ShareFile.Api.Client.Transfers
{
    public class UploadResponse : List<UploadedFile>
    {

    }

    public class UploadedFile
    {
        public string DisplayName { get; set; }
        public string Filename { get; set; }
        public string Id { get; set; }
        public string ParentId { get; set; }

        [JsonProperty("md5")]
        public string Hash { get; set; }
        /// <summary>
        /// The locally computed file hash.
        /// Note: we don't compute this value for all uploads (namely Standard uploads). It will be null in that case.
        /// </summary>
        public string LocalHash { get; set; }
        public long Size { get; set; }
        public string UploadId { get; set; }
    }
}
