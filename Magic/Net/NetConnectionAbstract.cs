using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Magic.Net
{
    public abstract class NetConnectionAbstract : INetConnection
    {
        private readonly ConcurrentQueue<byte[]> _receivedDataQueue = new ConcurrentQueue<byte[]>();
        private readonly AutoResetEvent _receivedDataResetEvent = new AutoResetEvent(false);

        public abstract void Connect();

        public abstract bool IsConnected { get; }
        
        protected abstract byte[] ReadData();

        protected abstract void SendData(byte[] data);

        public void Run(bool withNewThread = false)
        {

            if (withNewThread)
            {
                (new Thread(RunInternal)).Start();
            }
            else
            {
                RunInternal();
            }
        }

        private void RunInternal()
        {
            ReadDataInternal();
        }

        private void ReadDataInternal()
        {
            while (IsConnected)
            {
                try
                {
                    byte[] buffer = ReadData();
                    if (buffer != null && buffer.Length > 0)
                        AddToReceivedDataQueue(buffer);
                }
                catch (Exception)
                {
                    if (!IsConnected)
                        break;
                }
            }
        }

        private void AddToReceivedDataQueue(byte[] buffer)
        {
            _receivedDataQueue.Enqueue(buffer);
            _receivedDataResetEvent.Set();
        }
    }
}
