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
            NetCommandPackage buffer = new NetCommandPackage(new byte[] { 1, 0, 0, 0, 0, });
            connection.AddAddToReceivedDataQueue(buffer);
            
            A.CallTo(() => connection.IsConnected).Returns(true);

            // Abort after 300 Milliseconds
            Task.Delay(TimeSpan.FromMilliseconds(600))
                .ContinueWith(t => A.CallTo(() => connection.IsConnected).Returns(false));

            connection.Run();
            Thread.Sleep(3000);

            A.CallTo(() => connection.OnReceivedData(buffer)).MustHaveHappened(Repeated.AtLeast.Once);
        }
        
    }
}
