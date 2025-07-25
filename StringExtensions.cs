using System.Buffers;
using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace MyExtensions;

public static class StringExtensions
{

    /// <summary>
    /// Checks if a string is null or empty.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNullOrEmpty([NotNullWhen(false)] this string? value)
    {
        return String.IsNullOrEmpty(value);
    }

    /// <summary>
    /// Checks if a string is null or empty or whitespace.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>   
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string? value)
    {
        return String.IsNullOrWhiteSpace(value);
    }

    /// <summary>
    /// Returns a default value if the string is null.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: NotNull]
    public static string IfNull(this string? value, [DisallowNull] string defaultValue = "")
    {
        return value ?? defaultValue;
    }

    /// <summary>
    /// Repeat a string n times.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="ntimes"></param>
    /// <returns></returns>
    [return: NotNull]
    public static string Repeat([DisallowNull] this string value, int ntimes)
    {
        if (string.IsNullOrEmpty(value) || ntimes <= 0)
            return string.Empty;

        //Span<char> _text = stackalloc char[value.Length * ntimes];
        //for (int i = 0; i < ntimes; i++)
        //{
        //    value.AsSpan().CopyTo(_text.Slice(i * value.Length, value.Length));
        //}

        //return _text.ToString();

        return string.Create(value.Length * ntimes, (value, ntimes), (span, state) =>
        {
            var (_value, _repetitions) = state;
            for (int i = 0; i < _repetitions; i++)
            {
                _value.AsSpan().CopyTo(span.Slice(i * _value.Length));
            }
        });
    }

    /// <summary>
    /// Repeat a char n times.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="ntimes"></param>
    /// <returns></returns>
    [return: NotNull]
    public static string Repeat([DisallowNull] this char value, int ntimes)
    {
        if (ntimes <= 0 || value == default)
            return string.Empty;

        //Span<char> _text = stackalloc char[size];
        //for (int i = 0; i < size; i++)
        //{
        //    _text[i] = value;
        //}

        //return _text.ToString();

        return string.Create(ntimes, (value, ntimes), (span, state) =>
        {
            var (_value, _repetitions) = state;
            for (int i = 0; i < _repetitions; i++)
            {
                span[i] = _value;
            }
        });

    }

    /// <summary>
    /// Returns the left part of a string.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="lenght"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<char> Left(this ReadOnlySpan<char> input, int lenght)
    {
        return input.Slice(0, Math.Min(lenght, input.Length));
    }

    /// <summary>
    /// Returns the left part of a string. 
    /// </summary>
    /// <param name="input"></param>
    /// <param name="lenght"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Left(this string input, int lenght)
    {
        return input.AsSpan().Slice(0, Math.Min(lenght, input.Length)).ToString();
    }

    /// <summary>
    /// Returns the right part of a string.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="lenght"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<char> Right(this ReadOnlySpan<char> input, int lenght)
    {
        return input.Slice(Math.Max(input.Length - lenght, 0));
    }

    /// <summary>
    /// Returns the right part of a string.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="lenght"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: NotNull]
    public static string Right([DisallowNull] this string input, int lenght)
    {
        return input.AsSpan().Right(lenght).ToString();
    }

    /// <summary>
    /// Replace a string using regex.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="regexOldString"></param>
    /// <param name="newString"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string RegexReplace(
        [DisallowNull] this string value,
        [StringSyntax("Regex")] string regexOldString,
        [DisallowNull] string newString)
    {
        var rgx = new Regex(regexOldString);
        return rgx.Replace(value, newString);
    }

    /// <summary>
    /// Replace tokens in a string using a dictionary.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="tokenBegin"></param>
    /// <param name="tokenEnd"></param>
    /// <param name="replacementTokens"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static string ReplaceTokens(
        this ReadOnlySpan<char> text,
        [DisallowNull] char tokenBegin,
        [DisallowNull] char tokenEnd,
        FrozenDictionary<string, string> replacementTokens,
        bool urlEncode = false)
    {
        if (text.IsEmpty || text.IsWhiteSpace())
            return text.ToString();

        if (replacementTokens == null || replacementTokens.Count == 0)
            return text.ToString();

        if (!text.Contains(tokenBegin) || !text.Contains(tokenEnd))
            return text.ToString();

        var path = text.Slice(0);

        var sb = new StringBuilder(path.Length);

        while (true)
        {
            int indexBegin = path.IndexOf('{');

            if (indexBegin < 0)
            {
                sb.Append(path);
                break;
            }

            sb.Append(path.Slice(0, indexBegin));

            int indexEnd = path.IndexOf('}');
            if (indexEnd < 0)
                throw new ArgumentException("Uri mal formada. Não encontrado tokenEnd no fim da substituição.");

            var tokenName = path.Slice(indexBegin + 1, indexEnd - 1 - indexBegin);

            if (replacementTokens.TryGetValue(tokenName.ToString(), out var token))
            {
                sb.Append((urlEncode) ? HttpUtility.UrlEncode(token) : token);
            }
            else
            {
                sb.Append(path.Slice(indexBegin, indexEnd - indexBegin));
            }

            path = path.Slice(indexEnd + 1);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Replace tokens in a string using a dictionary.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="tokenBegin"></param>
    /// <param name="tokenEnd"></param>
    /// <param name="replacementTokens"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ReplaceTokens(
        [DisallowNull] this string text,
        [DisallowNull] char tokenBegin,
        [DisallowNull] char tokenEnd,
        FrozenDictionary<string, string> replacementTokens,
        bool urlEncode = false)
    {
        return text.AsSpan().ReplaceTokens(tokenBegin, tokenEnd, replacementTokens, urlEncode);
    }

    /// <summary>
    /// Converts a string to hexadecimal using UTF-8 encoding.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Utf8ToHex(this string value)
    {
        return Utf8ToHex(value.AsSpan());
    }

    /// <summary>
    /// Converts a string to hexadecimal using UTF-8 encoding.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string Utf8ToHex(this ReadOnlySpan<char> input)
    {
        checked
        {
            if (input.IsEmpty)
                return string.Empty;

            var _size = Encoding.UTF8.GetByteCount(input);

            Span<byte> utf8Bytes =
                (_size < 1024)
                ? stackalloc byte[_size]
                : new byte[_size];

            _ = Encoding.UTF8.GetBytes(input, utf8Bytes);

            return Convert.ToHexString(utf8Bytes);
        }
    }

    // TODO: Melhorar a conversão de hexadecimal para UTF-8
    /// <summary>
    /// Converts a hexadecial to a string using UTF-8 encoding.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string HexToUtf8(this ReadOnlySpan<char> hexInput)
    {
        checked
        {
            // Valida o comprimento do hexadecimal (deve ser par)
            if (hexInput.Length % 2 != 0)
            {
                throw new ArgumentException("O comprimento do hexadecimal deve ser par.");
            }

            // Aloca espaço para os bytes na pilha
            Span<byte> utf8Bytes =
                (hexInput.Length / 2 < 1024)
                ? stackalloc byte[hexInput.Length / 2]
                : new byte[hexInput.Length / 2];

            // Converte o hexadecimal em bytes
            for (int i = 0; i < utf8Bytes.Length; i++)
            {
                utf8Bytes[i] = byte.Parse(hexInput.Slice(i * 2, 2));
            }

            // Converte os bytes UTF-8 em uma string
            return Encoding.UTF8.GetString(utf8Bytes);
        }
    }

    /// <summary>
    /// Converts a string to a ReadOnlySpan of bytes.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    public static byte[] ToBytesArray(this ReadOnlySpan<char> text, int size = 0)
    {
        checked
        {
            int _length = (size == 0) ? text.Length : Math.Min(size, text.Length);


            if (_length > 1024)
            {
                byte[] _bytes = new byte[_length];
                checked
                {
                    for (int i = 0; i < _length; i++)
                    {
                        char c = text[i];

                        if (c >= '0' && c <= '9')
                        {
                            _bytes[i] = (byte)(c - '0');
                        }
                        else
                        {
                            _ = byte.TryParse(text.Slice(i, 1), out _bytes[i]);
                        }
                    }
                }
                return _bytes;
            }
            else
            {
                Span<byte> _bytes = stackalloc byte[_length];
                checked
                {
                    for (int i = 0; i < _length; i++)
                    {
                        char c = text[i];
                        if (c >= '0' && c <= '9')
                        {
                            _bytes[i] = (byte)(c - '0');
                        }
                        else
                        {
                            _ = byte.TryParse(text.Slice(i, 1), out _bytes[i]);
                        }
                    }
                }

                return _bytes.ToArray();
            }
        }
    }

    /// <summary>
    /// Converts a string to a ReadOnlySequence of characters.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySequence<char> AsReadOnlySequence(this string input)
    {
        if (input == null) return ReadOnlySequence<char>.Empty;

        // Zero allocations - usa diretamente a memória da string
        return new ReadOnlySequence<char>(input.AsMemory());
    }

    /// <summary>
    /// Converts a ReadOnlyMemory of characters to a ReadOnlySequence of characters.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySequence<char> AsReadOnlySequence(this ReadOnlyMemory<char> input)
    {
        // Zero allocations - usa diretamente o ReadOnlyMemory
        return new ReadOnlySequence<char>(input);
    }

}
