using System;
using System.Collections.Generic;
using System.Globalization;
using ShareFile.Api.Client.Extensions;
using ShareFile.Api.Models;

namespace ShareFile.Api.Client.Transfers
{
    public class UploadSpecificationRequest
    {
        public Uri Parent { get; set; }
        public UploadMethod Method { get; set; }
        public bool Raw { get; set; }
        public string FileName { get; set; }
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
        public DateTime ClientCreatedDateUtc { get; set; }
        public DateTime ClientModifiedDateUtc { get; set; }

        public UploadSpecificationRequest()
        {
            ResponseFormat = "json";
            FileSize = 0;
            Tool = "apiv3";
            Method = UploadMethod.Threaded;
            ThreadCount = 1;
            Raw = true;
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
                    {"clientCreatedDateUTC", ClientCreatedDateUtc.ToString("u")},
                    {"clientModifiedDateUTC", ClientModifiedDateUtc.ToString("u")}
                };
        }
    }
}
