using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace MyExtensions;

public static class JsonExtensions
{
    public static bool CheckJson([StringSyntax("Json")] this string json, Type type)
    {
        Dictionary<string, bool> _dic = [];

        var _fd = type.GetProperties();

        foreach (var prop in _fd)
        {
            var _attReq = (JsonRequiredAttribute?)prop.GetCustomAttributes(typeof(JsonRequiredAttribute), false).FirstOrDefault();
            var _attName = (JsonPropertyNameAttribute?)prop.GetCustomAttributes(typeof(JsonPropertyNameAttribute), false).FirstOrDefault();
            var _attIgnore = (JsonIgnoreAttribute?)prop.GetCustomAttributes(typeof(JsonIgnoreAttribute), false).FirstOrDefault();

            if (_attIgnore is null)
                _dic.TryAdd(
                    _attName?.Name ?? prop.Name,
                    (_attReq is not null)
                    );
        }

        JsonNode? jsonObj = JsonNode.Parse(json);

        if (jsonObj is null) return false;

        foreach (var item in _dic)
        {
            if (item.Value == true)
            {
                if (jsonObj[item.Key] is null)
                    return false;
            }
        }

        return true;
    }

    //extension(JsonSerializer)
    //{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryDeserialize<T>(FileStream json, JsonSerializerOptions options, out T? output)
    {
        try
        {
            output = JsonSerializer.Deserialize<T>(json, options);
            return true;
        }
        catch
        {
            output = default;
            return false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryDeserialize<T>([StringSyntax("Json")] string json, JsonSerializerOptions options, out T? output)
    {
        try
        {
            output = JsonSerializer.Deserialize<T>(json, options);

            if (output is null)
            {
                output = default;
                return false;
            }

            return true;
        }
        catch
        {
            output = default;
            return false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryDeserialize<T>([StringSyntax("Json")] string json, JsonTypeInfo<T> jsonTypeInfo, out T? output)
    {
        try
        {
            output = JsonSerializer.Deserialize<T>(json, jsonTypeInfo);

            if (output is null)
            {
                output = default;
                return false;
            }
            return true;
        }
        catch
        {
            output = default;
            return false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<T?> DeserializeFileAsync<T>(string path, JsonSerializerOptions options)
    {
        try
        {
            using FileStream fileStream = new(path, FileMode.Open, FileAccess.Read);
            return await JsonSerializer.DeserializeAsync<T>(fileStream, options).ConfigureAwait(false);
        }
        catch (JsonException)
        {
            var _json = (await File.ReadAllTextAsync(path)).AsSpan();
            return JsonSerializer.Deserialize<T>(_json, options);
        }
        catch
        {
            throw;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<T?> DeserializeFileAsync<T>(string path, JsonTypeInfo<T> jsonTypeInfo)
    {
        try
        {
            using FileStream fileStream = new(path, FileMode.Open, FileAccess.Read);
            return await JsonSerializer.DeserializeAsync<T>(fileStream, jsonTypeInfo).ConfigureAwait(false);
        }
        catch (JsonException)
        {
            var _json = (await File.ReadAllTextAsync(path)).AsSpan();
            return JsonSerializer.Deserialize<T>(_json, jsonTypeInfo);
        }
        catch
        {
            throw;
        }
    }

    //}
}
