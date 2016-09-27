using System;
using System.Collections.Generic;

namespace Magic.Net
{
    public interface INetConnectionAdapter : IDisposable
    {
        bool IsConnected { get; }
        Uri Address { get; }

        void Open(bool withOwnThread);

        void Close();
        
        NetDataPackage ReadData();

        void WriteData(IEnumerable<ArraySegment<byte>> buffers);
    }
    
}