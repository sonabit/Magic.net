using System.ServiceModel.Channels;
using Magic.Net;

namespace NUnit.Magic.Net.Test.Helper
{

    public class TestNetConnection : NetConnection
    {
        private static BufferManager _bufferManager;

        static TestNetConnection()
        {
            _bufferManager = BufferManager.CreateBufferManager(500 * 1024 *1024, 10 * 1024 *1024);
        }
        public TestNetConnection(INetConnectionAdapter connectionAdapter, IDataPackageHandler dataPackageHandler) 
            : base(connectionAdapter, dataPackageHandler, _bufferManager)
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
