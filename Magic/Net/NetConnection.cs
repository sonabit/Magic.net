using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.ServiceModel.Channels;
using System.Threading;
using JetBrains.Annotations;
using Magic.Net.Data;
using Magic.Serialization;

namespace Magic.Net
{
    public abstract class NetConnection : INetConnection
    {
        #region Ctros

        protected NetConnection()
        {
            TimeoutMilliseconds = Timeout.Infinite;
        }

        #endregion  Ctros

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

        protected abstract INetConnectionAdapter CreateAdapter(ISystem system);

        internal void InitializeConnection(bool withNewThread = false)
        {
            BeginSending(withNewThread);

            BeginRead(true);
        }

        [ExcludeFromCodeCoverage]
        private void BeginRead(bool withNewThread = false)
        {
            new Thread(ProcessingReceivedDataQueueInternal) {IsBackground = true}.Start();
            if (withNewThread)
                new Thread(ReadDataInternal) {IsBackground = true}.Start();
            else
                ReadDataInternal();
        }

        private void ReadDataInternal()
        {
            if (_isReading) return;
            _isReading = true;
            while (_connectionAdapter.IsConnected)
                try
                {
                    NetPackage package = _connectionAdapter.ReadData();
                    if ((package != null) && !package.IsEmpty)
                        AddToReceivedDataQueue(package);
                }
                catch (Exception)
                {
                    if (!_connectionAdapter.IsConnected)
                        break;
                }
            _isReading = false;
        }

