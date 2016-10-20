using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Magic.Net;
using Magic.Net.Data;
using Magic.Net.Server;
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
            _remoteUri = new Uri("magic://" + Environment.MachineName + "/TargetSystem");
            _serverSystem = new NodeSystem("TargetSystem");
            NamedPipeServerNetConnection namedPipeServerNetConnection = new NamedPipeServerNetConnection();
            namedPipeServerNetConnection.LinkTo(_serverSystem);
            _serverSystem.Start();

            _clientSystem = new NodeSystem("ClientSystem");
            NamedPipeNetConnection pipeClient = new NamedPipeNetConnection(_remoteUri); 
            pipeClient.LinkTo(_clientSystem);
            _clientSystem.Start();
        }

        [TearDown]
        public void Dispose()
        {
            _tokenSource.Cancel(false);
            _clientSystem.Stop();
            _serverSystem.Stop();
        }

        private NodeSystem _clientSystem;
        private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();
        private NodeSystem _serverSystem;
        private Uri _remoteUri;

        [Test]
        public void HubTests_EnvironmentCheck()
        {
            Uri[] serverAddresses = _serverSystem.LocalAddresses().ToArray();
            Assert.AreEqual(1, serverAddresses.Length);
            Assert.AreEqual(_remoteUri.ToString().ToLower(), serverAddresses[0].ToString().ToLower());
        }


        [Test]
        public void ReceiveFromOjectStreamTest()
        {
            Assert.Inconclusive("not stable");
            //Given 
            var streamId = Guid.NewGuid();
            var adapter = new TestStreamAdapter(
                new Uri("magic://localtest/testhub"),
                new Uri("magic://remotemachine/testhub"),
                Stream.Null,
                _tokenSource.Token.WaitHandle
            );

            adapter.OnWriteData += delegate(TestStreamAdapter a, byte[] bytes)
            {
                var header = new NetDataPackageHeader(ref bytes);
                switch (header.PackageContentType)
                {
                    case DataPackageContentType.NetCommand:
                        var package = new NetDataPackage(bytes);
                        ISerializeFormatter serializeFormatter = new GFormatter<BinaryFormatter>();
                        var command = serializeFormatter.Deserialize<NetCommand>(package.Buffer.Array,
                            package.Buffer.Offset);
                        var commandId = command.Id;
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

            connection.LinkTo(_clientSystem);

            // When
            IEnumerator<object> test = _clientSystem.CreateObjectStream<object>(new Uri("magic://remotemachine/testhub"),
                TimeSpan.FromSeconds(20));

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

        [Test]
        public void ReceiveFromOjectStreamTest2()
        {
            Assert.Inconclusive("not stable");
            // Given // When
            IEnumerator<object> objects = _clientSystem.CreateObjectStream<object>(_remoteUri);


            // Thewn
            Assert.NotNull(objects);
        }
    }
}