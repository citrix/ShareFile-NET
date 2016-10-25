namespace ShareFile.Api.Client.Transfers.Uploaders
{
    public class FilePart
    {
        public byte[] Bytes { get; private set; }
        public int Index { get; private set; }
        public long Offset { get; private set; }
        public int Length { get; private set; }
        public string UploadUrl { get; private set; }
        public string Hash { get; private set; }
        public bool IsLastPart { get; private set; }

        public FilePart(
            byte[] bytes,
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

        public string GetComposedUploadUrl()
        {
            return GetComposedUploadUrl(this.UploadUrl);
        }

        public string GetComposedUploadUrl(string uploadUrl)
        {
            return string.Format("{0}&index={1}&byteOffset={2}&hash={3}", uploadUrl, Index, Offset, Hash);
        }
    }
}
