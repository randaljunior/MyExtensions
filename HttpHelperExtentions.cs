using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MyExtensions;

public static class HttpHelperExtentions
{
    /// <summary>
    /// Adiciona ou atualiza um parâmetro de consulta na URL.
    /// </summary>
    /// <param name="uri"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static Uri AddUpdateQueryParameter(this Uri uri, string key, string value)
    {
        ArgumentNullException.ThrowIfNull(uri);

        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("A chave não pode ser nula ou vazia.", nameof(key));


        if (string.IsNullOrEmpty(uri.Query) || uri.Query == "?")
        {
            string encodedKey = HttpUtility.UrlEncode(key);
            string encodedValue = HttpUtility.UrlEncode(value);
            string paramToAdd = $"{encodedKey}={encodedValue}";

            return new Uri(uri.GetLeftPart(UriPartial.Path) + "?" + paramToAdd, uri.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative);
        }

        var query = HttpUtility.ParseQueryString(uri.Query);
        if (query[key] == value)
        {
            return uri; // Retorna a URI original se o valor já for o mesmo
        }

        query[key] = value;

        var uriBuilder = new UriBuilder(uri)
        {
            Query = query.ToString()
        };

        return uriBuilder.Uri;
    }

    /// <summary>
    /// Remove um parâmetro de consulta da URL.
    /// </summary>
    /// <param name="uri"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static Uri RemoveQueryParameter(this Uri uri, string key)
    {
        ArgumentNullException.ThrowIfNull(uri);

        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("A chave não pode ser nula ou vazia.", nameof(key));

        if (string.IsNullOrEmpty(uri.Query) || uri.Query == "?")
            return uri;

        var query = HttpUtility.ParseQueryString(uri.Query);

        if (query[key] == null)
            return uri;

        query.Remove(key);

        if (query.Count == 0)
        {
            return new Uri(uri.GetLeftPart(UriPartial.Path), uri.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative);
        }

        var uriBuilder = new UriBuilder(uri)
        {
            Query = query.ToString()
        };
        return uriBuilder.Uri;
    }

    /// <summary>
    /// Remove múltiplos parâmetros de consulta da URL.
    /// </summary>
    /// <param name="uri"></param>
    /// <param name="keys"></param>
    /// <returns></returns>
    public static Uri RemoveQueryParameters(this Uri uri, params string[] keys)
    {
        ArgumentNullException.ThrowIfNull(uri);

        if (keys is null || keys.Length == 0)
            return uri;


        if (string.IsNullOrEmpty(uri.Query) || uri.Query == "?")
            return uri;

        var query = HttpUtility.ParseQueryString(uri.Query);

        bool anyRemoved = false;

        foreach (var key in keys)
        {
            if (!string.IsNullOrEmpty(key) && query[key] is not null)
            {
                query.Remove(key);
                anyRemoved = true;
            }
        }

        if (!anyRemoved)
            return uri;

        if (query.Count == 0)
        {
            return new Uri(uri.GetLeftPart(UriPartial.Path), uri.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative);
        }

        var uriBuilder = new UriBuilder(uri)
        {
            Query = query.ToString()
        };
        return uriBuilder.Uri;
    }

    /// <summary>
    /// Substitui os partes de uma URL.
    /// </summary>
    /// <param name="uri"></param>
    /// <param name="replacementTokens"></param>
    /// <returns></returns>
    public static Uri ReplaceTokens(this Uri uri, FrozenDictionary<string, string> replacementTokens)
    {
        ArgumentNullException.ThrowIfNull(uri);
        var uriBuilder = new UriBuilder(uri);
        uriBuilder.Path = uriBuilder.Path.AsSpan().ReplaceTokens('{','}', replacementTokens);

        return uriBuilder.Uri;
    }
}
