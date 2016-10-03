using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.ServiceModel.Channels;
using System.Threading;
using JetBrains.Annotations;

namespace Magic.Net
{
    public class NetConnection : INetConnection
    {
        #region Fields

        private readonly INetConnectionAdapter _connectionAdapter;
        private readonly ISystem _system;
        private readonly BufferManager _bufferManager;
        private static int[] _bufferFilter;
        private readonly IDataPackageDispatcher _dataPackageDispatcher;
        private readonly ConcurrentQueue<NetDataPackage> _receivedDataQueue = new ConcurrentQueue<NetDataPackage>();
        private readonly ConcurrentQueue<ArraySegment<byte>[]> _sendingQueue =
            new ConcurrentQueue<ArraySegment<byte>[]>();

        private readonly AutoResetEvent _receivedDataResetEvent = new AutoResetEvent(false);
        private readonly AutoResetEvent _processSendingWaiter = new AutoResetEvent(false);
        private bool _isDisposed = false;
        // ReSharper disable once InconsistentNaming
        private event Action<INetConnection> _disconnected;
        
        #endregion Fields

        #region Ctros

        public NetConnection(INetConnectionAdapter connectionAdapter, ISystem system, IDataPackageHandler dataPackageHandler, BufferManager bufferManager)
        {
            TimeoutMilliseconds = Timeout.Infinite;
            _connectionAdapter = connectionAdapter;
            _system = system;
            _bufferManager = bufferManager;
            _dataPackageDispatcher = new DataPackageDispatcher(dataPackageHandler);
        }

        #endregion  Ctros

        #region INetConnection

        public event Action<INetConnection> Disconnected
        {
            add { this._disconnected += value; }
            remove { this._disconnected -= value; }
        }

        public void Send(params byte[][] bytes)
        {
            int len = bytes.Sum(b => b.Length);
            byte[] segmensts = _bufferManager.TakeBuffer(len);
            int pos = 0;
            foreach (Array buffer in bytes)
            {
                Buffer.BlockCopy(buffer, 0, segmensts, pos, buffer.Length);
                pos += buffer.Length;
            }
            _sendingQueue.Enqueue(new[] {new ArraySegment<byte>(segmensts, 0, len)});
            _processSendingWaiter.Set();
        }

        public void Send(params ArraySegment<byte>[] segments)
        {
            _sendingQueue.Enqueue(segments);
            _processSendingWaiter.Set();
        }

        public int TimeoutMilliseconds { get; [PublicAPI]set; }
        public DataSerializeFormat DefaultSerializeFormat { get { return DataSerializeFormat.MsBinary;} }

        public void Open()
        {
            if (!_connectionAdapter.IsConnected)
            {
                _connectionAdapter.Open();
            }

            byte[] buffer = _connectionAdapter.Encoding.GetBytes(this.RemoteAddress.ToString());

            NetDataPackage package =
                new NetDataPackage(
                    new NetDataPackageHeader(1, DataPackageContentType.ConnectionMetaData, DataSerializeFormat.Custom),
                    buffer, 0, buffer.Length);

            this.Send(package.DataSegments().ToArray());
            
            InitializeConnection(true);
        }

        public void Close()
        {
            _connectionAdapter.Close();
            _receivedDataResetEvent.Set();
            OnDisconnected();
        }

        public bool IsConnected
        {
            get { return _connectionAdapter.IsConnected; }
        }

        public Uri RemoteAddress
        {
            get { return _connectionAdapter.RemoteAddress; }
        }

        #endregion INetConnection
        
        internal void InitializeConnection(bool withNewThread = false)
        {
            BeginSending(withNewThread);
            
            BeginRead(true);
        }

        [ExcludeFromCodeCoverage]
        private void BeginRead(bool withNewThread = false)
        {
            new Thread(ProcessingReceivedDataQueueInternal) { IsBackground = true }.Start();
            if (withNewThread)
            {
                new Thread(ReadDataInternal) {IsBackground = true}.Start();
            }
            else
            {
                ReadDataInternal();
            }
        }

