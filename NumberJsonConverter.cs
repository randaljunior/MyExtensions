using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Buffers;

namespace MyExtensions;

public sealed class BigIntegerJsonConverter : JsonConverter<BigInteger>
{
    public override BigInteger Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            reader.TryGetInt64(out var Value);
            return (BigInteger)Value;
        }
        else if (reader.TokenType == JsonTokenType.String)
        {
            var _str = reader.GetString();
            if (String.IsNullOrEmpty(_str))
                return default;

            _ = BigInteger.TryParse(_str, out BigInteger _bigInteger);
            return _bigInteger;
        }

        return default;
    }

    public override void Write(Utf8JsonWriter writer, BigInteger value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}

public sealed class NumberJsonConverter<T> : JsonConverter<T>
    where T : struct, INumber<T>, IMinMaxValue<T>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryParseFromReader(ref Utf8JsonReader reader, out T result)
    {
        // 1. Caminho feliz: o valor está contíguo na memória (99% dos casos)
        if (!reader.HasValueSequence)
        {
            return TryParseSpan(reader.ValueSpan, out result);
        }

        // 2. Caminho fragmentado (raro)
        var sequence = reader.ValueSequence;
        if (sequence.Length > 64)
        {
            result = default;
            return false;
        }

        // O buffer fica isolado neste escopo, o compilador permite!
        Span<byte> buffer = stackalloc byte[64];
        sequence.CopyTo(buffer);

        return TryParseSpan(buffer[..(int)sequence.Length], out result);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryParseSpan(ReadOnlySpan<byte> utf8Span, out T result)
    {
        if (utf8Span.IsEmpty)
        {
            result = default;
            return false;
        }

        // Converte os bytes UTF-8 para caracteres na Stack
        Span<char> chars = stackalloc char[64];
        int charCount = System.Text.Encoding.UTF8.GetChars(utf8Span, chars);

        // Faz o parse final
        return T.TryParse(chars[..charCount], CultureInfo.InvariantCulture, out result);
    }

    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            return T.CreateSaturating(reader.GetDecimal());
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            if (TryParseFromReader(ref reader, out T result))
            {
                return result;
            }
            return default;
        }

        throw new JsonException($"Token inesperado ({reader.TokenType}) ao tentar ler {typeof(T).Name}.");
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        if (typeof(T) == typeof(int))
            writer.WriteNumberValue(Unsafe.As<T, int>(ref value));
        else if (typeof(T) == typeof(long))
            writer.WriteNumberValue(Unsafe.As<T, long>(ref value));
        else if (typeof(T) == typeof(decimal))
            writer.WriteStringValue(Unsafe.As<T, decimal>(ref value).ToString(CultureInfo.InvariantCulture));
        else if (typeof(T) == typeof(double))
            writer.WriteNumberValue(Unsafe.As<T, double>(ref value));
        else if (typeof(T) == typeof(float))
            writer.WriteNumberValue(Unsafe.As<T, float>(ref value));
        else if (typeof(T) == typeof(short))
            writer.WriteNumberValue(Unsafe.As<T, short>(ref value));
        else if (typeof(T) == typeof(byte))
            writer.WriteNumberValue(Unsafe.As<T, byte>(ref value));
        else
            writer.WriteNumberValue(decimal.CreateSaturating(value));
    }

    public override T ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (TryParseFromReader(ref reader, out T result))
        {
            return result;
        }

        throw new JsonException($"Não foi possível converter a chave do dicionário para {typeof(T).Name}.");
    }

    public override void WriteAsPropertyName(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        Span<char> buffer = stackalloc char[64];
        if (value.TryFormat(buffer, out int charsWritten, default, CultureInfo.InvariantCulture))
        {
            writer.WritePropertyName(buffer[..charsWritten]);
        }
        else
        {
            // Fallback seguro caso o TryFormat falhe (muito raro para números)
            writer.WritePropertyName(value.ToString(null, CultureInfo.InvariantCulture));
        }
    }
}

