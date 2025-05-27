using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace MyExtensions;

public static partial class HashExtensions
{
    /// <summary>
    /// Computes the SHA512 hash of the input string and returns it as a hexadecimal string.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string ComputeHashSHA512(this string input)
    {
        ArgumentNullException.ThrowIfNull(input);
        Span<byte> hash = SHA512.HashData(Encoding.UTF8.GetBytes(input));
        return Helpers.ByteArrayToHexViaLookup32(hash);
    }

    /// <summary>
    /// Computes the SHA256 hash of the input string and returns it as a hexadecimal string.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string ComputeHashSHA1(this string input)
    {
        ArgumentNullException.ThrowIfNull(input);
        Span<byte> hash = SHA1.HashData(Encoding.UTF8.GetBytes(input));
        return Helpers.ByteArrayToHexViaLookup32(hash);
    }


}

public static class Helpers
{
    private static uint[]? _lookup32;

    private static void CreateLookup32()
    {
        _lookup32 = new uint[256];
        for (int i = 0; i < 256; i++)
        {
            string s = i.ToString("X2");
            _lookup32[i] = ((uint)s[0]) + ((uint)s[1] << 16);
        }
    }

    /// <summary>
    /// Converts a byte array to a hexadecimal string using a lookup table for performance.
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static string ByteArrayToHexViaLookup32(Span<byte> bytes)
    {
        if (_lookup32 is null) CreateLookup32();

        Span<char> result =
            (bytes.Length * 2) <= 1024
                ? stackalloc char[bytes.Length * 2]
                : new char[bytes.Length * 2];

        for (int i = 0; i < bytes.Length; i++)
        {
            var val = _lookup32![bytes[i]];
            result[2 * i] = (char)val;
            result[2 * i + 1] = (char)(val >> 16);
        }

        return result.ToString();
    }
}
