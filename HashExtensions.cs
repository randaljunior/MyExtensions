using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace MyExtensions
{
    public static partial class HashExtensions
    {
        public static string ComputeHashSHA512(this string input)
        {
            using (SHA512 SHA512 = new SHA512Managed())
            {
                var hash = SHA512.ComputeHash(Encoding.UTF8.GetBytes(input));

                var str = Helpers.ByteArrayToHexViaLookup32(hash);

                return str;
            }
        }

        public static string ComputeHashSHA1(this string input)
        {
            using (SHA1 SHA1 = new SHA1Managed())
            {
                var hash = SHA1.ComputeHash(Encoding.UTF8.GetBytes(input));

                var str = Helpers.ByteArrayToHexViaLookup32(hash);

                return str;
            }
        }


    }

    public static class Helpers
    {
        internal static readonly uint[] _lookup32 = CreateLookup32();

        private static uint[] CreateLookup32()
        {
            var result = new uint[256];
            for (int i = 0; i < 256; i++)
            {
                string s = i.ToString("X2");
                result[i] = ((uint)s[0]) + ((uint)s[1] << 16);
            }
            return result;
        }

        internal static string ByteArrayToHexViaLookup32(byte[] bytes)
        {
            var lookup32 = _lookup32;
            var result = new char[bytes.Length * 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                var val = lookup32[bytes[i]];
                result[2 * i] = (char)val;
                result[2 * i + 1] = (char)(val >> 16);
            }
            return new string(result);
        }
    }
}