        private void ProcessingReceivedDataQueueInternal()
        {
            while (_connectionAdapter.IsConnected)
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

        protected void DequeueReceivedData()
        {
            while (!_receivedDataQueue.IsEmpty)
            {
                NetPackage package;
                if (!_receivedDataQueue.TryDequeue(out package))
                    break;
                if (package.IsEmpty) continue;

                NetOjectPackage data = Deserialize(package);

                _dataPackageDispatcher.Handle(new RequestState(this, data));
            }
        }

        [NotNull]
        private NetOjectPackage Deserialize(NetPackage package)
        {
            NetDataPackage dataPackage = package as NetDataPackage;
            if (dataPackage != null)
                switch (package.PackageContentType)
                {
                    case DataPackageContentType.NetCommand:
                        return Deserialize<NetCommand>(dataPackage);
                    case DataPackageContentType.NetCommandResult:
                        return Deserialize<NetCommandResult>(dataPackage);
                    case DataPackageContentType.NetObjectStreamInitialize:
                        return Deserialize<NetObjectStreamInitializeRequest>(dataPackage);
                    case DataPackageContentType.NetObjectStreamData:
                        throw new NotImplementedException();
                    case DataPackageContentType.NetObjectStreamClose:
                        throw new NotImplementedException();
                    case DataPackageContentType.ConnectionMetaData:
                        throw new NotImplementedException();
                    default:
                        // this case should never happened
                        throw new NetException(NetExceptionReasonses.UnknownPackageContentType,
                            string.Format("PackageContentType {0} unknown.", package.PackageContentType));
                }
            NetOjectPackage result = (NetOjectPackage) package;
            return result;
        }

        private NetOjectPackage Deserialize<T>(NetDataPackage package)
        {
            ISerializeFormatter serializeFormatter = _system.FormatterCollection.GetFormatter(package.SerializeFormat);

            return
                new NetOjectPackage(
                    NetDataPackageHeader.CreateNetDataPackageHeader(package.PackageContentType, package.SerializeFormat,
                        package.Version),
                    serializeFormatter.Deserialize<T>(package.Buffer.Array, package.Buffer.Offset));
        }

        protected void AddToReceivedDataQueue([NotNull] NetPackage buffer)
        {
            _receivedDataQueue.Enqueue(buffer);
            _receivedDataResetEvent.Set();
        }

        [ExcludeFromCodeCoverage]
        private void BeginSending(bool withNewThread = false)
        {
            if (withNewThread)
                new Thread(SendingInternal) {IsBackground = true}.Start();
            else
                SendingInternal();
        }

        private void SendingInternal()
        {
            if (_isSending) return;
            _isSending = true;

            ArraySegment<byte>[] buffers = null;

            while (IsConnected)
            {
                _processSendingWaiter.WaitOne();

                if (!IsConnected) break;
                if (_isDisposed) break;

                while (!_isDisposed && IsConnected && !_sendingQueue.IsEmpty)
                    if (_sendingQueue.TryPeek(out buffers))
                    {
                        if (buffers.Any(b => b.Count > 0))
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

                        if (!_sendingQueue.TryDequeue(out buffers)) //&& buffers != null
                        {
                            SpinWait spin = new SpinWait();
                            while (!_sendingQueue.TryDequeue(out buffers))
                                spin.SpinOnce();
                        }

                        ReturnBuffers(buffers);

                        buffers = null;

                        if (_isDisposed) break;
                    }
            }
            _isSending = false;
            AbortedSending(buffers);
        }

        private void ReturnBuffers(IEnumerable<ArraySegment<byte>> buffers)
        {
            foreach (var buffer in buffers)
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

        // ReSharper disable once UnusedParameter.Local
        private void AbortedSending(ArraySegment<byte>[] buffers)
        {
            Trace.TraceInformation("{0}.AbortedSending {1}", typeof(NetConnection).Name,
                _connectionAdapter.RemoteAddress);
        }

        private void OnDisconnected()
        {
            var handler = _disconnected;
            if (handler != null) handler(this);
        }

        #region Fields

        private INetConnectionAdapter _connectionAdapter;
        private ISystem _system;
        private BufferManager _bufferManager;
        private static int[] _bufferFilter;
        private IDataPackageDispatcher _dataPackageDispatcher;
        private readonly ConcurrentQueue<NetPackage> _receivedDataQueue = new ConcurrentQueue<NetPackage>();

        private readonly ConcurrentQueue<ArraySegment<byte>[]> _sendingQueue =
            new ConcurrentQueue<ArraySegment<byte>[]>();

        private readonly AutoResetEvent _receivedDataResetEvent = new AutoResetEvent(false);
        private readonly AutoResetEvent _processSendingWaiter = new AutoResetEvent(false);
        private readonly bool _isDisposed = false;
        private bool _isInitialized;
        private bool _isReading;
        private bool _isSending;

        // ReSharper disable once InconsistentNaming
        private event Action<INetConnection> _disconnected;

        #endregion Fields

        #region INetConnection

        public void LinkTo(ISystem system)
        {
            if (_isInitialized) return;
            _isInitialized = true;

            _system = system;
            _bufferManager = system.BufferManager;
            _dataPackageDispatcher = new DataPackageDispatcher(system.PackageHandler);

            _connectionAdapter = CreateAdapter(_system);

            _system.AddConnection(this);
        }

        public event Action<INetConnection> Disconnected
        {
            add { _disconnected += value; }
            remove { _disconnected -= value; }
        }

        public void Send(params byte[][] bytes)
        {
            var len = bytes.Sum(b => b.Length);
            var segmensts = _bufferManager.TakeBuffer(len);
            var pos = 0;
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

        public int TimeoutMilliseconds { get; [PublicAPI] set; }

        public DataSerializeFormat DefaultSerializeFormat
        {
            get { return DataSerializeFormat.MsBinary; }
        }

        public void Open()
        {
            if (!_connectionAdapter.IsConnected)
            {
                _connectionAdapter.Open();


                var buffer = _connectionAdapter.Encoding.GetBytes(LocalAddress.ToString());

                NetDataPackage package =
                    new NetDataPackage(
                        NetDataPackageHeader.CreateNetDataPackageHeader(DataPackageContentType.ConnectionMetaData,
                            DataSerializeFormat.Custom),
                        buffer, 0, buffer.Length);

                Send(package.DataSegments().ToArray());
            }

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
            get { return _isSending && _isReading && _connectionAdapter.IsConnected; }
        }

        public Uri RemoteAddress
        {
            get { return _connectionAdapter.RemoteAddress; }
        }

        public Uri LocalAddress
        {
            get { return _connectionAdapter.LocalAddress; }
        }

        #endregion INetConnection
    }
}