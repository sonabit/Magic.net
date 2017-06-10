using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Magic.Net;

namespace NetConnectionSample
{
    class Program
    {
        static void Main(string[] args)
        {
            

        }
    }


    class NetNode
    {
        private readonly Uri _uri;
        private bool _isEnabled;
        private NetConnectionIpAdapter _connectionAdapter;

        public NetNode(Uri uri)
        {
            _uri = uri;
        }

        public bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }
            set
            {
                if (value)
                {
                    DoEnabled();
                }
                else
                {
                    DoDisable();
                }
            }
        }

        private void DoDisable()
        {
            throw new NotImplementedException();
        }

        private void DoEnabled()
        {
            if (_isEnabled) return;

            if (_uri.Port > 0)
                _connectionAdapter = new NetConnectionIpAdapter(_uri.Port);
            if (_connectionAdapter != null)
            {
                _connectionAdapter.Connect();
                return;
            }
            throw new NotSupportedException();
        }
    }

    class NetConnectionIpAdapter : INetConnectionAdapter
    {
        public NetConnectionIpAdapter(int port)
        {
            System.Net.Sockets.TcpListener listener = new TcpListener(new IPEndPoint(IPAddress.Any, port));
            listener.AcceptTcpClientAsync();
        }

        public bool IsConnected { get; }

        public void Connect()
        {
            throw new NotImplementedException();
        }

        public NetDataPackage ReadData()
        {
            throw new NotImplementedException();
        }

        public void WriteData(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}
