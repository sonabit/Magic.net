using System.ServiceModel.Channels;
using JetBrains.Annotations;
using Magic.Net;

namespace NUnit.Magic.Net.Test.Helper
{

    public class TestNetConnection : NetConnection
    {
        private readonly INetConnectionAdapter _connectionAdapter;
        private static readonly BufferManager _bufferManager;

        static TestNetConnection()
        {
            _bufferManager = BufferManager.CreateBufferManager(500 * 1024 *1024, 10 * 1024 *1024);
        }
        public TestNetConnection([NotNull]INetConnectionAdapter connectionAdapter) 
            : base()
        {
            _connectionAdapter = connectionAdapter;
        }

        internal void AddAddToReceivedDataQueue(NetDataPackage package)
        {
            AddToReceivedDataQueue(package);
        }

        public void CallDequeueReceivedData()
        {
            DequeueReceivedData();
        }

        #region Overrides of NetConnection

        protected override INetConnectionAdapter CreateAdapter(ISystem system)
        {
            return _connectionAdapter;
        }

        #endregion
    }
}
