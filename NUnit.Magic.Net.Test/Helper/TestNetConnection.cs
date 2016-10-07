using System.ServiceModel.Channels;
using JetBrains.Annotations;
using Magic.Net;

namespace NUnit.Magic.Net.Test.Helper
{

    internal class TestNetConnection : NetConnection
    {
        private static readonly BufferManager _bufferManager;

        static TestNetConnection()
        {
            _bufferManager = BufferManager.CreateBufferManager(500 * 1024 *1024, 10 * 1024 *1024);
        }
        public TestNetConnection([NotNull]INetConnectionAdapter connectionAdapter, [NotNull]IDataPackageHandler dataPackageHandler, [NotNull]ISystem system) 
            : base(connectionAdapter, system, dataPackageHandler, _bufferManager)
        {
        }
        
        internal void AddAddToReceivedDataQueue(NetDataPackage package)
        {
            AddToReceivedDataQueue(package);
        }

        public void CallDequeueReceivedData()
        {
            DequeueReceivedData();
        }
    }
}
