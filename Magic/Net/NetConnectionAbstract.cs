using System;
using System.Collections.Concurrent;
using System.Threading;
using JetBrains.Annotations;

namespace Magic.Net
{
    public class NetConnectionAbstract : INetConnection
    {
        private readonly INetConnectionAdapter _connectionAdapter;
        private readonly INetConnectionPackageManager _packageManager;

        private readonly ConcurrentQueue<NetCommandPackage> _receivedDataQueue =
            new ConcurrentQueue<NetCommandPackage>();

        private readonly AutoResetEvent _receivedDataResetEvent = new AutoResetEvent(false);

        protected NetConnectionAbstract(INetConnectionAdapter connectionAdapter,
            INetConnectionPackageManager packageManager)
        {
            _connectionAdapter = connectionAdapter;
            _packageManager = packageManager;
        }

        public void Connect()
        {
            _connectionAdapter.Connected();
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
                    NetCommandPackage package = _connectionAdapter.ReadData();
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
                NetCommandPackage package;
                if (!_receivedDataQueue.TryDequeue(out package))
                    break;
                if (package.IsEmpty) continue;

                switch (package.Version)
                {
                    case 1:
                        // accept Version 1
                        break;
                    default:
                        throw new NotSupportedException(
                            string.Format("NetCommandPackage version {0} not supported!", package.Version));
                }

                switch (package.PackageContenttyp)
                {
                    case DataPackageContenttyp.NetCommand:
                        HandelReceivedData(package);
                        break;
                    default:
                        // this case should never happened
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        protected void AddToReceivedDataQueue([NotNull] NetCommandPackage buffer)
        {
            _receivedDataQueue.Enqueue(buffer);
            _receivedDataResetEvent.Set();
        }

        private void HandelReceivedData([NotNull] NetCommandPackage package)
        {
            _packageManager.ReceivedData(package);
            //if (!ThreadPool.QueueUserWorkItem(HandelReceivedDataCallBack, package))
            //    throw new Exception("Unexpected state, ThreadPool.QueueUserWorkItem() returns false!");
        }
    }

    public interface INetConnectionPackageManager
    {
        void ReceivedData(NetCommandPackage package);
    }

    public interface INetConnectionAdapter
    {
        bool IsConnected { get; }

        void Connected();
        NetCommandPackage ReadData();
        void WriteData(byte[] data);
    }

    /// <summary>
    ///     Specifies a data package of NetConnection
    /// </summary>
    public enum DataPackageContenttyp : byte
    {
        NetCommand = 0x1,
        NetCommandStream = 0x10
    }
}