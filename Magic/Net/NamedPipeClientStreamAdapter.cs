using System;
using System.IO.Pipes;
using System.ServiceModel.Channels;
using JetBrains.Annotations;

namespace Magic.Net
{
    sealed class NamedPipeClientStreamAdapter : NamedPipeAdapter
    {
        [NotNull]
        private readonly NamedPipeClientStream _stream;

        public NamedPipeClientStreamAdapter([NotNull] NamedPipeClientStream stream, Uri remoteAddress, [NotNull] BufferManager bufferManager)
            : base(stream, bufferManager)
        {
            this.RemoteAddress = remoteAddress;
            _stream = stream;
        }

        #region Overrides of ReadWriteStreamAdapter

        public sealed override void Open()
        {
            _stream.Connect(30000);
        } 
        #endregion
    }
}