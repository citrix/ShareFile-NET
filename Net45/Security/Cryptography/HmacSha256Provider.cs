using System;
using System.Security.Cryptography;

namespace ShareFile.Api.Client.Security.Cryptography
{
    public class HmacSha256Provider : IHmacSha256Provider
    {
        static HmacSha256Provider()
        {
            HmacSha256ProviderFactory.Register(bytes =>
            {
                return new HmacSha256Provider
                {
                    Key = bytes
                };
            });
        }

        public byte[] Key { get; set; }
        public byte[] ComputeHash(byte[] buffer)
        {
            var hmac = new HMACSHA256(Key);

            return hmac.ComputeHash(buffer);
        }
    }
}
