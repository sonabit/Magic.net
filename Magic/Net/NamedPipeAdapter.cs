using System.IO.Pipes;
using JetBrains.Annotations;

namespace Magic.Net
{
    internal abstract class NamedPipeAdapter : ReadWriteStreamAdapter
    {
        private readonly PipeStream _stream;

        protected NamedPipeAdapter([NotNull] PipeStream stream, [NotNull] ISystem system) 
            : base(stream, system)
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