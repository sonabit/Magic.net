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
            INetConnectionPackageManager packageManager = A.Fake<INetConnectionPackageManager>();
            INetConnectionAdapter adapter = A.Fake<INetConnectionAdapter>();
            TestNetConnection connection = new TestNetConnection(adapter, packageManager);
            NetCommandPackage buffer = new NetCommandPackage(new byte[] { 1, 1, 0, 0, 0, });
            connection.AddAddToReceivedDataQueue(buffer);
            connection.CallDequeueReceivedData();

            A.CallTo(() => packageManager.ReceivedData(buffer)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void When_Version_Is_Not_1()
        {
            INetConnectionPackageManager packageManager = A.Fake<INetConnectionPackageManager>();
            INetConnectionAdapter adapter = A.Fake<INetConnectionAdapter>();
            TestNetConnection connection = new TestNetConnection(adapter, packageManager);  
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
