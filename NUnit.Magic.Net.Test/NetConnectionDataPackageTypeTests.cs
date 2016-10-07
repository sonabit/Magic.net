using System;
using FakeItEasy;
using Magic.Net;
using Magic.Net.Data;
using Magic.Serialization;
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
            ISerializeFormatterCollection formatterCollection = A.Fake<ISerializeFormatterCollection>();

            ISerializeFormatter fakeMagicFormatter = A.Fake<ISerializeFormatter>();
            A.CallTo(() => fakeMagicFormatter.Deserialize(A<byte[]>.Ignored, A<Type>.That.Matches(t => t == typeof(NetCommand)), A<long>.Ignored))
                .Returns(new NetCommand());
            A.CallTo(() => fakeMagicFormatter.Deserialize(A<byte[]>.Ignored, A<Type>.That.Matches(t => t == typeof(NetObjectStreamInitializeRequest)), A<long>.Ignored))
                .Returns(new NetObjectStreamInitializeRequest());

            A.CallTo(() => formatterCollection.GetFormatter(DataSerializeFormat.Magic)).Returns(fakeMagicFormatter);


            ISystem fakeSystem = new NodeSystem("UnitTestSystem", new ServiceCollection(), formatterCollection);
            
            _connection = new TestNetConnection(adapter, _dataPackageHandler, fakeSystem);
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
        public void When_DataPackage_Type_Is_NetCommandResult()
        {
            var package = new NetDataPackage(new byte[] { 1, 1, 0, 0, 0 });
            _connection.AddAddToReceivedDataQueue(package);
            _connection.CallDequeueReceivedData();
            A.CallTo(() => _dataPackageHandler.ReceiveCommand(A<RequestState>.That.Matches(state => state.Package.Data is NetCommand))).MustHaveHappened(Repeated.AtLeast.Once);
        }

        [Test, Category("package type")]
        public void When_DataPackage_Type_Is_NetObjectStreamInitialize()
        {
            var package = new NetDataPackage(new byte[] { 1, 10, 0, 0, 0 });
            _connection.AddAddToReceivedDataQueue(package);
            _connection.CallDequeueReceivedData();

            A.CallTo(() => _dataPackageHandler.ReceiveCommandStream(A<RequestState>.That.Matches(state => state.Package.Data is NetObjectStreamInitializeRequest))).MustHaveHappened(Repeated.AtLeast.Once);
        }
    }
}