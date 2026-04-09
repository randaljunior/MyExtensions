using System.Text.Json;
using System.Text.Json.Serialization;

namespace MyExtensions;

[Obsolete("User o MyEnumSourceGenerator instead")]
public sealed class EnumSpaceSpaceJsonConverter<T> : JsonConverter<T> where T : struct, Enum
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var _str = reader.GetString()?.Trim().Normalize();

        if (_str.IsNullOrEmpty())
            return default;

        return _str.ToEnum<T>();
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.GetDescription(' '));
    }
}

[Obsolete("User o MyEnumSourceGenerator instead")]
public sealed class EnumCommaJsonConverter<T> : JsonConverter<T> where T : struct, Enum
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var _str = reader.GetString()?.Trim().Normalize();

        if (_str.IsNullOrEmpty())
            return default;

        return _str.ToEnum<T>();
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.GetDescription(','));
    }
}

[Obsolete("User o MyEnumSourceGenerator instead")]
public sealed class EnumArrayJsonConverter<T> : JsonConverter<T> where T : struct, Enum
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("Esperado início de array.");
        }

        List<string> valores = [];

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
                break;

            if (reader.TokenType == JsonTokenType.String)
            {
                valores.Add(reader.GetString()!.Normalize());
                continue;
            }

            if (reader.TokenType == JsonTokenType.Number)
            {
                valores.Add(reader.GetString()!);
                continue;
            }

            if (reader.TokenType == JsonTokenType.Null)
            {
                valores.Add(string.Empty);
                continue;
            }

            throw new JsonException("Esperado valor string dentro do array.");
        }

        // Aqui você pode transformar o array de strings em T
        // Exemplo: se T for um enum com descrição, você pode fazer o inverso de GetDescription
        // Para simplificar, vamos apenas retornar o primeiro valor como exemplo:
        string combinado = string.Join(",", valores);

        return combinado.ToEnum<T>();
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        var span = value.GetDescription(',').Normalize().AsSpan();
        var descricoes = span.Split(',');

        writer.WriteStartArray();

        foreach (var item in descricoes)
        {
            writer.WriteStringValue(span[item].Trim());
        }

        writer.WriteEndArray();
    }
}