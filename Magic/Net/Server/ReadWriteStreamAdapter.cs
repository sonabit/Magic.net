using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using JetBrains.Annotations;

namespace Magic.Net.Server
{
    internal abstract class ReadWriteStreamAdapter :  INetConnectionAdapter
    {
        #region Fields
        private readonly byte[] _lenBuffer = new byte[sizeof(int)];

        private bool disposedValue = false; // Dient zur Erkennung redundanter Aufrufe.
        private readonly Stream _stream;
        private readonly IBufferManager _bufferManager;

        #endregion Fields

        public ReadWriteStreamAdapter([NotNull]Stream stream,[NotNull] IBufferManager bufferManager)
        {
            this._stream = stream;
            this._bufferManager = bufferManager;
        }

        #region Implementation of INetConnectionAdapter

        public abstract bool IsConnected { get; }
        public Uri Address { get; private set; }
        public void Open(bool withOwnThread)
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            throw new NotImplementedException();
        }

        [CanBeNull]
        public NetDataPackage ReadData()
        {
            //Länge der zu empfangenden Daten lesen 4 bytes = int32
            var rlen = _stream.Read(_lenBuffer, 0, 4);
            if (0 == rlen)
            {
                // Keine Daten, die Pipe wurde geschlossen oder irgendwas lief schief
                Dispose();
                return null;
            }


            var _len = BitConverter.ToInt32(_lenBuffer, 0);

            //eigentliche Daten lesen
            // Die Daten können aus mehreren Segmenten bestehen, der muss auf die Reihenfolge achten
            var currentCount = 0;
            var bytes1 = new byte[_len];
            while (currentCount < _len)
            {
                rlen = _stream.Read(bytes1, currentCount, _len - currentCount);
                currentCount += rlen;
                if (rlen == 0)
                {
                    // Keine Daten, die Pipe wurde geschlossen oder irgendwas lief schief
                    Dispose();
                    return null;
                }
            }

            //Debug.WriteLine("ReceiveFromStreamInternal " + (bytes1.Length + 4));
            var p = new NetDataPackage(bytes1);
            return  p;
        }

        public void WriteData(IEnumerable<ArraySegment<byte>> buffers)
        {
            foreach (var buffer in buffers)
            {
                _stream.Write(buffer.Array, buffer.Offset, buffer.Count);
            }
            _stream.Flush();
        }

        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: verwalteten Zustand (verwaltete Objekte) entsorgen.
                }

                // TODO: nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer weiter unten überschreiben.
                // TODO: große Felder auf Null setzen.

                disposedValue = true;
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