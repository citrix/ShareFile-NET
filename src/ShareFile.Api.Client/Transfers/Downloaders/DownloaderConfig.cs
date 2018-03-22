using System;

namespace ShareFile.Api.Client.Transfers.Downloaders
{
    public class DownloaderConfig
    {
        public RangeRequest RangeRequest { get; set; }
        [Obsolete]
        public int BufferSize { get => Configuration.BufferSize; set { return; } }
        public TimeSpan ProgressReportInterval { get; set; }
        public bool AllowRangeRequestOffByOne { get; set; }

        public DownloaderConfig()
        {
            RangeRequest = null;
            ProgressReportInterval = TimeSpan.FromMilliseconds(100);
            AllowRangeRequestOffByOne = true;
        }

        public static DownloaderConfig Default
        {
            get
            {
                return new DownloaderConfig();
            }
        }
    }

    public class RangeRequest
    {
        public long? Begin { get; set; }
        public long? End { get; set; }
    }
}
