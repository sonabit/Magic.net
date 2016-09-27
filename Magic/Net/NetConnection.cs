using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;

namespace Magic.Net
{
    public class NetConnection : INetConnection
    {
        private readonly INetConnectionAdapter _connectionAdapter;
        private readonly IDataPackageDispatcher _dataPackageDispatcher;
        private readonly ConcurrentQueue<NetDataPackage> _receivedDataQueue = new ConcurrentQueue<NetDataPackage>();

        private readonly AutoResetEvent _receivedDataResetEvent = new AutoResetEvent(false);

        protected NetConnection(INetConnectionAdapter connectionAdapter, IDataPackageHandler dataPackageHandler)
        {
            _connectionAdapter = connectionAdapter;
            _dataPackageDispatcher = new DataPackageDispatcher(dataPackageHandler);
        }

        public event Action<INetConnection> Disconnected;

        public void Open()
        {
            _connectionAdapter.Open(true);
        }

        public void Close()
        {
            _connectionAdapter.Close();
            OnDisconnected();
        }

        public bool IsConnected
        {
            get { return _connectionAdapter.IsConnected; }
        }

        public Uri Address
        {
            get { return _connectionAdapter.Address; }
        }

        protected void SendData(byte[] data)
        {
            var segmensts = new[] {new ArraySegment<byte>(data)};
            _connectionAdapter.WriteData(segmensts);
        }

        [ExcludeFromCodeCoverage]
        public void Run(bool withNewThread = false)
        {
            if (withNewThread)
            {
                new Thread(RunInternal) {IsBackground = true}.Start();
            }
            else
            {
                RunInternal();
            }
        }

        [ExcludeFromCodeCoverage]
        private void RunInternal()
        {
            new Thread(ProcessingReceivedDataQueueInternal) {IsBackground = true}.Start();
            ReadDataInternal();
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
                NetDataPackage package = null;
                if (!_receivedDataQueue.TryDequeue(out package))
                    break;
                if (package.IsEmpty) continue;

                _dataPackageDispatcher.Handle(package);
            }
        }

        protected void AddToReceivedDataQueue([NotNull] NetDataPackage buffer)
        {
            _receivedDataQueue.Enqueue(buffer);
            _receivedDataResetEvent.Set();
        }

        private void HandelReceivedData([NotNull] NetDataPackage package)
        {
            _dataPackageDispatcher.Handle(package);
            //if (!ThreadPool.QueueUserWorkItem(HandelReceivedDataCallBack, package))
            //    throw new Exception("Unexpected state, ThreadPool.QueueUserWorkItem() returns false!");
        }

        private void OnDisconnected()
        {
            var handler = Disconnected;
            if (handler != null) handler(this);
        }
    }

    public interface INetConnectionSettings
    {
        Uri Uri { get; }
    }
}