using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.Unicode;

namespace MyExtensions
{
    public static class StringExtensions
    {

        /// <summary>
        /// Checks if a string is null or empty.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this string? value)
        {
            return String.IsNullOrEmpty(value);
        }

        /// <summary>
        /// Checks if a string is null or empty or whitespace.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsNullOrWhiteSpace(this string? value)
        {
            return String.IsNullOrWhiteSpace(value);
        }

        /// <summary>
        /// Repeat a string n times.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="ntimes"></param>
        /// <returns></returns>
        public static string Repeat(this string value, int ntimes)
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
        public static string Repeat(this char value, int ntimes)
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
        public static ReadOnlySpan<char> Right(this ReadOnlySpan<char> input, int lenght)
        {
            return input.Slice(Math.Max(input.Length - 1, input.Length - lenght - 1));
        }

        /// <summary>
        /// Returns the right part of a string.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="lenght"></param>
        /// <returns></returns>
        public static string? Right(this string input, int lenght)
        {
            return input.AsSpan().Slice(Math.Max(input.Length - 1, input.Length - lenght - 1)).ToString();
        }

        /// <summary>
        /// Replace a string using regex.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="regexOldString"></param>
        /// <param name="newString"></param>
        /// <returns></returns>
        public static string RegexReplace(this string value, string regexOldString, string newString)
        {
            var rgx = new Regex(regexOldString);
            return rgx.Replace(value, newString);
        }

        /// <summary>
        /// Converts a string to hexadecimal using UTF-8 encoding.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Converts a string to a ReadOnlySpan of bytes.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static byte[] ToBytesArray(this ReadOnlySpan<char> text, int size = 0)
        { 
            int _length = (size == 0) ? text.Length : Math.Min(size, text.Length);


            if (_length > 1024)
            {
                byte[] _bytes = new byte[_length];
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
                return _bytes; 
            }
            else
            {
                Span<byte> _bytes = stackalloc byte[_length];
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

                return _bytes.ToArray();
            }
        }

    }
}
