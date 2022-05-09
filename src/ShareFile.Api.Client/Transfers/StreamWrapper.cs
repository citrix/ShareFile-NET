using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ShareFile.Api.Client.Transfers
{
    internal abstract class StreamWrapper : Stream
    {
        protected readonly Stream stream;

        public StreamWrapper(Stream stream)
        {
            this.stream = stream;
        }

        public override int Read(byte[] buffer, int offset, int count) => stream.Read(buffer, offset, count);
        public override void Write(byte[] buffer, int offset, int count) => stream.Write(buffer, offset, count);
        public override bool CanRead => stream.CanRead;
        public override bool CanSeek => stream.CanSeek;
        public override bool CanWrite => stream.CanWrite;
        public override long Length => stream.Length;
        public override long Position { get => stream.Position; set { stream.Position = value; } }
        public override void Flush() => stream.Flush();
        public override long Seek(long offset, SeekOrigin origin) => stream.Seek(offset, origin);
        public override void SetLength(long value) => stream.SetLength(value);
        public override bool CanTimeout => stream.CanTimeout;
        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
            => stream.CopyToAsync(destination, bufferSize, cancellationToken);
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => stream.ReadAsync(buffer, offset, count, cancellationToken);
        public override int ReadByte() => stream.ReadByte();
        public override int ReadTimeout { get => stream.ReadTimeout; set => stream.ReadTimeout = value; }
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => stream.WriteAsync(buffer, offset, count, cancellationToken);
        public override void WriteByte(byte value) => stream.WriteByte(value);
        public override int WriteTimeout { get => stream.WriteTimeout; set => stream.WriteTimeout = value; }
        public override Task FlushAsync(CancellationToken cancellationToken) => stream.FlushAsync(cancellationToken);
#if !PORTABLE && !NETSTANDARD1_3
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            => stream.BeginRead(buffer, offset, count, callback, state);
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            => stream.BeginWrite(buffer, offset, count, callback, state);
        public override void Close() => stream.Close();
        //public System.Runtime.Remoting.ObjRef CreateObjRef(Type requestedType) => stream.CreateObjRef(requestedType);
        public override int EndRead(IAsyncResult asyncResult) => stream.EndRead(asyncResult);
        public override void EndWrite(IAsyncResult asyncResult) => stream.EndWrite(asyncResult);
        public override object InitializeLifetimeService() => stream.InitializeLifetimeService();
#endif
    }
}