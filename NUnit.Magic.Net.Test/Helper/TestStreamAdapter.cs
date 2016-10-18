using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Magic.Net;

namespace NUnit.Magic.Net.Test.Helper
{
    sealed class TestStreamAdapter : INetConnectionAdapter
    {
        private readonly WaitHandle _stopHandle;

        #region Fields

        private readonly Queue<NetPackage> _inStream;
        private readonly Stream _outStream;
        private readonly AutoResetEvent _canReadEvent = new AutoResetEvent(false);

        #endregion

        public TestStreamAdapter(Uri localAddress, Uri remoteAddress, Stream writeStream, WaitHandle stopHandle)
        {
            _stopHandle = stopHandle;
            RemoteAddress = remoteAddress;
            LocalAddress = localAddress;
            _inStream = new Queue<NetPackage>();
            _outStream = writeStream;
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            Close();
            if (_outStream != null) _outStream.Dispose();
        }

        #endregion

        #region Implementation of INetConnectionAdapter

        public Encoding Encoding { get { return Encoding.UTF8;} }
        public bool IsConnected { get; private set; }
        public Uri RemoteAddress { get; private set; }
        public Uri LocalAddress { get; private set; }
        public void Open()
        {
            IsConnected = true;
        }

        public void Close()
        {
            IsConnected = false;
        }

        public NetPackage ReadData()
        {
            WaitHandle.WaitAny(new WaitHandle[] { _canReadEvent, _stopHandle});

            if (_inStream.Count == 0 || !IsConnected)
            {
                if (_stopHandle.WaitOne(0))
                {
                    IsConnected = false;
                }
                return null;
            }

            if (_inStream.Count > 0)
            {
                _canReadEvent.Set();
            }

            return _inStream.Dequeue();
        }

        public void AddNextReadPackages([NotNull]IEnumerable<NetPackage> packages )
        {
            packages.Each(_inStream.Enqueue);
            _canReadEvent.Set();
        }

        public void WriteData(params ArraySegment<byte>[] buffers)
        {
            if (OnWriteData != null)
            {
                byte[] data = new byte[buffers.Sum(b => b.Count)];
                int len = 0;
                foreach (ArraySegment<byte> arraySegment in buffers)
                {
                    Buffer.BlockCopy(arraySegment.Array,arraySegment.Offset, data, len, arraySegment.Count);
                    len = arraySegment.Count;
                }

                OnWriteData(this, data);
            }
        }

        public event Action<TestStreamAdapter, byte[]> OnWriteData;
        

        #endregion
    }
}
