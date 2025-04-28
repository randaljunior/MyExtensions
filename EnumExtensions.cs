using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

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
    /// <exception cref="ArgumentException"></exception>
    public static T? ToEnum<T>(this ReadOnlySpan<char> input, char separator = default)
    where T : notnull, Enum, new()
    {
        if(input.IsEmpty || input.IsWhiteSpace())
            return default;

        var _trimmedInput = input.Trim();

        // Validação do tipo
        var _type = typeof(T);
        if (!_type.IsEnum)
            throw new ArgumentException("T deve ser um tipo enum.", nameof(T));

        // Cria um dicionário com comparação case-insensitive para mapear as descrições para os valores do enum.
        var _enumFields = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
        foreach (T enumValue in Enum.GetValues(_type))
        {
            var _field = _type.GetField(enumValue.ToString());
            var _attribute = _field?.GetCustomAttribute<DescriptionAttribute>();
            if (_attribute is not null)
            {
                _enumFields.TryAdd(_attribute.Description, enumValue);
            }
        }

        // Determina se o enum possui o atributo [Flags]
        bool _hasFlags = _type.IsDefined(typeof(FlagsAttribute), false);

        // Se um separador for especificado, utiliza-o; caso contrário, divide a entrada por vírgula e espaço.
        var _tokens = separator != default ? _trimmedInput.Split(separator) : _trimmedInput.SplitAny(',', ' ');

        if (_hasFlags)
        {
            // Para enums com Flags, acumula os valores usando operações bit a bit.
            ulong _combined = 0;

            foreach (var _token in _tokens)
            {
                // Obtém o token e faz o trim dos espaços.
                var _trimmedToken = _trimmedInput[_token].Trim();
                if (_trimmedToken.IsEmpty) continue;

                // Se o token corresponder a uma descrição mapeada, acumula seu valor.
                if (_enumFields.TryGetValue(_trimmedToken.ToString(), out T? _enumVal) && _enumVal is not null)
                {
                    _combined |= Convert.ToUInt64(_enumVal);
                }
            }
            return (T)Enum.ToObject(_type, _combined);
        }
        else
        {
            // Para enums simples, usa o primeiro token não vazio.
            foreach (var _token in _tokens)
            {
                var _trimmedToken = _trimmedInput[_token].Trim();
                if (_trimmedToken.IsEmpty) continue;

                if (_enumFields.TryGetValue(_trimmedToken.ToString(), out T? _result) && _result is not null)
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
    public static string GetDescription(this Enum value, char delimiter = ' ')
    {
        ArgumentNullException.ThrowIfNull(value);

        var type = value.GetType();
        bool hasFlagsAttribute = type.IsDefined(typeof(FlagsAttribute), false);

        if (hasFlagsAttribute)
        {
            var numericValue = Convert.ToUInt64(value);
            var descriptions = new List<string>();
            var values = Enum.GetValues(type);

            foreach (Enum enumValue in values)
            {
                ulong flag = Convert.ToUInt64(enumValue);

                // Para lidar com o caso em que o valor é zero:
                if (flag == 0)
                {
                    if (numericValue == 0)
                    {
                        var field = type.GetField(enumValue.ToString());
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
                    var field = type.GetField(enumValue.ToString());
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
            var field = type.GetField(value.ToString());
            var attribute = field?.GetCustomAttribute<DescriptionAttribute>();
            return (attribute is null) ? value.ToString() : attribute.Description;
        }
    }

    public static Dictionary<T, string> EnumDict<T>(this Type enumType) where T : Enum
    {
        ArgumentNullException.ThrowIfNull(enumType);

        if (enumType != typeof(T))
            throw new ArgumentException("The provided type must match the generic type T.", nameof(enumType));

        return Enum.GetValues(enumType)
                   .Cast<T>()
                   .ToDictionary(item => item, item => item.GetDescription());
    }
}
