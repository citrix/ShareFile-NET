using System;
using System.IO;
using System.Threading.Tasks;

namespace ShareFile.Api.Client.FileSystem
{
    public sealed class PlatformFileStream : IPlatformFile
    {
        public PlatformFileStream(Stream stream, long length, string name, string fullName = null)
        {
            FStream = stream;
            Length = length;
            Name = name;
            FullName = fullName ?? name;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public void Dispose(bool disposeAllResources)
        {
            if (FStream != null)
            {
                FStream.Dispose();
            }
            GC.SuppressFinalize(this);
        }

        private readonly Stream FStream;
        public string FullName { get; private set; }
        public string Name { get; private set; }
        public long Length { get; private set; }
        public DateTimeOffset LastModified { get; private set; }

#if ASYNC
        public Task<Stream> OpenReadAsync()
        {
            if (FStream != null)
            {
                if (FStream.CanSeek)
                {
                    return Task.FromResult(FStream);
                }

                throw new Exception("Stream is not seekable.");
            }

            throw new Exception("Stream has not been provided.");
        }

        public Task<Stream> OpenWriteAsync()
        {
            if (FStream.CanWrite)
            {
                return Task.FromResult(FStream);
            }

            throw new Exception("Stream is not writable.");
        }
#endif

        public Stream OpenRead()
        {
            if (FStream != null)
            {
                if (FStream.CanSeek)
                {
                    return FStream;
                }

                throw new Exception("Stream is not seekable.");
            }

            throw new Exception("Stream has not been provided.");
        }

        public Stream OpenWrite()
        {
            if (FStream.CanWrite)
            {
                return FStream;
            }

            throw new Exception("Stream is not writable.");
        }
    }
}