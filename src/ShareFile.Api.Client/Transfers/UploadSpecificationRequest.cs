using System;
using System.Collections.Generic;
using System.Globalization;
using ShareFile.Api.Client.Extensions;
using ShareFile.Api.Client.Models;

namespace ShareFile.Api.Client.Transfers
{
    public class UploadSpecificationRequest
    {
        public Uri Parent { get; set; }
        public UploadMethod? Method { get; set; }

        /// <summary>
        /// Set is no longer supported; value will be overwritten during uploader constructor
        /// </summary>
        public bool Raw { get; set; }

        public string FileName { get; set; }

        public string BaseFileId { get; set; }
        public long FileSize { get; set; }
        public string BatchId { get; set; }
        public bool BatchLast { get; set; }
        public bool CanResume { get; set; }
        public bool StartOver { get; set; }
        public bool Unzip { get; set; }
        public string Tool { get; set; }
        public bool Overwrite { get; set; }
        public string Title { get; set; }
        public string Details { get; set; }
        public bool IsSend { get; set; }
        public string SendGuid { get; set; }
        public bool Notify { get; set; }
        public int ThreadCount { get; set; }
        public string ResponseFormat { get; private set; }
        public DateTime? ClientCreatedDateUtc { get; set; }
        public DateTime? ClientModifiedDateUtc { get; set; }
        public IEnumerable<Capability> ProviderCapabilities { get; set; }

        /// <summary>
        /// Will make a best effort to ensure a file is uploaded by modifying 
        /// FileName if it encounters a collision.  This is NOT supported on all
        /// providers.
        /// </summary>
        public bool ForceUnique { get; set; }

        public UploadSpecificationRequest()
        {
            ResponseFormat = "json";
            FileSize = 0;
            ThreadCount = 1;
            Raw = true;
        }

        public UploadSpecificationRequest(string fileName, long fileSize, Uri parent)
            : this()
        {
            FileName = fileName;
            FileSize = fileSize;
            Parent = parent;
        }

        public UploadSpecificationRequest(string fileName, long fileSize, Uri parent, UploadMethod method)
            : this(fileName, fileSize, parent)
        {
            Method = method;
        }

        public virtual IDictionary<string, string> ToDictionary()
        {
            return new Dictionary<string, string>
                {
                    {"method", Method.ToString()},
                    {"raw", Raw.ToLowerString()},
                    {"fileName", FileName ?? ""},
                    {"fileSize", FileSize.ToString(CultureInfo.InvariantCulture)},
                    {"batchId", BatchId ?? ""},
                    {"batchLast", BatchLast.ToLowerString()},
                    {"canResume", CanResume.ToLowerString()},
                    {"startOver", StartOver.ToLowerString()},
                    {"unzip", Unzip.ToLowerString()},
                    {"tool", Tool},
                    {"title", Title ?? ""},
                    {"details", Details ?? ""},
                    {"sendGuid", SendGuid ?? ""},
                    {"threadCount", Convert.ToString(ThreadCount)},
                    {"overwrite", Overwrite.ToLowerString()},
                    {"isSend", IsSend.ToLowerString()},
                    {"responseFormat", ResponseFormat},
                    {"notify", Notify.ToLowerString()},
                    {"clientCreatedDateUTC", ClientCreatedDateUtc.HasValue ? ClientCreatedDateUtc.Value.ToString("u"): ""},
                    {"clientModifiedDateUTC", ClientModifiedDateUtc.HasValue ? ClientModifiedDateUtc.Value.ToString("u"): ""},
                    {"baseFileId", BaseFileId}
            };
        }

        /// <summary>
        /// Convert to <see cref="UploadRequestParams"/> used for Upload2.
        /// </summary>
        /// <returns></returns>
        public UploadRequestParams ToRequestParams()
        {
            return new UploadRequestParams
            {
                Method = this.Method.GetValueOrDefault(),
                Raw = this.Raw,
                FileName = this.FileName,
                FileSize = this.FileSize,
                BatchId = this.BatchId,
                BatchLast = this.BatchLast,
                CanResume = this.CanResume,
                StartOver = this.StartOver,
                Unzip = this.Unzip,
                Title = this.Title,
                Details = this.Details,
                SendGuid = this.SendGuid,
                ThreadCount = this.ThreadCount,
                IsSend = this.IsSend,
                Notify = this.Notify,
                ClientCreatedDate = this.ClientCreatedDateUtc,
                ClientModifiedDate = this.ClientModifiedDateUtc,
                Tool = this.Tool,
                Overwrite = Overwrite,
                BaseFileId = BaseFileId
            };
        }
    }
}
