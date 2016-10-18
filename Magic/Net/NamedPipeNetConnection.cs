using System;
using System.IO.Pipes;

namespace Magic.Net
{
    public sealed class NamedPipeNetConnection : NetConnection
    {
        private readonly Uri _remoteAddress;

        public NamedPipeNetConnection(Uri remoteAddress, ISystem system)
            : base()
        {
            _remoteAddress = remoteAddress;
        }

        #region Overrides of NetConnection

        protected override INetConnectionAdapter CreateAdapter(ISystem system)
        {
            var endPoint = new MagicNetEndPoint(_remoteAddress);
            var pipe = new NamedPipeClientStream(endPoint.Host, endPoint.SystemName,
                PipeDirection.InOut, PipeOptions.Asynchronous | PipeOptions.WriteThrough);

            return new NamedPipeClientStreamAdapter(pipe, endPoint.OriginUri, system);
        }

        #endregion
    }
}