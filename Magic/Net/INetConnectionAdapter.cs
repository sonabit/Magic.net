using System;
using System.Collections.Generic;
using System.Text;
using Magic.Net.Data;

namespace Magic.Net
{
    public interface INetConnectionAdapter : IDisposable
    {
        Encoding Encoding { get; }

        bool IsConnected { get; }

        Uri RemoteAddress { get; }

        Uri LocalAddress { get; }

        void Open();

        void Close();
        
        NetPackage ReadData();

        void WriteData(params ArraySegment<byte>[] buffers);
    }
    
}