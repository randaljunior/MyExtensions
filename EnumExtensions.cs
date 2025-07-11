using System.Reflection;
using System.Runtime.InteropServices;

namespace MyExtensions;

/// <summary>
/// Attribute to provide a description for enum values.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Enum, Inherited = false, AllowMultiple = false)]
public sealed class DescriptionAttribute : Attribute
{
    public string Description { get; }

    public DescriptionAttribute(string description)
    {
        Description = description;
    }
}

public static partial class EnumExtensions
{
    /// <summary>
    /// Converts a string to an enum value, using the description attribute if available.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="input"></param>
    /// <param name="separator"></param>
    /// <returns></returns>
    public static T ToEnum<T>(this string input, char separator = default)
    where T : struct, Enum
    {
        if (input.IsNullOrWhiteSpace())
            return default;

        return input.AsSpan().ToEnum<T>(separator);
    }

    /// <summary>
    /// Converts a string to an enum value, using the description attribute if available.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="input"></param>
    /// <param name="separator"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static T ToEnum<T>(this ReadOnlySpan<char> input, char separator = default)
    where T : struct, Enum
    {
        var _trimmedInput = input.Trim();

        if (input.IsEmpty || input.IsWhiteSpace())
            return default;

        // Se um separador for especificado, utiliza-o; caso contrário, divide a entrada por vírgula e espaço.
        var _tokens = separator != default ? _trimmedInput.Split(separator) : _trimmedInput.SplitAny(',', ' ', '|');

        List<ReadOnlyMemory<char>> _tokenList = [];

        foreach (var _token in _tokens)
        {
            // Adiciona o token não vazio à lista.
            _tokenList.Add(new ReadOnlyMemory<char>(_trimmedInput[_token].Trim().ToArray()));
        }

        if (_tokenList.Count == 0)
            return default;

        // Converte a lista de ReadOnlyMemory<char> para um array.
        
        return CollectionsMarshal.AsSpan(_tokenList).ToEnum<T>();
    }

    public static T ToEnum<T>(this ReadOnlyMemory<char>[] input)
        where T : struct, Enum
    {
       return input.AsSpan().ToEnum<T>();
    }

    public static T ToEnum<T>(this Span<ReadOnlyMemory<char>> input)
        where T : struct, Enum, IComparable, IConvertible, ISpanFormattable
    {
        if(input.IsEmpty)
            return default;

        // Validação do tipo
        var _type = typeof(T);
        if (!_type.IsEnum)
            throw new ArgumentException("T deve ser um tipo enum.", nameof(T));

        // Cria um dicionário com comparação case-insensitive para mapear as descrições para os valores do enum.
        var _enumFields = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
        string[] _enumNames = Enum.GetNames<T>();
        T[] _enumValues = Enum.GetValues<T>();
        //T[] _enumValues2 = Enum.GetValuesAsUnderlyingType<T>();

        for (int i = 0; i < _enumNames.Length; i++)
        {
            var _field = _type.GetField(_enumNames[i]);
            var _attribute = _field?.GetCustomAttribute<DescriptionAttribute>();
            if (_attribute is not null)
            {
                _enumFields.TryAdd(_attribute.Description, _enumValues[i]);
            }
        }

        // Determina se o enum possui o atributo [Flags]
        bool _hasFlags = _type.IsDefined(typeof(FlagsAttribute), false);

        if (_hasFlags)
        {
            // Para enums com Flags, acumula os valores usando operações bit a bit.
            ulong _combined = 0;

            foreach (var _token in input)
            {
                var _trimmedToken = _token.Span.Trim();
                if (_trimmedToken.IsEmpty) continue;

                // Se o token corresponder a uma descrição mapeada, acumula seu valor.
                if (_enumFields.TryGetValue(_trimmedToken.ToString(), out var _enumVal))
                {
                    var _underType = Enum.GetUnderlyingType(_type);

                    _combined |= _enumVal.ToUInt64(null); //Convert.ToUInt64(_enumVal);
                }
            }
            return (T)Enum.ToObject(_type, _combined);
        }
        else
        {
            // Para enums simples, usa o primeiro token não vazio.
            foreach (var _token in input)
            {
                var _trimmedToken = _token.Span.Trim();
                if (_trimmedToken.IsEmpty) continue;

                if (_enumFields.TryGetValue(_trimmedToken.ToString(), out T _result))
                {
                    return _result;
                }
                else
                {
                    // Se não encontrar a descrição mapeada, retorna o valor padrão.
                    return default;
                }
            }
            return default;
        }
    }


    /// <summary>
    /// Gets the description of an enum value.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="delimiter">Default is ' ' (space)</param>
    /// <returns></returns>
    public static string GetDescription<T>(this T value, char delimiter = ' ')
        where T : struct, Enum, IComparable, IConvertible, ISpanFormattable
    {
        var type = value.GetType();
        bool hasFlagsAttribute = type.IsDefined(typeof(FlagsAttribute), false);

        if (hasFlagsAttribute)
        {
            var numericValue = value.ToUInt64(null);
            var descriptions = new List<string>();
            var stringValues = Enum.GetNames<T>();
            var values = Enum.GetValues<T>();

            //foreach (string enumValue in stringValues)
            //{

            for (int i = 0; i < stringValues.Length; i++)
            { 
                ulong flag = Convert.ToUInt64(values[i].ToUInt64(null));

                if(flag > numericValue)
                    break;

                // Para lidar com o caso em que o valor é zero:
                if (flag == 0)
                {
                    if (numericValue == 0)
                    {
                        var field = type.GetField(stringValues[i]);
                        var attr = field?.GetCustomAttribute<DescriptionAttribute>();
                        if (attr is not null)
                        {
                            descriptions.Add(attr.Description);
                        }
                    }
                }
                // Para valores diferentes de zero, verifica se o flag está presente
                else if ((numericValue & flag) == flag)
                {
                    var field = type.GetField(stringValues[i]);
                    var attr = field?.GetCustomAttribute<DescriptionAttribute>();
                    if (attr is not null)
                    {
                        descriptions.Add(attr.Description);
                    }
                }
            }

            return string.Join(delimiter, descriptions);
        }
        else
        {
            var _str = value.ToString();
            var field = type.GetField(_str);
            var attribute = field?.GetCustomAttribute<DescriptionAttribute>();
            return (attribute is null) ? _str : attribute.Description;
        }
    }

    public static Dictionary<T, string> EnumDict<T>(this Type enumType)
        where T : struct, Enum
    {
        ArgumentNullException.ThrowIfNull(enumType);

        if (enumType != typeof(T))
            throw new ArgumentException("The provided type must match the generic type T.", nameof(enumType));

        return Enum.GetValues(enumType)
                   .Cast<T>()
                   .ToDictionary(static item => item, static item => item.GetDescription());
    }
}
