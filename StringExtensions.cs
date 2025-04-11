using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace MyExtensions
{
    public static class StringExtensions
    {
        public static string Left(this string value, int length)
        {
            return value?.Substring(0, Math.Min(length, value.Length));
        }

        public static string Right(this string value, int length)
        {
            return value?.Substring(Math.Max(value.Length - 1, value.Length - length - 1));
        }

        public static string RegexReplace(this string value, string regexOldString, string newString)
        {
            var rgx = new Regex(regexOldString);
            return rgx.Replace(value, newString);
        }

        public static string Utf8ToHex(this string value)
        {
            byte[] vs = Encoding.UTF8.GetBytes(value);
            var hexValue = BitConverter.ToString(vs).Replace("-", "");
            return hexValue;
        }

        public static string HexToUtf8(this string value)
        {
            byte[] vs = Encoding.UTF8.GetBytes(value);
            var hexValue = BitConverter.ToString(vs).Replace("-", "");
            return hexValue;
        }

    }
}