        private void ReadDataInternal()
        {
            while (_connectionAdapter.IsConnected)
            {
                try
                {
                    var package = _connectionAdapter.ReadData();
                    if (package != null && package.Buffer.ToArray().Length > 0)
                        AddToReceivedDataQueue(package);
                }
                catch (Exception)
                {
                    if (!_connectionAdapter.IsConnected)
                        break;
                }
            }
        }

        private void ProcessingReceivedDataQueueInternal()
        {
            while (_connectionAdapter.IsConnected)
            {
                try
                {
                    _receivedDataResetEvent.WaitOne();
                    DequeueReceivedData();
                }
                catch (ThreadAbortException)
                {
                    break;
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
            }
        }

        protected void DequeueReceivedData()
        {
            while (!_receivedDataQueue.IsEmpty)
            {
                NetDataPackage package;
                if (!_receivedDataQueue.TryDequeue(out package))
                    break;
                if (package.IsEmpty) continue;

                _dataPackageDispatcher.Handle(new RequestState(this, package));
            }
        }

        protected void AddToReceivedDataQueue([NotNull] NetDataPackage buffer)
        {
            _receivedDataQueue.Enqueue(buffer);
            _receivedDataResetEvent.Set();
        }

        private static int[] BufferFilter
        {
            get
            {
                if (_bufferFilter != null) return _bufferFilter;

                _bufferFilter = new int[24];
                for (var power = 7; power < 31; power++)
                    _bufferFilter[power - 7] = Convert.ToInt32(Math.Pow(2, power));

                return _bufferFilter;
            }
        }

        [ExcludeFromCodeCoverage]
        private void BeginSending(bool withNewThread = false)
        {
            if (withNewThread)
            {
                new Thread(SendingInternal) { IsBackground = true }.Start();
            }
            else
            {
                SendingInternal();
            }
        }
        private void SendingInternal()
        {
            ArraySegment<byte>[] buffers = null;
            
            while (IsConnected)
            {
                _processSendingWaiter.WaitOne();

                if (!IsConnected) break;
                if (_isDisposed) break;

                while (!_isDisposed && IsConnected && !_sendingQueue.IsEmpty)
                {
                    if (_sendingQueue.TryPeek(out buffers))
                    {
                        if (buffers.Any(b => b.Count > 0))
                        {
                            try
                            {
                                if (!IsConnected) break;
                                _connectionAdapter.WriteData(buffers);
                            }
                            catch (Exception exception)
                            {
                                Trace.TraceError(exception.ToString());
                                break;
                            }
                        }
                        if (!_sendingQueue.TryDequeue(out buffers)) //&& buffers != null
                        {
                            var spin = new SpinWait();
                            while (!_sendingQueue.TryDequeue(out buffers))
                            {
                                spin.SpinOnce();
                            }
                        }

                        foreach (ArraySegment<byte> buffer in buffers)
                        {
                            if (Array.IndexOf(BufferFilter, buffer.Array.Length) > -1)
                                try
                                {
                                    _bufferManager.ReturnBuffer(buffer.Array);
                                }
                                catch (Exception exception)
                                {
                                    Trace.TraceError("Not enable to return buffer Length {0} : {1}", buffer.Array.Length,
                                        exception.Message);
                                }
                        }
                        
                        buffers = null;

                        if (_isDisposed) break;
                    }
                }
            }

            AbortedSending(buffers);
        }

        private void AbortedSending(ArraySegment<byte>[] buffers)
        {
            Trace.TraceInformation(string.Format("{0}.AbortedSending {1}", typeof (NetConnection).Name, _connectionAdapter.RemoteAddress));
        }

        private void OnDisconnected()
        {
            Action<INetConnection> handler = _disconnected;
            if (handler != null) handler(this);
        }
    }
}