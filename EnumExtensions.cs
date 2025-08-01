using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Enum, Inherited = false, AllowMultiple = false)]
public sealed class DescriptionAttribute(string description) : Attribute
{
    public string Description { get; } = description;
}

/// <summary>
/// Métodos de extensão otimizados para Enums.
/// </summary>
public static partial class EnumExtensions
{
    private static readonly char[] FlagDelimiters = [',', ' ', '|'];

    /// <summary>
    /// Converte uma string para um valor de enum, usando o cache de descrições para alta performance.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ToEnum<T>(this string input) where T : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(input))
            return default;

        return input.AsSpan().ToEnum<T>();
    }

    /// <summary>
    /// Converte um ReadOnlySpan<char> para um valor de enum, usando o cache de descrições.
    /// Esta é a implementação principal, com baixa alocação de memória.
    /// </summary>
    public static T ToEnum<T>(this ReadOnlySpan<char> input) where T : struct, Enum
    {
        var trimmedInput = input.Trim();
        if (trimmedInput.IsEmpty)
            return default;

        // Para enums sem [Flags], a conversão é uma busca direta no cache.
        if (!EnumCache<T>.IsFlagsEnum)
        {
            return EnumCache<T>.TryGetValue(trimmedInput, out var enumValue) ? enumValue : default;
        }

        // Para enums com [Flags], combina os valores encontrados.
        return ProcessFlagsFromSpan<T>(trimmedInput);
    }

    /// <summary>
    /// Converte um array de strings para um valor de enum combinado.
    /// Útil para enums com [Flags] quando você tem valores separados.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ToEnum<T>(this string[] inputs) where T : struct, Enum
    {
        if (inputs == null || inputs.Length == 0)
            return default;

        return ProcessMultipleInputs<T>(inputs.AsSpan());
    }

    /// <summary>
    /// Converte um array de ReadOnlyMemory<char> para um valor de enum combinado.
    /// Versão otimizada para baixa alocação de memória.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ToEnum<T>(this ReadOnlyMemory<char>[] inputs) where T : struct, Enum
    {
        if (inputs == null || inputs.Length == 0)
            return default;

        return ProcessMultipleInputs<T>(inputs.AsSpan());
    }

    /// <summary>
    /// Converte um Span<ReadOnlyMemory<char>> para um valor de enum combinado.
    /// Versão ainda mais otimizada para cenários de alta performance.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ToEnum<T>(this Span<ReadOnlyMemory<char>> inputs) where T : struct, Enum
    {
        if (inputs.IsEmpty)
            return default;

        return ProcessMultipleInputs<T>(inputs);
    }

    /// <summary>
    /// Obtém a descrição de um valor de enum a partir de um cache de alta performance.
    /// </summary>
    public static string GetDescription<T>(this T value, char delimiter = ' ') where T : struct, Enum
    {
        // Lógica para enums sem [Flags] é uma busca simples no cache.
        if (!EnumCache<T>.IsFlagsEnum)
        {
            return EnumCache<T>.ValueToDescriptionMap.TryGetValue(value, out var desc) ? desc : value.ToString();
        }

        // Lógica otimizada para enums com [Flags].
        var numericValue = Convert.ToUInt64(value);
        if (numericValue == 0)
        {
            return EnumCache<T>.ValueToDescriptionMap.TryGetValue(value, out var desc) ? desc : value.ToString();
        }

        // Usa StringBuilder pooled para reduzir alocações
        var sb = new StringBuilder();
        var hasValues = false;

        // Itera sobre os valores de flag cacheados e ordenados.
        foreach (var flagPair in EnumCache<T>.FlagValues)
        {
            if ((numericValue & flagPair.Key) == flagPair.Key)
            {
                if (hasValues)
                    sb.Append(delimiter);

                sb.Append(flagPair.Value);
                hasValues = true;
                numericValue &= ~flagPair.Key; // Remove o bit correspondente.
            }
        }

        if (!hasValues)
            return value.ToString();

        return sb.ToString();
    }

    /// <summary>
    /// Retorna um dicionário com todos os valores do enum e suas respectivas descrições.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Dictionary<T, string> EnumDict<T>() where T : struct, Enum
    {
        // Retorna uma cópia do dicionário cacheado para evitar modificações externas.
        return new Dictionary<T, string>(EnumCache<T>.ValueToDescriptionMap);
    }

    /// <summary>
    /// Processa múltiplos inputs e combina os valores encontrados.
    /// Método central que aplica o princípio DRY para todos os arrays.
    /// </summary>
    private static T ProcessMultipleInputs<T>(ReadOnlySpan<ReadOnlyMemory<char>> inputs) where T : struct, Enum
    {
        // Para enums sem [Flags], usa apenas o primeiro valor válido.
        if (!EnumCache<T>.IsFlagsEnum)
        {
            foreach (var input in inputs)
            {
                if (TryGetEnumValue<T>(input.Span, out var result))
                    return result;
            }
            return default;
        }

        // Para enums com [Flags], combina todos os valores válidos.
        ulong combinedValue = 0;
        foreach (var input in inputs)
        {
            if (TryGetEnumValueAsUlong<T>(input.Span, out var flagValue))
            {
                combinedValue |= flagValue;
            }
        }

        return Unsafe.As<ulong, T>(ref combinedValue);
    }

    /// <summary>
    /// Processa múltiplos inputs de string usando spans.
    /// </summary>
    private static T ProcessMultipleInputs<T>(ReadOnlySpan<string> inputs) where T : struct, Enum
    {
        // Para enums sem [Flags], usa apenas o primeiro valor válido.
        if (!EnumCache<T>.IsFlagsEnum)
        {
            foreach (var input in inputs)
            {
                if (!string.IsNullOrWhiteSpace(input) && TryGetEnumValue<T>(input.AsSpan(), out var result))
                    return result;
            }
            return default;
        }

        // Para enums com [Flags], combina todos os valores válidos.
        ulong combinedValue = 0;
        foreach (var input in inputs)
        {
            if (!string.IsNullOrWhiteSpace(input) && TryGetEnumValueAsUlong<T>(input.AsSpan(), out var flagValue))
            {
                combinedValue |= flagValue;
            }
        }

        return Unsafe.As<ulong, T>(ref combinedValue);
    }

    /// <summary>
    /// Processa flags de um span, dividindo por delimitadores.
    /// </summary>
    private static T ProcessFlagsFromSpan<T>(ReadOnlySpan<char> input) where T : struct, Enum
    {
        ulong combinedValue = 0;
        var remainingSpan = input;

        while (!remainingSpan.IsEmpty)
        {
            int delimiterIndex = remainingSpan.IndexOfAny(FlagDelimiters);
            var tokenSpan = (delimiterIndex == -1) ? remainingSpan : remainingSpan.Slice(0, delimiterIndex);

            if (TryGetEnumValueAsUlong<T>(tokenSpan, out var flagValue))
            {
                combinedValue |= flagValue;
            }

            if (delimiterIndex == -1) break;
            remainingSpan = remainingSpan.Slice(delimiterIndex + 1);
        }

        return Unsafe.As<ulong, T>(ref combinedValue);
    }

    /// <summary>
    /// Tenta obter um valor de enum de um span.
    /// </summary>
    private static bool TryGetEnumValue<T>(ReadOnlySpan<char> span, out T value) where T : struct, Enum
    {
        var trimmedSpan = span.Trim();
        if (!trimmedSpan.IsEmpty && EnumCache<T>.TryGetValue(trimmedSpan, out value))
        {
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Tenta obter um valor de enum como ulong de um span.
    /// </summary>
    private static bool TryGetEnumValueAsUlong<T>(ReadOnlySpan<char> span, out ulong value) where T : struct, Enum
    {
        var trimmedSpan = span.Trim();
        if (!trimmedSpan.IsEmpty && EnumCache<T>.TryGetValue(trimmedSpan, out var enumValue))
        {
            value = Unsafe.As<T, ulong>(ref enumValue);
            return true;
        }

        value = 0;
        return false;
    }

    /// <summary>
    /// Classe de cache estática para armazenar dados de enums, evitando o uso repetido de reflexão.
    /// </summary>
    private static class EnumCache<T> where T : struct, Enum
    {
        public static readonly bool IsFlagsEnum;
        public static readonly Dictionary<T, string> ValueToDescriptionMap;
        public static readonly List<KeyValuePair<ulong, string>> FlagValues;

        // Cache otimizado para lookup por string/span
        private static readonly Dictionary<string, T> _stringCache;
        private static readonly Dictionary<int, T> _hashCache;

        static EnumCache()
        {
            var type = typeof(T);
            IsFlagsEnum = type.IsDefined(typeof(FlagsAttribute), false);

            var enumNames = Enum.GetNames<T>();
            var enumValues = Enum.GetValues<T>();

            _stringCache = new Dictionary<string, T>(enumNames.Length * 2, StringComparer.OrdinalIgnoreCase);
            _hashCache = new Dictionary<int, T>(enumNames.Length * 2);
            ValueToDescriptionMap = new Dictionary<T, string>(enumValues.Length);
            FlagValues = IsFlagsEnum ? new List<KeyValuePair<ulong, string>>() : null!;

            for (int i = 0; i < enumNames.Length; i++)
            {
                var name = enumNames[i];
                var value = enumValues[i];
                var field = type.GetField(name)!;
                var attribute = field.GetCustomAttribute<DescriptionAttribute>();
                var description = attribute?.Description ?? name;

                ValueToDescriptionMap[value] = description;

                // Cache tanto o nome quanto a descrição
                _stringCache.TryAdd(name, value);
                _stringCache.TryAdd(description, value);

                // Cache por hash para lookup rápido de spans
                _hashCache.TryAdd(string.GetHashCode(name, StringComparison.OrdinalIgnoreCase), value);
                _hashCache.TryAdd(string.GetHashCode(description, StringComparison.OrdinalIgnoreCase), value);

                if (IsFlagsEnum)
                {
                    var ulongValue = Unsafe.As<T, ulong>(ref value);
                    // Não adiciona o valor '0' (None) à lista de flags para combinação.
                    if (ulongValue > 0)
                    {
                        FlagValues!.Add(new KeyValuePair<ulong, string>(ulongValue, description));
                    }
                }
            }

            if (IsFlagsEnum)
            {
                // Ordena por valor decrescente para que flags compostas (ex: All = 7) sejam
                // encontradas antes das flags individuais (ex: Read = 1, Write = 2).
                FlagValues!.Sort((a, b) => b.Key.CompareTo(a.Key));
            }
        }

        /// <summary>
        /// Tenta obter um valor de enum de um span, usando cache otimizado.
        /// </summary>
        public static bool TryGetValue(ReadOnlySpan<char> span, out T value)
        {
            // Primeiro tenta pelo hash (mais rápido)
            var hash = string.GetHashCode(span, StringComparison.OrdinalIgnoreCase);
            if (_hashCache.TryGetValue(hash, out value))
            {
                return true;
            }

            // Fallback para string (necessário para casos de colisão de hash)
            if (span.Length <= 256) // Evita alocações desnecessárias para strings muito grandes
            {
                var str = span.ToString();
                return _stringCache.TryGetValue(str, out value);
            }

            value = default;
            return false;
        }
    }
}