using System;

namespace Magic.Net
{
    public interface INetConnection
    {
        bool IsConnected { get; }

        Uri RemoteAddress { get; }

        Uri LocalAddress { get; }

        int TimeoutMilliseconds{ get; }
        DataSerializeFormat DefaultSerializeFormat { get; }

        void Open();

        void Close();
        
        event Action<INetConnection> Disconnected;
        
        void Send(params byte[][] bytes);

        void Send(params ArraySegment<byte>[] segments);
    }
}