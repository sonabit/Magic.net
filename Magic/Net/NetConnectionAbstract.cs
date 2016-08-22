using System;
using System.Collections.Concurrent;
using System.Threading;
using JetBrains.Annotations;

namespace Magic.Net
{
    public class NetConnectionAbstract : INetConnection
    {
        private readonly ConcurrentQueue<NetDataPackage> _receivedDataQueue = new ConcurrentQueue<NetDataPackage>();
        private readonly INetConnectionAdapter _connectionAdapter;

        private readonly AutoResetEvent _receivedDataResetEvent = new AutoResetEvent(false);
        private readonly IDataPackageDispatcher _dataPackageDispatcher;

        protected NetConnectionAbstract(INetConnectionAdapter connectionAdapter, IDataPackageHandler netCommandHandler)
        {
            _connectionAdapter = connectionAdapter;
            _dataPackageDispatcher = new DataPackageDispatcher(netCommandHandler);
        }

        public void Connect()
        {
            _connectionAdapter.Connect();
        }

        public bool IsConnected
        {
            get { return _connectionAdapter.IsConnected; }
        }

        protected void SendData(byte[] data)
        {
            _connectionAdapter.WriteData(data);
        }

        public void Run(bool withNewThread = false)
        {
            if (withNewThread)
            {
                new Thread(RunInternal).Start();
            }
            else
            {
                RunInternal();
            }
        }

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
                    NetDataPackage package = _connectionAdapter.ReadData();
                    if (package != null && package.Buffer.Length > 0)
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
    }

    public interface INetConnectionAdapter
    {
        bool IsConnected { get; }

        void Connect();
        NetDataPackage ReadData();
        void WriteData(byte[] data);
    }
}