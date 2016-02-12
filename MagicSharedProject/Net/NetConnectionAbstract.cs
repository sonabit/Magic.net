using System;
using System.Collections.Concurrent;
using System.Threading;
using JetBrains.Annotations;

namespace Magic.Net
{
    public abstract class NetConnectionAbstract : INetConnection
    {
        private readonly ConcurrentQueue<NetCommandPackage> _receivedDataQueue =
            new ConcurrentQueue<NetCommandPackage>();

        private readonly AutoResetEvent _receivedDataResetEvent = new AutoResetEvent(false);

        public abstract void Connect();

        public abstract bool IsConnected { get; }

        protected abstract NetCommandPackage ReadData();

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
                    while (!_receivedDataQueue.IsEmpty)
                    {
                        NetCommandPackage package = null;
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

        protected void AddToReceivedDataQueue([NotNull] NetCommandPackage buffer)
        {
            _receivedDataQueue.Enqueue(buffer);
            _receivedDataResetEvent.Set();
        }

        private void HandelReceivedData([NotNull] NetCommandPackage package)
        {
            if (!ThreadPool.QueueUserWorkItem(HandelReceivedDataCallBack, package))
                throw new Exception("mööp");
        }

        private void HandelReceivedDataCallBack([NotNull] object package)
        {
            Console.WriteLine("HandelReceivedDataCallBack");
            OnReceivedData((NetCommandPackage) package);
        }

        public virtual void OnReceivedData([NotNull] NetCommandPackage buffer)
        {
        }
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