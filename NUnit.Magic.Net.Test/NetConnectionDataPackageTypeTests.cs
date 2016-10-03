using FakeItEasy;
using Magic.Net;
using NUnit.Framework;
using NUnit.Magic.Net.Test.Helper;

namespace NUnit.Magic.Net.Test
{
    public class NetConnectionDataPackageTypeTests
    {
        private TestNetConnection _connection;
        private IDataPackageHandler _dataPackageHandler;

        [SetUp]
        public void Init()
        {
            _dataPackageHandler = A.Fake<IDataPackageHandler>();
            INetConnectionAdapter adapter = A.Fake<INetConnectionAdapter>();
            _connection = new TestNetConnection(adapter, _dataPackageHandler);
            A.CallTo(() => adapter.IsConnected).Returns(true);
        }

        [Test, Category("package type")]
        public void When_DataPackage_Type_Is_unknown()
        {
            var buffer = new NetDataPackage(new byte[] { 1, 255, 0, 0, 0 });
            _connection.AddAddToReceivedDataQueue(buffer);
            var exception = Assert.Throws<NetCommandException>(_connection.CallDequeueReceivedData);

            Assert.NotNull(exception);
            Assert.AreEqual("PackageContentType 255 unknown.", exception.Message);
            Assert.AreEqual(NetCommandExceptionReasonses.UnknownPackageContentType, exception.Reasonses);
        }

        [Test, Category("package type")]
        public void When_DataPackage_Type_Is_1()
        {
            var package = new NetDataPackage(new byte[] { 1, 1, 0, 0, 0 });
            _connection.AddAddToReceivedDataQueue(package);
            _connection.CallDequeueReceivedData();
            A.CallTo(() => _dataPackageHandler.ReceiveCommand(A<RequestState>.Ignored)).MustHaveHappened(Repeated.AtLeast.Once);
        }

        [Test, Category("package type")]
        public void When_DataPackage_Type_Is_10()
        {
            var package = new NetDataPackage(new byte[] { 1, 10, 0, 0, 0 });
            _connection.AddAddToReceivedDataQueue(package);
            _connection.CallDequeueReceivedData();

            A.CallTo(() => _dataPackageHandler.ReceiveCommandStream(A<RequestState>.Ignored)).MustHaveHappened(Repeated.AtLeast.Once);
        }
    }
}