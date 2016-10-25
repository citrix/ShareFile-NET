namespace ShareFile.Api.Client.Transfers.Downloaders
{
    public class DownloaderConfig
    {
        public RangeRequest RangeRequest { get; set; }
        public int BufferSize { get; set; }

        public DownloaderConfig()
        {
            RangeRequest = null;
            BufferSize = 4096;
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