public sealed class NullableNumberJsonConverter<T> : JsonConverter<T?>
    where T : struct, INumber<T>, IMinMaxValue<T>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryParseFromReader(ref Utf8JsonReader reader, out T result)
    {
        // 1. Caminho feliz: o valor está contíguo na memória (99% dos casos)
        if (!reader.HasValueSequence)
        {
            return TryParseSpan(reader.ValueSpan, out result);
        }

        // 2. Caminho fragmentado (raro)
        var sequence = reader.ValueSequence;
        if (sequence.Length > 64)
        {
            result = default;
            return false;
        }

        // O buffer fica isolado neste escopo, o compilador permite!
        Span<byte> buffer = stackalloc byte[64];
        sequence.CopyTo(buffer);

        return TryParseSpan(buffer[..(int)sequence.Length], out result);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryParseSpan(ReadOnlySpan<byte> utf8Span, out T result)
    {
        if (utf8Span.IsEmpty)
        {
            result = default;
            return false;
        }

        // Converte os bytes UTF-8 para caracteres na Stack
        Span<char> chars = stackalloc char[64];
        int charCount = System.Text.Encoding.UTF8.GetChars(utf8Span, chars);

        // Faz o parse final
        return T.TryParse(chars[..charCount], CultureInfo.InvariantCulture, out result);
    }

    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType == JsonTokenType.Number)
        {
            // OTIMIZAÇÃO: Removido o Convert.ChangeType (que causava boxing). 
            // Usando CreateSaturating igual ao conversor não-nulo.
            return T.CreateSaturating(reader.GetDecimal());
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            if (TryParseFromReader(ref reader, out T result))
            {
                return result;
            }
            return null;
        }

        throw new JsonException($"Unexpected token {reader.TokenType} for type {typeof(T?)}.");
    }

    public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
    {
        if (!value.HasValue)
        {
            writer.WriteNullValue();
            return;
        }

        T val = value.Value;

        // OTIMIZAÇÃO: Alinhado com o conversor principal para evitar alocação de string
        // (exceto para decimal, que você definiu explicitamente como string no original)
        if (typeof(T) == typeof(int))
            writer.WriteNumberValue(Unsafe.As<T, int>(ref val));
        else if (typeof(T) == typeof(long))
            writer.WriteNumberValue(Unsafe.As<T, long>(ref val));
        else if (typeof(T) == typeof(decimal))
            writer.WriteStringValue(Unsafe.As<T, decimal>(ref val).ToString(CultureInfo.InvariantCulture));
        else if (typeof(T) == typeof(double))
            writer.WriteNumberValue(Unsafe.As<T, double>(ref val));
        else if (typeof(T) == typeof(float))
            writer.WriteNumberValue(Unsafe.As<T, float>(ref val));
        else if (typeof(T) == typeof(short))
            writer.WriteNumberValue(Unsafe.As<T, short>(ref val));
        else if (typeof(T) == typeof(byte))
            writer.WriteNumberValue(Unsafe.As<T, byte>(ref val));
        else
            writer.WriteNumberValue(decimal.CreateSaturating(val));
    }

    public override T? ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (TryParseFromReader(ref reader, out T result))
        {
            return result;
        }

        throw new JsonException($"Não foi possível converter a chave do dicionário para {typeof(T?).Name}.");
    }

    public override void WriteAsPropertyName(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            Span<char> buffer = stackalloc char[64];
            if (value.Value.TryFormat(buffer, out int charsWritten, default, CultureInfo.InvariantCulture))
            {
                writer.WritePropertyName(buffer[..charsWritten]);
            }
            else
            {
                writer.WritePropertyName(value.Value.ToString(null, CultureInfo.InvariantCulture));
            }
        }
        else
        {
            writer.WritePropertyName(string.Empty);
        }
    }
}

public sealed class NullableBolleanJsonConverter : JsonConverter<bool?>
{
    public override bool? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {


        switch (reader.TokenType)
        {
            case JsonTokenType.True:
                return true;

            case JsonTokenType.False:
                return false;

            case JsonTokenType.String:
                var _str = reader.GetString();
                var _success = bool.TryParse(_str, out bool _bool);
                return (_success) ? _bool : null;

            case JsonTokenType.Number:
                return reader.GetInt32() == 1;

            default:
                return null;
        }
    }

    public override void Write(Utf8JsonWriter writer, bool? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteBooleanValue(value ?? false);
    }
}

public sealed class BolleanJsonConverter : JsonConverter<bool>
{
    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {


        switch (reader.TokenType)
        {
            case JsonTokenType.True:
                return true;

            case JsonTokenType.False:
                return false;

            case JsonTokenType.String:
                var _str = reader.GetString();
                _ = bool.TryParse(_str, out bool _bool);
                return _bool;

            case JsonTokenType.Number:
                return reader.GetInt32() == 1;

            default:
                return false;
        }
    }

    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
    {
        writer.WriteBooleanValue(value);
    }
}
