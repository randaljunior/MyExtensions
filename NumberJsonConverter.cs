using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

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
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // 1. Caso o token seja um número (ex: 123.45)
        if (reader.TokenType == JsonTokenType.Number)
        {
            // O GetDecimal() ainda é a forma mais precisa de ler o número bruto do JSON
            // CreateSaturating faz o "clamp" (limita aos extremos do tipo T) sem lançar erro.
            return T.CreateSaturating(reader.GetDecimal());
        }

        // 2. Caso o token seja uma string (ex: "999999999999")
        if (reader.TokenType == JsonTokenType.String)
        {
            string? value = reader.GetString();
            if (string.IsNullOrWhiteSpace(value))
                return default;

            // Tentamos converter para decimal primeiro para ter a maior amplitude possível
            // Se o texto for um número maior que o decimal suporta, o decimal.TryParse falhará.
            if (decimal.TryParse(value, CultureInfo.InvariantCulture, out decimal decimalValue))
            {
                return T.CreateSaturating(decimalValue);
            }

            // Fallback para números absurdamente gigantes (estilo BigInteger no JSON)
            // T.Parse geralmente lança erro se estourar, então o TryParse + Saturating é o ideal.
            if (T.TryParse(value, CultureInfo.InvariantCulture, out T result))
            {
                return result;
            }
        }

        throw new JsonException($"Token inesperado ({reader.TokenType}) ao tentar ler {typeof(T).Name}.");
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        // Otimização de JIT: no .NET 10, esse typeof(T) é resolvido em tempo de compilação
        // transformando isso em uma chamada direta sem custo de branching.
        if (typeof(T) == typeof(int))
            writer.WriteNumberValue(Unsafe.As<T, int>(ref value));
        else if (typeof(T) == typeof(long))
            writer.WriteNumberValue(Unsafe.As<T, long>(ref value));
        else if (typeof(T) == typeof(decimal))
            writer.WriteNumberValue(Unsafe.As<T, decimal>(ref value));
        else if (typeof(T) == typeof(double))
            writer.WriteNumberValue(Unsafe.As<T, double>(ref value));
        else if (typeof(T) == typeof(float))
            writer.WriteNumberValue(Unsafe.As<T, float>(ref value));
        else if (typeof(T) == typeof(short))
            writer.WriteNumberValue(Unsafe.As<T, short>(ref value));
        else if (typeof(T) == typeof(byte))
            writer.WriteNumberValue(Unsafe.As<T, byte>(ref value));
        else
        {
            // Para qualquer outro tipo INumber (como BigInteger ou Half)
            writer.WriteNumberValue(decimal.CreateSaturating(value));
        }
    }
}

public sealed class NullableNumberJsonConverter<T> : JsonConverter<T?>
    where T : struct, IConvertible, IParsable<T>, ISpanParsable<T>, INumber<T>, IFormattable, IComparable<T>, IEquatable<T>
{
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType == JsonTokenType.Number)
        {
            return (T)Convert.ChangeType(reader.GetDecimal(), typeof(T), CultureInfo.InvariantCulture);
        }
        else if (reader.TokenType == JsonTokenType.String)
        {
            var str = reader.GetString();
            if (string.IsNullOrWhiteSpace(str))
                return null;

            _ = T.TryParse(str, CultureInfo.InvariantCulture, out T value);
            return value;
        }

        throw new JsonException($"Unexpected token {reader.TokenType} for type {typeof(T?)}.");
    }

    public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
        }
        else
        {
            writer.WriteStringValue(value.Value.ToString(null, CultureInfo.InvariantCulture));
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
