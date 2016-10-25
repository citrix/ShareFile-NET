namespace ShareFile.Api.Client.Security.Cryptography
{
    public interface IHmacSha256Provider
    {
        byte[] Key { get; set; }
        byte[] ComputeHash(byte[] buffer);
    }
}
