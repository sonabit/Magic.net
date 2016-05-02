using System;
using System.Runtime.CompilerServices;
using Magic.Net;

namespace NUnit.Magic.Net.Test.Helper
{
    
    // ReSharper disable once ClassNeverInstantiated.Global
    public class TestNetConnection : NetConnectionAbstract
    {
        public TestNetConnection(INetConnectionAdapter connectionAdapter, IDataPackageHandler netCommandHandler) 
            : base(connectionAdapter, netCommandHandler)
        {
        }
        
        
        public void AddAddToReceivedDataQueue(NetDataPackage package)
        {
            base.AddToReceivedDataQueue(package);
        }

        public void CallDequeueReceivedData()
        {
            base.DequeueReceivedData();
        }
    }
}
