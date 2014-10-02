using System;

namespace ShareFile.Api.Client.Transfers.Uploaders
{
    public class FileUploaderConfig
    {
        public const int DefaultPartSize = 4*1024*1024;
        public const int DefaultNumberOfThreads = 4;
        public const int DefaultHttpTimeout = 60000;
        public const int DefaultHttpTimeoutBackoffFactor = 2;
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
        public int HttpTimeoutBackoffFactor { get; set; }
        public int ThreadStartPauseInMS { get; set; }
        public bool UseRequestStreamBuffering { get; set; }
        public bool RequireChunksCompleteInOrder { get; set; }
        public int? WriteTimeout { get; set; }
        public int? ReadTimeout { get; set; }

        public FileChunkConfig ChunkConfig { get; set; }

        public FileUploaderConfig()
        {
            NumberOfThreads = DefaultNumberOfThreads;
            PartSize = DefaultPartSize;
            HttpTimeout = DefaultHttpTimeout;
            HttpTimeoutBackoffFactor = DefaultHttpTimeoutBackoffFactor;
            ThreadStartPauseInMS = DefaultThreadStartPauseInMS;
            UseRequestStreamBuffering = true;
            RequireChunksCompleteInOrder = false;
            ChunkConfig = new FileChunkConfig();
        }
    }

    public class FileChunkConfig
    {
        public int InitialChunkSize { get; set; }
        public int MaxChunkSize { get; set; }
        public int MinChunkSize { get; set; }
        public TimeSpan TargetChunkUploadTime { get; set; }
        public int MaxChunkIncreaseFactor { get; set; }
        public int MaxChunkDecreaseFactor { get; set; }
        public int ChunkRetryCount { get; set; }

        public FileChunkConfig()
        {
            InitialChunkSize = 160 * 1024;
            MaxChunkSize = 8 * 1024 * 1024;
            MinChunkSize = 4 * 1024;
            TargetChunkUploadTime = TimeSpan.FromSeconds(15);
            MaxChunkIncreaseFactor = 4;
            MaxChunkDecreaseFactor = 2;
            ChunkRetryCount = 1;
        }
    }
}
