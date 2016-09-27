using Magic.Net;

namespace NUnit.Magic.Net.Test.Helper
{

    public class TestNetConnection : NetConnection
    {
        public TestNetConnection(INetConnectionAdapter connectionAdapter, IDataPackageHandler dataPackageHandler) 
            : base(connectionAdapter, dataPackageHandler)
        {
        }
        
        public void AddAddToReceivedDataQueue(NetDataPackage package)
        {
            AddToReceivedDataQueue(package);
        }

        public void CallDequeueReceivedData()
        {
            DequeueReceivedData();
        }
    }
}
