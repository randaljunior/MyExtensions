using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;

namespace MyExtensions
{
    public static partial class EnumExtensions
    {
        public static string GetDescription<T>(this T e) where T : IConvertible
        {
            if (e is Enum)
            {
                Type type = e.GetType();
                Array values = System.Enum.GetValues(type);

                foreach (int val in values)
                {
                    if (val == e.ToInt32(CultureInfo.InvariantCulture))
                    {
                        var memInfo = type.GetMember(type.GetEnumName(val));

                        if (memInfo[0]
                            .GetCustomAttributes(typeof(DescriptionAttribute), false)
                            .FirstOrDefault() is DescriptionAttribute descriptionAttribute)
                        {
                            return descriptionAttribute.Description;
                        }
                    }
                }
            }

            return null;
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
}
