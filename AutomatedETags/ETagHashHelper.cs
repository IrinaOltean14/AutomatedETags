using System;
using System.Collections.Generic;
using System.IO.Hashing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AutomatedETags
{
    internal static class ETagHashHelper
    {
        public static string GenerateRawHashFromString(string input, ETagAlgorithm algorithm)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes;

            if (algorithm == ETagAlgorithm.XxHash64)
            {
                hashBytes = XxHash64.Hash(inputBytes);
            }
            else
            {
                using HashAlgorithm hasher = algorithm switch
                {
                    ETagAlgorithm.MD5 => MD5.Create(),
                    _ => SHA256.Create()
                };
                hashBytes = hasher.ComputeHash(inputBytes);
            }

            var sb = new StringBuilder();
            foreach (byte b in hashBytes) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }
}
