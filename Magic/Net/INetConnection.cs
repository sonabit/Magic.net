using System;

namespace Magic.Net
{
    public interface INetConnection
    {
        bool IsConnected { get; }

        Uri Address { get; }

        void Open();

        void Close();


        event Action<INetConnection> Disconnected;
    }
}