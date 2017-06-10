using System;
using System.IO;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace Magic.Net
{
    internal abstract class ReadWriteStreamAdapter : INetConnectionAdapter
    {
        #region Ctors

        protected ReadWriteStreamAdapter([NotNull] Stream stream, [NotNull] ISystem system)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            if (system == null) throw new ArgumentNullException("system");
            _stream = stream;
            _system = system;
        }

        #endregion Ctors

        #region Fields

        private readonly byte[] _lenBuffer = new byte[sizeof(int)];

        private bool _disposedValue; // Dient zur Erkennung redundanter Aufrufe.
        private readonly Stream _stream;
        private readonly ISystem _system;

        #endregion Fields

        #region Implementation of INetConnectionAdapter

        public Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }

        public abstract bool IsConnected { get; }
        public Uri RemoteAddress { get; protected set; }
        public Uri LocalAddress { get; protected set; }

        public abstract void Open();

        public virtual void Close()
        {
            _stream.Close();
        }

        [CanBeNull]
        public NetPackage ReadData()
        {
            //Länge der zu empfangenden Daten lesen 4 bytes = int32
            var rlen = _stream.Read(_lenBuffer, 0, 4);
            if (0 == rlen)
            {
                // Keine Daten, die Pipe wurde geschlossen oder irgendwas lief schief
                Dispose();
                return null;
            }


            var len = BitConverter.ToInt32(_lenBuffer, 0);

            //eigentliche Daten lesen
            // Die Daten können aus mehreren Segmenten bestehen, der muss auf die Reihenfolge achten
            var currentCount = 0;
            var buffer = _system.BufferManager.TakeBuffer(len);
            while (currentCount < len)
            {
                rlen = _stream.Read(buffer, currentCount, len - currentCount);
                currentCount += rlen;
                if (rlen == 0)
                {
                    // Keine Daten, die Stream wurde geschlossen oder irgendwas lief schief
                    Dispose();
                    return null;
                }
            }

            //Debug.WriteLine("ReceiveFromStreamInternal " + (bytes1.Length + 4));

            NetDataPackage p = new NetDataPackage(buffer);
            return p;
        }

        public void WriteData(params ArraySegment<byte>[] buffers)
        {
            // ReSharper disable once BuiltInTypeReferenceStyle
            var len = buffers.Sum(b => b.Count);
            var lenBuffer = _system.BufferManager.TakeBuffer(4);

            len.ToBuffer(lenBuffer);
            _stream.Write(lenBuffer, 0, 4);
            _system.BufferManager.ReturnBuffer(lenBuffer);

            foreach (var arraySegment in buffers)
                _stream.Write(arraySegment.Array, arraySegment.Offset, arraySegment.Count);

            _stream.Flush();
        }

        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: verwalteten Zustand (verwaltete Objekte) entsorgen.
                }

                // TODO: nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer weiter unten überschreiben.
                // TODO: große Felder auf Null setzen.

                _disposedValue = true;
            }
        }

        // TODO: Finalizer nur überschreiben, wenn Dispose(bool disposing) weiter oben Code für die Freigabe nicht verwalteter Ressourcen enthält.
        // ~ReadWriteStreamAdapter() {
        //   // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in Dispose(bool disposing) weiter oben ein.
        //   Dispose(false);
        // }

        // Dieser Code wird hinzugefügt, um das Dispose-Muster richtig zu implementieren.
        public void Dispose()
        {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in Dispose(bool disposing) weiter oben ein.
            Dispose(true);
            // TODO: Auskommentierung der folgenden Zeile aufheben, wenn der Finalizer weiter oben überschrieben wird.
            // GC.SuppressFinalize(this);
        }

        #endregion

        #endregion
    }
}