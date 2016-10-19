using System;
using System.IO.Pipes;
using System.Text;
using JetBrains.Annotations;

namespace Magic.Net.Server
{
    internal sealed class NamedPipeClientAdapterHost : NamedPipeAdapter
    {
        [NotNull] private readonly NamedPipeServerStream _stream;

        internal NamedPipeClientAdapterHost([NotNull] NamedPipeServerStream stream, [NotNull] ISystem system)
            : base(stream, system)
        {
            _stream = stream;
        }

        internal void Initialize()
        {
            NetPackage data = ReadData();
            NetDataPackage dataPackage = data as NetDataPackage;
            if ((dataPackage == null) || (dataPackage.PackageContentType != DataPackageContentType.ConnectionMetaData))
            {
                Dispose();
                throw new NetException(NetExceptionReasonses.PackeContentTypeRejected,
                    "The first have to be a DataPackageContentType.ConnectionMetaData");
            }

            if (dataPackage.PackageContentType == DataPackageContentType.ConnectionMetaData)
            {
                var uriString = Encoding.UTF8.GetString(dataPackage.Buffer.Array, dataPackage.Buffer.Offset,
                    dataPackage.Buffer.Count);
                RemoteAddress = new Uri(uriString, UriKind.Relative);
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