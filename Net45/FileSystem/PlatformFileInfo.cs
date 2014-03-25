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

        public async Task<Stream> OpenReadAsync()
        {
            return _fileInfo.OpenRead();
        }

        public async Task<Stream> OpenWriteAsync()
        {
            return _fileInfo.OpenWrite();
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
