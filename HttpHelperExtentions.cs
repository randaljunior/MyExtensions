using System;
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
    /// <param name="_uri"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static Uri AddUpdateQueryParameter(this Uri _uri, string key, string value)
    {
        ArgumentNullException.ThrowIfNull(_uri);

        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("A chave não pode ser nula ou vazia.", nameof(key));

        var uriBuilder = new UriBuilder(_uri);

        if (string.IsNullOrEmpty(uriBuilder.Query) || uriBuilder.Query == "?")
        {
            uriBuilder.Query = $"{HttpUtility.UrlEncode(key)}={HttpUtility.UrlEncode(value)}";
            return uriBuilder.Uri;
        }

        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        query[key] = value;
        uriBuilder.Query = query.ToString();
        return uriBuilder.Uri;
    }

    /// <summary>
    /// Remove um parâmetro de consulta da URL.
    /// </summary>
    /// <param name="_uri"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static Uri RemoveQueryParameter(this Uri _uri, string key)
    {
        ArgumentNullException.ThrowIfNull(_uri);

        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("A chave não pode ser nula ou vazia.", nameof(key));

        var uriBuilder = new UriBuilder(_uri);

        if (string.IsNullOrEmpty(uriBuilder.Query) || uriBuilder.Query == "?")
            return _uri;

        var query = HttpUtility.ParseQueryString(uriBuilder.Query);

        if (query[key] == null)
            return _uri;

        query.Remove(key);
        uriBuilder.Query = query.ToString();
        return uriBuilder.Uri;
    }
}
