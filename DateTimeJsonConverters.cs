using System.Buffers;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MyExtensions;

public sealed class DateOnlyJsonConverter : JsonConverter<DateOnly>
{
    private const string _format = "yyyy-MM-dd";
    private const string _fullFormat = "yyyy-MM-dd HH:mm:ssZ";
    private const int _maxDateLength = 32;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static DateOnly Parse(ref Utf8JsonReader reader)
    {
        // Obtém o span de bytes diretamente do buffer (UTF-8)
        ReadOnlySpan<byte> utf8Span = reader.HasValueSequence
            ? reader.ValueSequence.ToArray()
            : reader.ValueSpan;

        if (utf8Span.IsEmpty)
            return default;

        // Transcreve para char na pilha (Stack) - Zero Heap Allocation
        Span<char> chars = stackalloc char[_maxDateLength];
        int charCount = System.Text.Encoding.UTF8.GetChars(utf8Span, chars);
        ReadOnlySpan<char> source = chars[..charCount];

        // 1. Tenta formato yyyy-MM-dd
        if (DateOnly.TryParseExact(source, _format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
            return result;

        // 2. Fallback para yyyy-MM-dd HH:mm:ssZ
        if (DateTime.TryParseExact(source, _fullFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime))
            return DateOnly.FromDateTime(dateTime);

        return default;
    }

    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return Parse(ref reader);
    }

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
    {
        // Formata diretamente no buffer da stack
        Span<char> buffer = stackalloc char[10];
        if (value.TryFormat(buffer, out int charsWritten, _format, CultureInfo.InvariantCulture))
        {
            writer.WriteStringValue(buffer[..charsWritten]);
        }
        else
        {
            writer.WriteStringValue(value.ToString(_format));
        }
    }

    public override DateOnly ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return Parse(ref reader);
    }

    public override void WriteAsPropertyName(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
    {
        Span<char> buffer = stackalloc char[10];
        if (value.TryFormat(buffer, out int charsWritten, _format, CultureInfo.InvariantCulture))
        {
            writer.WritePropertyName(buffer[..charsWritten]);
        }
        else
        {
            writer.WritePropertyName(value.ToString(_format));
        }
    }
}

public sealed class DateOnlyNullableJsonConverter : JsonConverter<DateOnly?>
{
    private const string _format = "yyyy-MM-dd";
    private const string _fullFormat = "yyyy-MM-dd HH:mm:ssZ";
    private const int _maxDateLength = 32; // Suficiente para os formatos esperados

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static DateOnly? Parse(ref Utf8JsonReader reader)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        // Obtém o span de bytes (UTF-8) diretamente do buffer da reader
        ReadOnlySpan<byte> utf8Span = reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan;

        if (utf8Span.IsEmpty)
            return null;

        // Transcreve para char span usando stackalloc (alocação na pilha, zero GC)
        Span<char> chars = stackalloc char[_maxDateLength];
        int charCount = System.Text.Encoding.UTF8.GetChars(utf8Span, chars);
        ReadOnlySpan<char> source = chars[..charCount];

        // 1. Tentativa com formato curto (yyyy-MM-dd)
        if (DateOnly.TryParseExact(source, _format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
            return result;

        // 2. Fallback para formato longo (yyyy-MM-dd HH:mm:ssZ)
        if (DateTime.TryParseExact(source, _fullFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime))
            return DateOnly.FromDateTime(dateTime);

        return null;
    }

    public override DateOnly? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return Parse(ref reader);
    }

    public override void Write(Utf8JsonWriter writer, DateOnly? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            // Para escrita, usamos um buffer pequeno na pilha para formatar a data
            Span<char> buffer = stackalloc char[10]; // "yyyy-MM-dd" sempre tem 10 caracteres
            if (value.Value.TryFormat(buffer, out int charsWritten, _format, CultureInfo.InvariantCulture))
            {
                writer.WriteStringValue(buffer[..charsWritten]);
            }
            else
            {
                writer.WriteStringValue(value.Value.ToString(_format)); // Fallback seguro
            }
        }
        else
        {
            writer.WriteNullValue();
        }
    }

    public override DateOnly? ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return Parse(ref reader);
    }

    public override void WriteAsPropertyName(Utf8JsonWriter writer, DateOnly? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            Span<char> buffer = stackalloc char[10];
            if (value.Value.TryFormat(buffer, out int charsWritten, _format, CultureInfo.InvariantCulture))
            {
                writer.WritePropertyName(buffer[..charsWritten]);
            }
            else
            {
                writer.WritePropertyName(value.Value.ToString(_format));
            }
        }
        else
        {
            writer.WritePropertyName(string.Empty);
        }
    }
}
