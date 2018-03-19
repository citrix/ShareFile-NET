using ShareFile.Api.Client.Transfers.Uploaders.Buffers;

namespace ShareFile.Api.Client.Transfers.Uploaders
{
    internal class FilePart
    {
        public readonly IBuffer Bytes;
        public readonly int Index;
        public readonly long Offset;
        public readonly int Length;
        public readonly string Hash;
        public readonly bool IsLastPart;

        public FilePart(
            IBuffer bytes,
            int index,
            long offset,
            int length,
            string hash,
            bool isLastPart)
        {
            this.Bytes = bytes;
            this.Index = index;
            this.Offset = offset;
            this.Length = length;
            this.Hash = hash;
            this.IsLastPart = isLastPart;
        }
        
        public string GetComposedUploadUrl(string uploadUrl)
        {
            return $"{uploadUrl}&index={Index}&byteOffset={Offset}&hash={Hash}";
        }
    }
}
