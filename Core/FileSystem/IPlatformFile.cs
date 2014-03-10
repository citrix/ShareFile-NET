using System;
using System.IO;
using System.Threading.Tasks;

namespace ShareFile.Api.Client.FileSystem
{
    public interface IPlatformFile : IDisposable
    {
        string FullName { get; }
        string Name { get; }
        long Length { get; }

        Task<Stream> OpenReadAsync();
        Task<Stream> OpenWriteAsync();

        Stream OpenRead();
        Stream OpenWrite();
    }
}
