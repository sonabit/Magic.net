using System;
using JetBrains.Annotations;

namespace Magic.Net
{
    internal static class UriExtensions
    {
        public static Uri AsLocalUri(this Uri uri)
        {
            UriBuilder urb = new UriBuilder(uri)
            {
                
                Host = System.Environment.MachineName
            };
            return urb.Uri;
        }

        [PublicAPI]
        public static Uri AsRemoteUri(this Uri uri, string remoteHost)
        {
            UriBuilder urb = new UriBuilder(uri)
            {
                Host = remoteHost
            };
            return urb.Uri;
        }

        public static Uri AddPath([NotNull]this Uri baseUri, string relativPath)
        {
            UriBuilder builder = new UriBuilder(baseUri);

            if (relativPath[0] != '/')
                builder.Path += "/";
            builder.Path += relativPath;

            return builder.Uri;
        }
    }
}
