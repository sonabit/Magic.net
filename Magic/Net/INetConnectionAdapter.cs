using System;
using System.Collections.Generic;
using System.Text;

namespace Magic.Net
{
    public interface INetConnectionAdapter : IDisposable
    {
        Encoding Encoding { get; }

        bool IsConnected { get; }
        Uri RemoteAddress { get; }

        //void Open(bool withOwnThread);

        void Close();
        
        NetDataPackage ReadData();

        void WriteData(params ArraySegment<byte>[] buffers);

        void Open();
    }
    
}