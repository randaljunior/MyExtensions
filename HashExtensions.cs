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
            ArgumentNullException.ThrowIfNull(input);
            Span<byte> hash = SHA512.HashData(Encoding.UTF8.GetBytes(input));
            return Helpers.ByteArrayToHexViaLookup32(hash);
        }

        public static string ComputeHashSHA1(this string input)
        {
            ArgumentNullException.ThrowIfNull(input);
            Span<byte> hash = SHA1.HashData(Encoding.UTF8.GetBytes(input));
            return Helpers.ByteArrayToHexViaLookup32(hash);
        }


    }

    public static class Helpers
    {
        private static readonly uint[] Lookup32 = CreateLookup32();

        private static uint[] CreateLookup32()
        {
            var lookup = new uint[256];
            for (int i = 0; i < 256; i++)
            {
                string s = i.ToString("X2");
                lookup[i] = ((uint)s[0]) + ((uint)s[1] << 16);
            }
            return lookup;
        }

        internal static string ByteArrayToHexViaLookup32(Span<byte> bytes)
        {
            Span<char> result =
                (bytes.Length * 2) <= 1024
                    ? stackalloc char[bytes.Length * 2]
                    : new char[bytes.Length * 2];

            for (int i = 0; i < bytes.Length; i++)
            {
                var val = Lookup32[bytes[i]];
                result[2 * i] = (char)val;
                result[2 * i + 1] = (char)(val >> 16);
            }

            return result.ToString();
        }
    }
}
