using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Magic.Net;
using Magic.Net.Data;
using Magic.Serialization;
using NUnit.Framework;
using NUnit.Magic.Net.Test.Helper;

namespace NUnit.Magic.Net.Test
{
    [TestFixture]
    public class HubTests
    {
        [SetUp]
        public void Init()
        {
            _hub = new NodeSystem();
            _hub.Start();
        }

        [TearDown]
        public void Dispose()
        {
            _tokenSource.Cancel(false);
            _hub.Stop();
        }

        private NodeSystem _hub;
        private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();


        [Test]
        public void ReceiveFromOjectStreamTest()
        {
            //Given 
            Guid streamId = Guid.NewGuid();
            TestStreamAdapter adapter = new TestStreamAdapter(
                new Uri("magic://localtest/testhub"),
                new Uri("magic://remotemachine/testhub"),
                Stream.Null,
                _tokenSource.Token.WaitHandle
            );

            adapter.OnWriteData += delegate(TestStreamAdapter a, byte[] bytes)
            {
                NetDataPackageHeader header = new NetDataPackageHeader(ref bytes);
                switch (header.PackageContentType)
                {
                    case DataPackageContentType.NetCommand:
                        NetDataPackage package = new NetDataPackage(bytes);
                        ISerializeFormatter serializeFormatter = new GFormatter<BinaryFormatter>();
                        NetCommand command = serializeFormatter.Deserialize<NetCommand>(package.Buffer.Array,
                            package.Buffer.Offset);
                        Guid commandId = command.Id;
                        a.AddNextReadPackages(new NetPackage[]
                        {
                            new NetOjectPackage(
                                NetDataPackageHeader.CreateNetDataPackageHeader(
                                    DataPackageContentType.NetCommandResult, DataSerializeFormat.MsBinary),
                                new NetCommandResult(commandId,
                                    new RemoteObjectStream<object>(
                                        new Uri("magic://remotemachine/testhub/" + streamId, UriKind.Absolute)))),
                            new NetOjectPackage(
                                NetDataPackageHeader.CreateNetDataPackageHeader(
                                    DataPackageContentType.NetObjectStreamData, DataSerializeFormat.MsBinary),
                                new NetObjectStreamData(streamId, new object())),
                            new NetOjectPackage(
                                NetDataPackageHeader.CreateNetDataPackageHeader(
                                    DataPackageContentType.NetObjectStreamData, DataSerializeFormat.MsBinary),
                                new NetObjectStreamData(streamId, new object()))
                        });
                        break;
                }
            };

            INetConnection connection = new TestNetConnection(adapter);

            connection.LinkTo(_hub);

            // When
            var test = _hub.CreateObjectStream<object>(new Uri("magic://remotemachine/testhub"),
                TimeSpan.FromSeconds(10));

            var testResult = new object[10];
            using (test)
            {
                var count = 0;
                while (test.MoveNext())
                {
                    testResult[count] = test.Current;
                    count++;
                }
                Array.Resize(ref testResult, count);
            }


            //Then
            Assert.AreEqual(2, testResult.Length);
        }
    }
}