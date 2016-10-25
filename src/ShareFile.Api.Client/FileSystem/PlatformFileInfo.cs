#if NET45
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShareFile.Api.Client.FileSystem
{
    public class PlatformFileInfo : IPlatformFile
    {
        public void Dispose()
        {
            
        }

        public PlatformFileInfo(FileInfo fileInfo)
        {
            _fileInfo = fileInfo;

            FullName = _fileInfo.FullName;
            Name = _fileInfo.Name;
            Length = _fileInfo.Length;
        }

        private readonly FileInfo _fileInfo;

        public string FullName { get; private set; }
        public string Name { get; private set; }
        public long Length { get; private set; }

        public Task<Stream> OpenReadAsync()
        {
            return Task.FromResult((Stream)_fileInfo.OpenRead());
        }

        public Task<Stream> OpenWriteAsync()
        {
            return Task.FromResult((Stream)_fileInfo.OpenWrite());
        }

        public Stream OpenRead()
        {
            return _fileInfo.OpenRead();
        }

        public Stream OpenWrite()
        {
            return _fileInfo.OpenWrite();
        }
    }
}
#endif