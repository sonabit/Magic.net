using System.IO.Pipes;
using System.ServiceModel.Channels;
using JetBrains.Annotations;
using Magic.Net.Server;

namespace Magic.Net
{
    internal abstract class NamedPipeAdapter : ReadWriteStreamAdapter
    {
        private readonly PipeStream _stream;

        protected NamedPipeAdapter([NotNull] PipeStream stream, [NotNull] BufferManager bufferManager) 
            : base(stream, bufferManager)
        {
            _stream = stream;
        }

        #region Overrides of ReadWriteStreamAdapter

        public override bool IsConnected
        {
            get { return _stream.IsConnected; }
        }
        
        #endregion
    }
}