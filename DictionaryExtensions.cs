namespace MyExtensions;

public static class DictionaryExtensions
{
    public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
    {
        if (dictionary.ContainsKey(key))
        {
            dictionary[key] = value;
        }
        else
        {
            dictionary.Add(key, value);
        }
    }

    public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> valueFactory)
    {
        if (dictionary.TryGetValue(key, out var value))
        {
            return value;
        }
        value = valueFactory(key);
        dictionary.Add(key, value);
        return value;
    }

    public static bool TryGetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, out TValue value)
    {
        if (dictionary.TryGetValue(key, out value))
        {
            return true;
        }
        value = default!;
        return false;
    }

    public static void RemoveIfExists<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
    {
        if (dictionary.ContainsKey(key))
        {
            dictionary.Remove(key);
        }
    }

    public static async Task ClearAndDisposeAsync<TKey, TValue>(this IDictionary<TKey, TValue> dictionary) where TKey : notnull
    {
        await Task.Run(() => ClearAndDispose(dictionary));
    }


    public static void ClearAndDispose<TKey, TValue>(this IDictionary<TKey, TValue> dictionary) where TKey : notnull
    {
        if (dictionary is Dictionary<TKey, TValue> concreteDict)
        {
            Dictionary<TKey, TValue>.Enumerator enumerator = concreteDict.GetEnumerator();
            while (enumerator.MoveNext())
            {
                TValue value = enumerator.Current.Value;
                if (value is IDisposable disposableValue)
                {
                    disposableValue.Dispose();
                }
            }
        }
        else
        {
            foreach (KeyValuePair<TKey, TValue> kvp in dictionary)
            {
                if (kvp.Value is IDisposable disposableValue)
                {
                    disposableValue.Dispose();
                }
            }
        }

        (dictionary as IDisposable)?.Dispose();

        dictionary.Clear();
    }
}
