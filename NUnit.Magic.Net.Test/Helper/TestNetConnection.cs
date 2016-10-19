using JetBrains.Annotations;
using Magic.Net;

namespace NUnit.Magic.Net.Test.Helper
{
    public class TestNetConnection : NetConnection
    {
        private readonly INetConnectionAdapter _connectionAdapter;

        public TestNetConnection([NotNull] INetConnectionAdapter connectionAdapter)
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