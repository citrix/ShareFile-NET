namespace ShareFile.Api.Client.Transfers.Uploaders
{
    public class FileUploaderConfig
    {
        public const int DefaultPartSize = 4*1024*1024;
        public const int DefaultNumberOfThreads = 4;
        public const int DefaultHttpTimeout = 600000;
        public const int MaxNumberOfThreads = 4;
        public const int DefaultThreadStartPauseInMS = 100;

        private int _numberOfThreads;

        public int NumberOfThreads
        {
            get { return _numberOfThreads; }
            set { _numberOfThreads = value > MaxNumberOfThreads ? MaxNumberOfThreads : value; }
        }

        public int PartSize { get; set; }
        public int HttpTimeout { get; set; }
        public int ThreadStartPauseInMS { get; set; }
        public bool UseRequestStreamBuffering { get; set; }
        public bool RequireChunksCompleteInOrder { get; set; }
        public int? WriteTimeout { get; set; }
        public int? ReadTimeout { get; set; }

        public FileUploaderConfig()
        {
            NumberOfThreads = DefaultNumberOfThreads;
            PartSize = DefaultPartSize;
            HttpTimeout = DefaultHttpTimeout;
            ThreadStartPauseInMS = DefaultThreadStartPauseInMS;
            UseRequestStreamBuffering = true;
            RequireChunksCompleteInOrder = false;
        }
    }
}
