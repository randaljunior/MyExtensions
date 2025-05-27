using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyExtensions;

public static class NumberExtensions
{
    /// <summary>
    /// Converts a string to an integer. Returns null if the conversion fails.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static int? ToInt(this string text)
    {
        return text.AsSpan().ToInt();
    }

    /// <summary>
    /// Converts a char to an integer. Returns null if the conversion fails.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static int? ToInt(this char text)
    {
        return text.ToInt();
    }

    /// <summary>
    /// Converts a ReadOnlySpan<char> to an integer. Returns null if the conversion fails.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static int? ToInt(this ReadOnlySpan<char> text)
    {
        return (int.TryParse(text, out int result)) ? result : null;
    }

    /// <summary>
    /// Converts a ReadOnlySpan<char> to an integer. Returns null if the conversion fails.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static int? ToInt(this ReadOnlySpan<uint> numbers)
    {
        if (numbers.Length == 0 || numbers.Length > 10) return null;

        int sum = 0;
        int multiplier = 1;

        for (int i = numbers.Length - 1; i >= 0; i--)
        {
            sum += (int)(numbers[i] * multiplier);
            multiplier *= 10;
        }

        return sum;
    }

    /// <summary>
    /// Converts a ReadOnlySpan<uint> to an integer. Returns null if the conversion fails.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static uint? ToUint(this string text)
    {
        return text.AsSpan().ToUint();
    }

    /// <summary>
    /// Converts a char to an unsigned integer. Returns null if the conversion fails.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static uint? ToUint(this char text)
    {
        return text.ToUint();
    }

    /// <summary>
    /// Converts a ReadOnlySpan<char> to an unsigned integer. Returns null if the conversion fails.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static uint? ToUint(this ReadOnlySpan<char> text)
    {
        return (uint.TryParse(text, out uint result)) ? result : null;
    }

    /// <summary>
    /// Converts a ReadOnlySpan<uint> to an unsigned integer. Returns null if the conversion fails.
    /// </summary>
    /// <param name="numbers"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Converts a string to a long. Returns null if the conversion fails.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static long? ToLong(this string text)
    {
        return text.AsSpan().ToLong();
    }

    /// <summary>
    /// Converts a char to a long. Returns null if the conversion fails.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static long? ToLong(this char text)
    {
        return text.ToLong();
    }

    /// <summary>
    /// Converts a ReadOnlySpan<char> to a long. Returns null if the conversion fails.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static long? ToLong(this ReadOnlySpan<char> text)
    {
        return (long.TryParse(text, out long result)) ? result : null;
    }

    /// <summary>
    /// Converts a ReadOnlySpan<uint> to an long. Returns null if the conversion fails.
    /// </summary>
    /// <param name="numbers"></param>
    /// <returns></returns>
    public static long? ToLong(this ReadOnlySpan<uint> numbers)
    {
        if (numbers.Length == 0 || numbers.Length > 20) return null;

        long result = 0;
        long multiplier = 1;

        for (int i = numbers.Length - 1; i >= 0; i--)
        {
            result += numbers[i] * multiplier;
            multiplier *= 10;
        }
        return result;
    }

    /// <summary>
    /// Converts a ReadOnlySpan<uint> to a long. Returns null if the conversion fails.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static ulong? ToUlong(this string text)
    {
        return text.AsSpan().ToUlong();
    }

    /// <summary>
    /// Converts a char to an unsigned long. Returns null if the conversion fails.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static ulong? ToUlong(this char text)
    {
        return text.ToUlong();
    }

    /// <summary>
    /// Converts a ReadOnlySpan<char> to an unsigned long. Returns null if the conversion fails.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static ulong? ToUlong(this ReadOnlySpan<char> text)
    {
        return (ulong.TryParse(text, out ulong result)) ? result : null;
    }

    /// <summary>
    /// Converts a ReadOnlySpan<uint> to an unsigned long. Returns null if the conversion fails.
    /// </summary>
    /// <param name="numbers"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Return the individual digits of a number.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="numDigits"></param>
    /// <returns></returns>
    public static ReadOnlySpan<uint> GetDigits(this int source, int numDigits = 0)
    {
        return GetDigits(source < 0 ? (uint)-source : (uint)source, numDigits);
    }

    /// <summary>
    /// Return the individual digits of a number.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="numDigits"></param>
    /// <returns></returns>
    public static ReadOnlySpan<uint> GetDigits(this int? source, int numDigits = 0)
    {
        return GetDigits(source ?? 0, numDigits);
    }

    /// <summary>
    /// Return the individual digits of a number.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="numDigits"></param>
    /// <returns></returns>
    public static ReadOnlySpan<uint> GetDigits(this uint source, int numDigits = 0)
    {
        if (source == 0 && numDigits == 0)
            return [0];

        if (numDigits == 0)
        {
            uint temp = source;
            while (temp != 0)
            {
                numDigits++;
                temp /= 10;
            }
        }

        Span<uint> _ints = new uint[numDigits];

        for (int i = _ints.Length - 1; i > 0; i--)
        {
            _ints[i] = (uint)(source % 10);
            source /= 10;
        }

        return _ints;
    }

    /// <summary>
    /// Return the individual digits of a number.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="numDigits"></param>
    /// <returns></returns>
    public static ReadOnlySpan<uint> GetDigits(this uint? source, int numDigits = 0)
    {
        return GetDigits(source ?? 0, numDigits);
    }

    /// <summary>
    /// Return the individual digits of a number.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="numDigits"></param>
    /// <returns></returns>
    public static ReadOnlySpan<uint> GetDigits(this ulong source, int numDigits = 0)
    {
        if (source == 0 && numDigits == 0)
            return [0];

        if (numDigits == 0)
        {
            ulong temp = source;
            while (temp != 0)
            {
                numDigits++;
                temp /= 10;
            }
        }

        Span<uint> _ints = new uint[numDigits];

        for (int i = _ints.Length - 1; i > 0; i--)
        {
            _ints[i] = (uint)(source % 10);
            source /= 10;
        }

        return _ints;
    }

    /// <summary>
    /// Return the individual digits of a number.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="numDigits"></param>
    /// <returns></returns>
    public static ReadOnlySpan<uint> GetDigits(this ulong? source, int numDigits = 0)
    {
        return GetDigits(source ?? 0, numDigits);
    }

    /// <summary>
    /// Return the individual digits of a string.
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static ReadOnlySpan<uint> GetDigits(this ReadOnlySpan<char> source)
    {
        Span<uint> buffer = new uint[source.Length];

        int index = 0;

        foreach (char c in source)
        {
            if (char.IsDigit(c))
            {
                buffer[index++] = (uint)(c - '0');
            }
        }

        return buffer.Slice(0, index);
    }

    /// <summary>
    /// Return the individual digits of a string as a string.
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static string GetOnlyDigits(this ReadOnlySpan<char> source)
    {
        Span<char> buffer = new char[source.Length];

        int index = 0;

        foreach (char c in source)
        {
            if (char.IsDigit(c))
            {
                buffer[index++] = c;
            }
        }

        return buffer.Slice(0, index).ToString();
    }

}
