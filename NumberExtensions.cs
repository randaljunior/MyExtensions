using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyExtensions;

public static class NumberExtensions
{
    public static int? ToInt(this string text)
    {
        return text.AsSpan().ToInt();
    }

    public static int? ToInt(this char text)
    {
        return text.ToInt();
    }

    public static int? ToInt(this ReadOnlySpan<char> text)
    {
        return (int.TryParse(text, out int result)) ? result : null;
    }

    public static uint? ToUint(this string text)
    {
        return text.AsSpan().ToUint();
    }

    public static uint? ToUint(this char text)
    {
        return text.ToUint();
    }

    public static uint? ToUint(this ReadOnlySpan<char> text)
    {
        return (uint.TryParse(text, out uint result)) ? result : null;
    }

    public static uint? ToUint(this ReadOnlySpan<uint> numbers)
    {
        if (numbers.Length == 0 || numbers.Length > 10) return null;

        uint result = 0;
        uint multiplier = 1;

        for (int i = numbers.Length - 1; i >= 0; i--)
        {
            result += numbers[i] * multiplier;
            multiplier *= 10;
        }
        return result;
    }


    public static long? ToLong(this string text)
    {
        return text.AsSpan().ToLong();
    }

    public static long? ToLong(this char text)
    {
        return text.ToLong();
    }

    public static long? ToLong(this ReadOnlySpan<char> text)
    {
        return (long.TryParse(text, out long result)) ? result : null;
    }


    public static ulong? ToUlong(this string text)
    {
        return text.AsSpan().ToUlong();
    }

    public static ulong? ToUlong(this char text)
    {
        return text.ToUlong();
    }

    public static ulong? ToUlong(this ReadOnlySpan<char> text)
    {
        return (ulong.TryParse(text, out ulong result)) ? result : null;
    }

    public static ulong? ToUlong(this ReadOnlySpan<uint> numbers)
    {
        if (numbers.Length == 0 || numbers.Length > 20) return null;

        ulong result = 0;
        ulong multiplier = 1;

        for (int i = numbers.Length - 1; i >= 0; i--)
        {
            result += numbers[i] * multiplier;
            multiplier *= 10;
        }
        return result;
    }


    public static ReadOnlySpan<uint> GetDigits(this int source, int numDigits = 0)
    {
        Span<uint> _ints = new uint[
            (numDigits == 0)
            ? (uint)Math.Floor(Math.Log10(source) + 1)
            : numDigits
            ];

        for (int i = _ints.Length - 1; i > 0; i--)
        {
            _ints[i] = (uint)(source % 10);
            source /= 10;
        }

        return _ints;
    }

    public static ReadOnlySpan<uint> GetDigits(this int? source, int numDigits = 0)
    {
        return GetDigits(source ?? 0, numDigits);
    }

    public static ReadOnlySpan<uint> GetDigits(this uint source, int numDigits = 0)
    {
        Span<uint> _ints = new uint[
            (numDigits == 0)
            ? (uint)Math.Floor(Math.Log10(source) + 1)
            : numDigits
            ];

        for (int i = _ints.Length - 1; i > 0; i--)
        {
            _ints[i] = (uint)(source % 10);
            source /= 10;
        }

        return _ints;
    }

    public static ReadOnlySpan<uint> GetDigits(this uint? source, int numDigits = 0)
    {
        return GetDigits(source ?? 0, numDigits);
    }

    public static ReadOnlySpan<uint> GetDigits(this ulong source, int numDigits = 0)
    {
        Span<uint> _ints = new uint[
            (numDigits == 0)
            ? (uint)Math.Floor(Math.Log10(source) + 1)
            : numDigits
            ];

        for (int i = _ints.Length - 1; i > 0; i--)
        {
            _ints[i] = (uint)(source % 10);
            source /= 10;
        }

        return _ints;
    }

    public static ReadOnlySpan<uint> GetDigits(this ulong? source, int numDigits = 0)
    {
        return GetDigits(source ?? 0, numDigits);
    }

    public static ReadOnlySpan<uint> GetDigits(this ReadOnlySpan<char> source)
    {
        Span<uint> buffer = stackalloc uint[source.Length];
        int index = 0;

        foreach (char c in source)
        {
            if (char.IsDigit(c))
            {
                buffer[index++] = (uint)(c - '0');
            }
        }

        Span<uint> result = new uint[index];
        buffer.Slice(0, index).CopyTo(result);

        return result;
    }
}
