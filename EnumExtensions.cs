using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;

namespace MyExtensions;


[AttributeUsage(AttributeTargets.Field | AttributeTargets.Enum, Inherited = false, AllowMultiple = false)]
public sealed class DescriptionAttribute : Attribute
{
    public string Description { get; }

    public DescriptionAttribute(string description)
    {
        Description = description;
    }
}

public static partial class EnumExtensions
{
    public static T? ToEnum<T>(this ReadOnlySpan<char> _string, char separator = default) where T : notnull, Enum, new()
    {
        Dictionary<string, T> EnumFields = new();

        var type = typeof(T);
        if (!type.IsEnum) throw new ArgumentException("T deve ser um tipo enum.");

        bool hasFlagsAttribute = type.GetCustomAttributes(typeof(FlagsAttribute), false).Length > 0;

        {
            var _values = Enum.GetValues(typeof(T));

            foreach (var enumValue in _values)
            {
                var field = typeof(T).GetField(enumValue.ToString()!);
                var attribute = field?.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault() as DescriptionAttribute;
                if (attribute is not null)
                {
                    EnumFields.TryAdd(attribute.Description.ToLower(), (T)enumValue);
                }
            }
        }

        MemoryExtensions.SpanSplitEnumerator<char> scopeList;

        if (separator != default)
            scopeList = _string.Split(separator);
        else
            scopeList = _string.SplitAny(',', ' ');

        if (hasFlagsAttribute)
        {
            T scopeEnum = new();

            foreach (var srt in scopeList)
            {
                var slice = _string[srt].Trim();

                if (EnumFields.ContainsKey(slice.ToString().ToLower()))
                {
                    scopeEnum = (T)Enum.ToObject(type, ((ulong)(object)scopeEnum | (ulong)(object)EnumFields[slice.ToString().ToLower()]));
                }
            }

            return scopeEnum;
        }
        else
        {
            EnumFields.TryGetValue(scopeList.Current.ToString(), out var scopeEnum);
            return scopeEnum;
        }
    }


    //public static string GetDescription<T>(this T e) where T : IConvertible
    //{
    //    if (e is Enum)
    //    {
    //        Type type = e.GetType();
    //        Array values = System.Enum.GetValues(type);

    //        foreach (int val in values)
    //        {
    //            if (val == e.ToInt32(CultureInfo.InvariantCulture))
    //            {
    //                var memInfo = type.GetMember(type.GetEnumName(val));

    //                if (memInfo[0]
    //                    .GetCustomAttributes(typeof(DescriptionAttribute), false)
    //                    .FirstOrDefault() is DescriptionAttribute descriptionAttribute)
    //                {
    //                    return descriptionAttribute.Description;
    //                }
    //            }
    //        }
    //    }

    //    return null;
    //}

    public static string GetDescription(this Enum value, char delimiter = ' ')
    {
        var type = value.GetType();
        bool hasFlagsAttribute = type.GetCustomAttributes(typeof(FlagsAttribute), false).Length > 0;

        if (hasFlagsAttribute)
        {
            var values = Enum.GetValues(type);
            var descriptions = new List<string>();

            foreach (Enum enumValue in values)
            {
                if (value.HasFlag(enumValue))
                {
                    var field = type.GetField(enumValue.ToString());

                    var attribute = field?.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault() as DescriptionAttribute;
                    if (attribute != null)
                    {
                        descriptions.Add(attribute.Description);
                    }
                }
            }

            return string.Join(delimiter, descriptions);
        }
        else
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = field?.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault() as DescriptionAttribute;
            return attribute == null ? value.ToString() : attribute.Description;
        }
    }

    public static Dictionary<T, string> EnumDict<T>(this Type value) where T : Enum
    {
        var _dict = new Dictionary<T, string>();
        var _values = Enum.GetValues(value).Cast<T>();

        foreach (var item in _values)
        {
            _dict.Add(item, item.GetDescription());
        }

        return _dict;
    }
}
