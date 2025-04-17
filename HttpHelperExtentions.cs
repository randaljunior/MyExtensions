using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MyExtensions;

public static class HttpHelperExtentions
{
    public static Uri AddUpdateQueryParameter(this Uri _uri, string key, string value)
    {
        var uriBuilder = new UriBuilder(_uri);
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        query[key] = value;
        uriBuilder.Query = query.ToString();
        return uriBuilder.Uri;
    }

    public static Uri RemoveQueryParameter(this Uri _uri, string key)
    {
        var uriBuilder = new UriBuilder(_uri);
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        query.Remove(key);
        uriBuilder.Query = query.ToString();
        return uriBuilder.Uri;
    }
}
