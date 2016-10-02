using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Magic.Net
{

    public sealed class NamedPipeNetConnection : NetConnection
    {
        public NamedPipeNetConnection(Uri remoteUri, ISystem system) 
            : base(CreateConnectionAdapter(remoteUri, system), system.PackageHandler, system.BufferManager)
        {

        }

        private static INetConnectionAdapter CreateConnectionAdapter(Uri remoteUri, ISystem system)
        {
            var pipe = new NamedPipeClientStream(remoteUri.Host, remoteUri.GetStringOfSegment(1),
                    PipeDirection.InOut, PipeOptions.Asynchronous | PipeOptions.WriteThrough);
            return new NamedPipeClientStreamAdapter(pipe, new Uri(string.Format("magic://{0}/{1}", remoteUri.Host, remoteUri.GetStringOfSegment(1))), system.BufferManager);
        }
    }
}
