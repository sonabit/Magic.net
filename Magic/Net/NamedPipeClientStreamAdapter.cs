using System;
using System.IO.Pipes;
using JetBrains.Annotations;

namespace Magic.Net
{
    sealed class NamedPipeClientStreamAdapter : NamedPipeAdapter
    {
        [NotNull]
        private readonly NamedPipeClientStream _stream;

        public NamedPipeClientStreamAdapter([NotNull] NamedPipeClientStream stream, Uri remoteAddress, [NotNull] ISystem system)
            : base(stream, system)
        {
            this.RemoteAddress = remoteAddress;
            this.LocalAddress = remoteAddress.AsLocalUri();
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