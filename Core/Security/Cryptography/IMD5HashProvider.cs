namespace ShareFile.Api.Client.Security.Cryptography
{
// ReSharper disable InconsistentNaming
    public interface IMD5HashProvider
// ReSharper restore InconsistentNaming
    {
        IMD5HashProvider CreateHash();
        void Append(byte[] buffer, int offset, int size);
        void Finalize(byte[] buffer, int offset, int size);
        string ComputeHash(byte[] buffer);
        string GetComputedHashAsString();
        byte[] GetComputedHash();
    }
}
