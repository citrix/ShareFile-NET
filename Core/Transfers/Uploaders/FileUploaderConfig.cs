using System;

namespace ShareFile.Api.Client.Transfers.Uploaders
{
    public class FileUploaderConfig
    {
        public const int DefaultPartSize = 4*1024*1024;
        public const int DefaultNumberOfThreads = 1;
        public const int DefaultHttpTimeout = 60000;
        public const int MaxNumberOfThreads = 1;
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

        public FilePartConfig PartConfig { get; set; }

        public FileUploaderConfig()
        {
            NumberOfThreads = DefaultNumberOfThreads;
            PartSize = DefaultPartSize;
            HttpTimeout = DefaultHttpTimeout;
            ThreadStartPauseInMS = DefaultThreadStartPauseInMS;
            UseRequestStreamBuffering = true;
            RequireChunksCompleteInOrder = false;
            PartConfig = new FilePartConfig();
        }
    }

    public class FilePartConfig
    {
        public int InitialPartSize { get; set; }
        public int MaxPartSize { get; set; }
        public int MinPartSize { get; set; }
        public TimeSpan TargetPartUploadTime { get; set; }
        public int MaxPartSizeIncreaseFactor { get; set; }
        public int MaxPartSizeDecreaseFactor { get; set; }
        public int PartRetryCount { get; set; }

        public FilePartConfig()
        {
            InitialPartSize = 160 * 1024;
            MaxPartSize = 8 * 1024 * 1024;
            MinPartSize = 4 * 1024;
            TargetPartUploadTime = TimeSpan.FromSeconds(15);
            MaxPartSizeIncreaseFactor = 4;
            MaxPartSizeDecreaseFactor = 2;
            PartRetryCount = 1;
        }
    }
}
