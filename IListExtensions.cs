using System;
using System.Collections.Generic;
using System.Linq;

namespace MyExtensions
{
    public static partial class IListExtensions
    {
        public static void ReplaceItem<Tsource>(this IList<Tsource> list, Tsource newItem, Func<Tsource, bool> Selector)
        {
            var _oldItem = list.First(Selector);

            if (_oldItem != null)
            {
                list[list.IndexOf(_oldItem)] = newItem;
            }
        }
    }
}
