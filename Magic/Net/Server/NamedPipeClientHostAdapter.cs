using System;
using System.IO.Pipes;
using System.ServiceModel.Channels;
using System.Text;
using JetBrains.Annotations;

namespace Magic.Net.Server
{
    internal sealed class NamedPipeClientHostAdapter : NamedPipeAdapter
    {
        [NotNull] private readonly NamedPipeServerStream _stream;
        private readonly Uri _localAddress;

        internal NamedPipeClientHostAdapter([NotNull] NamedPipeServerStream stream, Uri localAddress, [NotNull] BufferManager bufferManager)
            : base(stream, bufferManager)
        {
            _stream = stream;
            _localAddress = localAddress;
        }
        
        internal void Initialize()
        {
            var data = ReadData();
            if (data == null || data.PackageContentType != DataPackageContentType.ConnectionMetaData)
            {
                Dispose();
                throw new NetCommandException(NetCommandExceptionReasonses.PackeContentTypeRejected,
                    "The first have to be a DataPackageContentType.ConnectionMetaData");
            }

            if (data.PackageContentType == DataPackageContentType.ConnectionMetaData)
            {
                var uriString = Encoding.UTF8.GetString(data.Buffer.Array, data.Buffer.Offset, data.Buffer.Count);
                RemoteAddress = new Uri(uriString);
            }
        }

        #region Overrides of ReadWriteStreamAdapter

        public override void Open()
        {
            throw new NotSupportedException();
        }

        public override void Close()
        {
            _stream.Disconnect();
            base.Close();
        }

        #endregion
    }
}