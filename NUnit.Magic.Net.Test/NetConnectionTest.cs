using System;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using FakeItEasy.Creation;
using Magic.Net;
using NUnit.Framework;
using NUnit.Magic.Net.Test.Helper;

namespace NUnit.Magic.Net.Test
{
    [TestFixture]
    public class NetConnectionTest
    {
        [Test]
        public void NetConnectionOnReceivedDataOkTest()
        {
            TestNetConnection connection = A.Fake<TestNetConnection>(o => o.CallsBaseMethods());
            NetCommandPackage buffer = new NetCommandPackage(new byte[] { 1, 1, 0, 0, 0, });
            connection.AddAddToReceivedDataQueue(buffer);
            
            A.CallTo(() => connection.IsConnected).Returns(true);

            connection.CallDequeueReceivedData();

            A.CallTo(() => connection.OnReceivedData(buffer)).MustHaveHappened(Repeated.AtLeast.Once);
        }

        [Test]
        public void When_Version_Is_not_1()
        {
            TestNetConnection connection = A.Fake<TestNetConnection>(o => o.CallsBaseMethods());
            NetCommandPackage buffer = new NetCommandPackage(new byte[] { 20, 1, 0, 0, 0, });
            connection.AddAddToReceivedDataQueue(buffer);

            Assert.Throws<NotSupportedException>(connection.CallDequeueReceivedData);

        }

    }

    [TestFixture]
    public class NetCommandPackageTests
    {
        [Test]
        public void Create_NetCommandPackage()
        {
            
        }
    }
}
