using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
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
    public static string ComputeHashSHA512([DisallowNull] this string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        Span<byte> hash = stackalloc byte[64];        
        SHA512.HashData(Encoding.UTF8.GetBytes(input.AsReadOnlySequence()),hash);

        return HashHelpers.ByteArrayToHex(hash);
    }

    /// <summary>
    /// Computes the SHA256 hash of the input string and returns it as a hexadecimal string.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string ComputeHashSHA1([DisallowNull] this string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        Span<byte> hash = stackalloc byte[64];
        SHA512.HashData(Encoding.UTF8.GetBytes(input.AsReadOnlySequence()), hash);

        return HashHelpers.ByteArrayToHex(hash);
    }
}

public static class HashHelpers
{
    private static uint[]? _lookup32;
    private static Vector256<byte>? _hexCharsLower;
    private static Vector256<byte>? _hexCharsUpper;

    public static void CreateLookup32()
    {
        _lookup32 = new uint[256];
        for (int i = 0; i < 256; i++)
        {
            string s = i.ToString("X2");
            _lookup32[i] = ((uint)s[0]) + ((uint)s[1] << 16);
        }

        if (Avx2.IsSupported)
        {
            _hexCharsLower = Vector256.Create((byte)'0', (byte)'1', (byte)'2', (byte)'3',
                                            (byte)'4', (byte)'5', (byte)'6', (byte)'7',
                                            (byte)'8', (byte)'9', (byte)'a', (byte)'b',
                                            (byte)'c', (byte)'d', (byte)'e', (byte)'f',
                                            (byte)'0', (byte)'1', (byte)'2', (byte)'3',
                                            (byte)'4', (byte)'5', (byte)'6', (byte)'7',
                                            (byte)'8', (byte)'9', (byte)'a', (byte)'b',
                                            (byte)'c', (byte)'d', (byte)'e', (byte)'f');

            _hexCharsUpper = Vector256.Create((byte)'0', (byte)'1', (byte)'2', (byte)'3',
                                            (byte)'4', (byte)'5', (byte)'6', (byte)'7',
                                            (byte)'8', (byte)'9', (byte)'A', (byte)'B',
                                            (byte)'C', (byte)'D', (byte)'E', (byte)'F',
                                            (byte)'0', (byte)'1', (byte)'2', (byte)'3',
                                            (byte)'4', (byte)'5', (byte)'6', (byte)'7',
                                            (byte)'8', (byte)'9', (byte)'A', (byte)'B',
                                            (byte)'C', (byte)'D', (byte)'E', (byte)'F');
        }
    }

    /// <summary>
    /// Converts a byte array to a hexadecimal string using the best suitable method.
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="uppercase"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ByteArrayToHex(Span<byte> bytes, bool uppercase = true)
    {
        if (Avx2.IsSupported && bytes.Length >= 32)
        {
            return ByteArrayToHexAvx2(bytes, uppercase);
        }

        return ByteArrayToHexViaLookup32(bytes);
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

    /// <summary>
    /// Converts a byte array to a hexadecimal string using a AVX2 instructions.
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="uppercase"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe string ByteArrayToHexAvx2(Span<byte> bytes, bool uppercase = true)
    {
        if (_lookup32 is null) CreateLookup32();

        Span<char> result =
            (bytes.Length * 2) <= 1024
                ? stackalloc char[bytes.Length * 2]
                : new char[bytes.Length * 2];

        var hexChars = uppercase ? _hexCharsUpper!.Value : _hexCharsLower!.Value;

        fixed (byte* bytesPtr = bytes)
        fixed (char* resultPtr = result)
        {
            int vectorSize = Vector256<byte>.Count; // 32 bytes
            int vectorizedLength = bytes.Length - (bytes.Length % vectorSize);

            // Processar em blocos de 32 bytes usando AVX2
            for (int i = 0; i < vectorizedLength; i += vectorSize)
            {
                // Carregar 32 bytes
                var inputVector = Avx.LoadVector256(bytesPtr + i);

                // Separar os nibbles alto e baixo
                var highNibbles = Avx2.ShiftRightLogical(inputVector.AsUInt32(), 4).AsByte();
                var lowNibbles = Avx2.And(inputVector, Vector256.Create((byte)0x0F));

                // Converter para caracteres hex usando shuffle
                var highHexChars = Avx2.Shuffle(hexChars, highNibbles);
                var lowHexChars = Avx2.Shuffle(hexChars, lowNibbles);

                // Intercalar os caracteres (high, low, high, low, ...)
                var result1 = Avx2.UnpackLow(highHexChars, lowHexChars);
                var result2 = Avx2.UnpackHigh(highHexChars, lowHexChars);

                // Converter para chars (16-bit) e armazenar
                var chars1 = Avx2.UnpackLow(result1.AsUInt16(), Vector256<ushort>.Zero);
                var chars2 = Avx2.UnpackHigh(result1.AsUInt16(), Vector256<ushort>.Zero);
                var chars3 = Avx2.UnpackLow(result2.AsUInt16(), Vector256<ushort>.Zero);
                var chars4 = Avx2.UnpackHigh(result2.AsUInt16(), Vector256<ushort>.Zero);

                Avx.Store((ushort*)(resultPtr + i * 2), chars1.AsUInt16());
                Avx.Store((ushort*)(resultPtr + i * 2 + 16), chars2.AsUInt16());
                Avx.Store((ushort*)(resultPtr + i * 2 + 32), chars3.AsUInt16());
                Avx.Store((ushort*)(resultPtr + i * 2 + 48), chars4.AsUInt16());
            }

            // Processar bytes restantes com lookup table
            for (int i = vectorizedLength; i < bytes.Length; i++)
            {
                var val = _lookup32![bytes[i]];
                result[2 * i] = (char)val;
                result[2 * i + 1] = (char)(val >> 16);
            }
        }

        return result.ToString();
    }

}
