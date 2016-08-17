using Magic.Net;

namespace NUnit.Magic.Net.Test.Helper
{

    public class TestNetConnection : NetConnectionAbstract
    {
        public TestNetConnection(INetConnectionAdapter connectionAdapter, IDataPackageHandler netCommandHandler) 
            : base(connectionAdapter, netCommandHandler)
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
