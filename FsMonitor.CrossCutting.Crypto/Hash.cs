using System;
using System.Linq;
using System.Security.Cryptography;

namespace FsMonitor.CrossCutting.Crypto
{
    public static class Hash
    {
        private static byte[] ComputeSHA1(byte[] input)
        {
            using (var hash = new SHA1Managed()) {
                return hash.ComputeHash(input);
            }
        }
        public static string SHA1ToHex(this byte[] input)
        {
            return string.Concat(ComputeSHA1(input).Select(b => b.ToString("x2")));
        }
        public static string SHA1ToBase64(this byte[] input)
        {
            return Convert.ToBase64String(ComputeSHA1(input));
        }
    }
}
