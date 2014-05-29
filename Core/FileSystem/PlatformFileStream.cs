using System;
using System.IO;
using System.Threading.Tasks;

namespace ShareFile.Api.Client.FileSystem
{
    public sealed class PlatformFileStream : IPlatformFile
    {
        public PlatformFileStream(Stream stream, long length, string name, string fullName = null)
        {
            Stream = stream;
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
            if (Stream != null)
            {
                Stream.Dispose();
            }
            GC.SuppressFinalize(this);
        }

        private Stream Stream { get; set; }
        public string FullName { get; private set; }
        public string Name { get; private set; }
        public long Length { get; private set; }

#if async
        public async Task<Stream> OpenReadAsync()
        {
            if (Stream != null)
            {
                if (Stream.CanSeek)
                {
                    return Stream;
                }

                throw new Exception("Stream is not seekable.");
            }

            throw new Exception("Stream has not been provided.");
        }

        public async Task<Stream> OpenWriteAsync()
        {
            if (Stream.CanWrite)
            {
                return Stream;
            }

            throw new Exception("Stream is not writable.");
        }
#endif

        public Stream OpenRead()
        {
            if (Stream != null)
            {
                if (Stream.CanSeek)
                {
                    return Stream;
                }

                throw new Exception("Stream is not seekable.");
            }

            throw new Exception("Stream has not been provided.");
        }

        public Stream OpenWrite()
        {
            if (Stream.CanWrite)
            {
                return Stream;
            }

            throw new Exception("Stream is not writable.");
        }
    }
}
