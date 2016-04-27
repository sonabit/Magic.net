using System;
using FakeItEasy;
using Magic.Net;
using NUnit.Framework;
using NUnit.Magic.Net.Test.Helper;

namespace NUnit.Magic.Net.Test
{
    [TestFixture]
    public class NetConnectionTest
    {
        [Test]
        public void NetConnection_ReceivedQueue_OnReceivedDataPackage_Ok()
        {
            IDataPackageHandler netCommandHandler = A.Fake<IDataPackageHandler>();
            TestNetConnection connection =
                A.Fake<TestNetConnection>(
                    o => o.WithArgumentsForConstructor(new object[] {netCommandHandler}).CallsBaseMethods());

            var package = new NetDataPackage(new byte[] {1, 1, 0, 0, 0});
            connection.AddAddToReceivedDataQueue(package);

            A.CallTo(() => connection.IsConnected).Returns(true);

            connection.CallDequeueReceivedData();

            A.CallTo(() => netCommandHandler.Handel(package)).MustHaveHappened(Repeated.AtLeast.Once);
        }

        [Test]
        public void When_DataPackage_Type_Is_unknown()
        {
            var connection = A.Fake<TestNetConnection>(o => o.CallsBaseMethods());
            var buffer = new NetDataPackage(new byte[] {1, 255, 0, 0, 0});
            connection.AddAddToReceivedDataQueue(buffer);

            var exception = Assert.Throws<NetCommandException>(connection.CallDequeueReceivedData);
            Assert.NotNull(exception);
            Assert.AreEqual("package.PackageContentType 255 unknown.", exception.Message);
            Assert.AreEqual(NetCommandExceptionReasonses.UnknownPackageContentType, exception.Reasonses);
        }

        [Test]
        public void When_Version_Is_not_1()
        {
            var connection = A.Fake<TestNetConnection>(o => o.CallsBaseMethods());
            var buffer = new NetDataPackage(new byte[] {20, 1, 0, 0, 0});
            connection.AddAddToReceivedDataQueue(buffer);

            Assert.Throws<NotSupportedException>(connection.CallDequeueReceivedData);
        }
    }
}