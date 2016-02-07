using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FakeItEasy;
using FakeItEasy.ExtensionSyntax.Full;
using Magic.Net;
using NUnit.Framework;
using NUnit.Magic.Net.Test.Helper;

namespace NUnit.Magic.Net.Test
{
    [TestFixture]
    public class NetConnectionTest
    {
        [Test]
        public void NetConnectionAbstractClassTest()
        {
            var connection = A.Fake<TestNetConnection>();

            A.CallTo(() => connection.WrapedReadData()).Returns(new byte[] { 0, 0, 0, 0, 0, });
            A.CallTo(() => connection.IsConnected).Returns(true);

            // Abort after 300 Milliseconds
            Task.Delay(TimeSpan.FromMilliseconds(300))
                .ContinueWith(t => A.CallTo(() => connection.IsConnected).Returns(false));

            connection.Run();


        }
    }
}
