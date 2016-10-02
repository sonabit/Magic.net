using System;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization.Formatters.Binary;
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
        [Test, Category("receive data package")]
        public void NetConnection_ReceivedQueue_OnReceivedDataPackage_Ok()
        {
            IDataPackageHandler dataPackageHandler = A.Fake<IDataPackageHandler>();
            INetConnectionAdapter adapter = A.Fake<INetConnectionAdapter>();
            TestNetConnection connection = new TestNetConnection(adapter, dataPackageHandler);
            A.CallTo(() => adapter.IsConnected).Returns(true);

            byte[] buffer;
            var bformatter = new BinaryFormatter();
            using (var stream = new MemoryStream())
            using (var bw = new BinaryWriter(stream))
            {
                bw.Write((byte)1);
                bw.Write((byte)1);
                bw.Write((byte)DataSerializeFormat.MsBinary);

                bformatter.Serialize(stream, new NetCommand(this.GetType(), null, new ParameterInfo[0], new object[0]));
                buffer = stream.ToArray();
            }
            
            var package = new NetDataPackage(buffer);
            connection.AddAddToReceivedDataQueue(package);

            connection.CallDequeueReceivedData();

            A.CallTo(() => dataPackageHandler.ReceiveCommand(A<NetDataPackage>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test, Category("data package version")]
        public void When_Version_Is_not_1()
        {
            var connection = A.Fake<TestNetConnection>(o => o.CallsBaseMethods());
            var buffer = new NetDataPackage(new byte[] { 20, 1, 0, 0, 0 });
            connection.AddAddToReceivedDataQueue(buffer);

            Assert.Throws<NotSupportedException>(connection.CallDequeueReceivedData);
        }
    }
}