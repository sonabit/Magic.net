using System;
using System.IO.Pipes;

namespace Magic.Net
{
    public sealed class NamedPipeNetConnection : NetConnection
    {
        public NamedPipeNetConnection(Uri remoteAddress, ISystem system)
            : base(CreateConnectionAdapter(remoteAddress, system), system, system.PackageHandler, system.BufferManager)
        {
        }

        private static INetConnectionAdapter CreateConnectionAdapter(Uri remoteAddress, ISystem system)
        {
            var endPoint = new MagicNetEndPoint(remoteAddress);
            var pipe = new NamedPipeClientStream(endPoint.Host, endPoint.SystemName,
                PipeDirection.InOut, PipeOptions.Asynchronous | PipeOptions.WriteThrough);

            return new NamedPipeClientStreamAdapter(pipe, endPoint.OriginUri, system.BufferManager);
        }
    }
}