namespace ShareFile.Api.Client.Transfers.Uploaders
{
    public class FilePart
    {
        public byte[] Bytes { get; set; }
        public int Index { get; set; }
        public long Offset { get; set; }
        public int Length { get; set; }
        public string UploadUrl { get; set; }
        public string Hash { get; internal set; }
        public bool IsLastPart { get; set; }

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
