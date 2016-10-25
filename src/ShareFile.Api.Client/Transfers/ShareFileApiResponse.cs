namespace ShareFile.Api.Client.Transfers
{
    internal interface ISFApiErrorResponse
    {
        bool? Error { get; set; }
        string ErrorMessage { get; set; }
        int ErrorCode { get; set; }
    }

    internal class ShareFileApiResponse<T> : ISFApiErrorResponse
    {
        public bool? Error { get; set; }
        public string ErrorMessage { get; set; }
        public int ErrorCode { get; set; }
        public T Value { get; set; }
    }
}
