using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MyExtensions;

public class StringNormalizeConverter : JsonConverter<string>
{
    public override bool HandleNull => true;

    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return Normalize(reader.GetString());
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        if (value == null)
            writer.WriteNullValue();
        else
            writer.WriteStringValue(Normalize(value));
    }

    public override string ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Chaves de dicionário no JSON nunca são nulas por especificação
        return Normalize(reader.GetString()) ?? string.Empty;
    }

    public override void WriteAsPropertyName(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        // Escreve a chave do dicionário normalizada
        writer.WritePropertyName(Normalize(value) ?? string.Empty);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string? Normalize(string? value)
    {
        if (value == null)
            return null;
        return value.IsNormalized(NormalizationForm.FormC) ? value : value.Normalize(NormalizationForm.FormC);
    }
}

public class NormalizeNamingPolicy : JsonNamingPolicy
{
    public override string ConvertName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;

        // Se o nome da propriedade já estiver em FormC, não aloca nada novo
        return name.IsNormalized(NormalizationForm.FormC) ? name : name.Normalize(NormalizationForm.FormC);
    }
}
