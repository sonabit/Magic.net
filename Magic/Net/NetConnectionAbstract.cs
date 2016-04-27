using System;
using System.Collections.Concurrent;
using System.Threading;
using JetBrains.Annotations;

namespace Magic.Net
{
    public abstract class NetConnectionAbstract : INetConnection
    {
        private readonly ConcurrentQueue<NetDataPackage> _receivedDataQueue =
            new ConcurrentQueue<NetDataPackage>();

        private readonly AutoResetEvent _receivedDataResetEvent = new AutoResetEvent(false);
        private readonly IDataPackageDispatcher _dataPackageDispatcher;

        protected NetConnectionAbstract(IDataPackageHandler netCommandHandler)
        {
            _dataPackageDispatcher = new DataPackageDispatcher(netCommandHandler);
        }

        public abstract void Connect();

        public abstract bool IsConnected { get; }

        protected abstract NetDataPackage ReadData();

        protected abstract void SendData(byte[] data);

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
            while (IsConnected)
            {
                try
                {
                    var package = ReadData();
                    if (package != null && package.Buffer.Length > 0)
                        AddToReceivedDataQueue(package);
                }
                catch (Exception)
                {
                    if (!IsConnected)
                        break;
                }
            }
        }

        private void ProcessingReceivedDataQueueInternal()
        {
            while (IsConnected)
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
        
    }
}