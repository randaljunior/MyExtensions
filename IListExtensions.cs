using System;
using System.Collections.Generic;
using System.Linq;

namespace MyExtensions
{
    public static partial class IListExtensions
    {
        public static void ReplaceItem<TSource>(this IList<TSource> list, TSource newItem, Func<TSource, bool> selector)
        {
            ArgumentNullException.ThrowIfNull(list);
            ArgumentNullException.ThrowIfNull(selector);

            if (list is List<TSource> lst)
            {
                int index = lst.FindIndex(new Predicate<TSource>(selector));
                if (index != -1)
                {
                    lst[index] = newItem;
                    return;
                }
            }
            else
            {
                int count = list.Count;
                for (int i = 0; i < count; i++)
                {
                    if (selector(list[i]))
                    {
                        list[i] = newItem;
                        return;
                    }
                }
            }
        }
    }
}
