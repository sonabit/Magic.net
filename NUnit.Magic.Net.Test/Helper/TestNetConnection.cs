using System;
using System.Runtime.CompilerServices;
using Magic.Net;

namespace NUnit.Magic.Net.Test.Helper
{
    
    // ReSharper disable once ClassNeverInstantiated.Global
    public class TestNetConnection : NetConnectionAbstract
    {
        public TestNetConnection(INetConnectionAdapter connectionAdapter, INetConnectionPackageManager packageManager) 
            : base(connectionAdapter, packageManager)
        {
        }

        public virtual NetCommandPackage WrapedReadData()
        {
             throw new NotImplementedException();
        }

        public virtual void WrapedWriteData(byte[] data)
        {
            SendData(data);
        }
        
        public void AddAddToReceivedDataQueue(NetCommandPackage package)
        {
            base.AddToReceivedDataQueue(package);
        }

        public void CallDequeueReceivedData()
        {
            base.DequeueReceivedData();
        }
    }
}
