using System;
using System.Net;
using System.Net.Http;
using ShareFile.Api.Client.Credentials;
using ShareFile.Api.Client.Transfers.Uploaders.Buffers;

namespace ShareFile.Api.Client.Transfers.Uploaders
{
    public class FileUploaderConfig
    {
        public const int DefaultPartSize = 4*1024*1024;
        public const int DefaultNumberOfThreads = 1;
        public const int DefaultHttpTimeout = 60000;
        public const int DefaultThreadStartPauseInMS = 100;
        private const int DefaultProgressReportIntervalMilliseconds = 100;

        public int NumberOfThreads { get; set; }
        public int PartSize { get; set; }
        public int HttpTimeout { get; set; }
        public Func<ICredentialCache, CookieContainer, HttpClient> HttpClientFactory { get; set; }
        public int ThreadStartPauseInMS { get; set; }
        public bool UseRequestStreamBuffering { get; set; }
        public bool RequireChunksCompleteInOrder { get; set; }
        public int? WriteTimeout { get; set; }
        public int? ReadTimeout { get; set; }
        public TimeSpan ProgressReportInterval { get; set; }

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
            ProgressReportInterval = TimeSpan.FromMilliseconds(DefaultProgressReportIntervalMilliseconds);
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
        /// <summary>
        /// Files smaller than this size will always be single-threaded. Defaults to 8MB.
        /// <para>
        /// The number of threads is scaled based on file size. 8MB will be 2 threads, 16MB will be 3 threads, etc.
        /// </para>
        /// </summary>
        public int MinFileSizeForMultithreaded { get; set; }
        public IBufferAllocator BufferAllocator { get; set; }

        public FilePartConfig()
        {
            InitialPartSize = 512 * 1024;
            MaxPartSize = 8 * 1024 * 1024;
            MinPartSize = 4 * 1024;
            MinFileSizeForMultithreaded = 8 * 1024 * 1024;
            TargetPartUploadTime = TimeSpan.FromSeconds(15);
            MaxPartSizeIncreaseFactor = 8;
            MaxPartSizeDecreaseFactor = 2;
            PartRetryCount = 1;
            BufferAllocator = new PooledBufferAllocator();
        }
    }
}
