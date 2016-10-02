using System;
using System.Collections.Generic;

namespace Magic.Net
{
    public interface INetConnectionAdapter : IDisposable
    {
        bool IsConnected { get; }
        Uri RemoteAddress { get; }

        //void Open(bool withOwnThread);

        void Close();
        
        NetDataPackage ReadData();

        void WriteData(params ArraySegment<byte>[] buffers);

        void Open();
    }
    
}