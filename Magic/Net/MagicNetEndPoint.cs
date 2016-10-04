using System;
using JetBrains.Annotations;

namespace Magic.Net
{
    [PublicAPI]
    public sealed class MagicNetEndPoint
    {
        public const string Schema = "magic";
        public const string SchemaPrefix = Schema + "://";

        private readonly Uri _uri;

        public MagicNetEndPoint([NotNull] Uri uri)
        {
            _uri = uri;
            SystemName = _uri.GetStringOfSegment(1);
        }

        public Uri OriginUri
        {
            get { return _uri; }
        }

        public string Host
        {
            get { return _uri.Host; }
        }

        public int Port
        {
            get { return _uri.Port; }
        }
        
        public string SystemName { get; private set; }

        public static Uri BuildUri(string host, int port, string systemName)
        {
            return BuildBaseUri(host, port, systemName);
        }

        public static Uri BuildUri(string host, string systemName)
        {
            return BuildBaseUri(host, 0, systemName);
        }

        public static Uri BuildBaseUri(string host, int port, string systemName)
        {
            UriBuilder urb = new UriBuilder(Schema, host, port, systemName);
            return urb.Uri;
        }

    }
}