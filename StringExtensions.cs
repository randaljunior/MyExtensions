using System;
using System.Text;
using System.Text.RegularExpressions;

namespace MyExtensions
{
    public static class StringExtensions
    {
        public static bool IsNullOrEmpty(this string? value)
        {
            return String.IsNullOrEmpty(value);
        }

        public static bool IsNullOrWhiteSpace(this string? value)
        {
            return String.IsNullOrWhiteSpace(value);
        }

        public static string Repeat(this string value, int size)
        {
            Span<char> _text = stackalloc char[value.Length * size];
            for (int i = 0; i < size; i++)
            {
                value.AsSpan().CopyTo(_text.Slice(i * value.Length, value.Length));
            }

            return _text.ToString();
        }

        public static string Repeat(this char value, int size)
        {
            Span<char> _text = stackalloc char[size];
            for (int i = 0; i < size; i++)
            {
                _text[i] = value;
            }

            return _text.ToString();
        }

        public static ReadOnlySpan<char> Left(this ReadOnlySpan<char> input, int lenght)
        {
            return input.Slice(0, Math.Min(lenght, input.Length));
        }

        public static string Left(this string input, int lenght)
        {
            return input.AsSpan().Slice(0, Math.Min(lenght, input.Length)).ToString();
        }

        public static ReadOnlySpan<char> Right(this ReadOnlySpan<char> input, int lenght)
        {
            return input.Slice(Math.Max(input.Length - 1, input.Length - lenght - 1));
        }

        public static string? Right(this string input, int lenght)
        {
            return input.AsSpan().Slice(Math.Max(input.Length - 1, input.Length - lenght - 1)).ToString();
        }

        public static string RegexReplace(this string value, string regexOldString, string newString)
        {
            var rgx = new Regex(regexOldString);
            return rgx.Replace(value, newString);
        }

        public static string Utf8ToHex(this string value)
        {
            return Utf8ToHex(value.AsSpan());
        }

        public static string Utf8ToHex(this ReadOnlySpan<char> input)
        {
            Span<byte> utf8Bytes = stackalloc byte[Encoding.UTF8.GetByteCount(input)];
            _ = Encoding.UTF8.GetBytes(input, utf8Bytes);

            return Convert.ToHexString(utf8Bytes).Replace("-", "");
        }

        // TODO: Melhorar a conversão de hexadecimal para UTF-8
        public static string HexToUtf8(this ReadOnlySpan<char> hexInput)
        {
            // Valida o comprimento do hexadecimal (deve ser par)
            if (hexInput.Length % 2 != 0)
            {
                throw new ArgumentException("O comprimento do hexadecimal deve ser par.");
            }

            // Aloca espaço para os bytes na pilha
            Span<byte> utf8Bytes = stackalloc byte[hexInput.Length / 2];

            // Converte o hexadecimal em bytes
            for (int i = 0; i < utf8Bytes.Length; i++)
            {
                utf8Bytes[i] = byte.Parse(hexInput.Slice(i * 2, 2));
            }

            // Converte os bytes UTF-8 em uma string
            return Encoding.UTF8.GetString(utf8Bytes);
        }

    }
}
