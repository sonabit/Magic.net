using System;
using Magic.Net;
using Magic.Net.Server;
using NUnit.Framework;

namespace NUnit.Magic.Net.Test
{
    [TestFixture]
    public class NetSystemHubTests
    {
        private NodeSystem _nodeSystem;

        [SetUp]
        public void InitTestMethod()
        {
            _nodeSystem = new NodeSystem("TestSystem");
        }

        [TearDown]
        public void CleanUpTestMethod()
        {
            _nodeSystem.Stop();
        }

        [Test, Category("Connection")]
        public void InitPipeServer()
        {
            // Given
            NamedPipeServerNetConnection pipeConnection = new NamedPipeServerNetConnection();

            // When
            pipeConnection.LinkTo(_nodeSystem);

            // Then
            Assert.AreEqual(false, pipeConnection.IsConnected);
        }

        [Test, Category("Connection")]
        public void InitSystem()
        {
            // Given
            NamedPipeServerNetConnection pipeConnection = new NamedPipeServerNetConnection();
            pipeConnection.LinkTo(_nodeSystem);

            // When
            _nodeSystem.Start();
            
            // Then
            Assert.AreEqual(true, pipeConnection.IsConnected); // Should be true becauce System starts is running
        }

        [Test, Category("Connection")]
        public void PipeServerCantNotOpen()
        {
            NamedPipeServerNetConnection pipeConnection = new NamedPipeServerNetConnection();
            pipeConnection.LinkTo(_nodeSystem);
            pipeConnection.Open();

            Assert.AreEqual(false, pipeConnection.IsConnected); // Should be false becauce System is not running
        }

        [Test, Category("Connection")]
        public void PipeServerOpenWithSystem()
        {
            // Given
            _nodeSystem.Start();
            NamedPipeServerNetConnection pipeConnection = new NamedPipeServerNetConnection();
            pipeConnection.LinkTo(_nodeSystem);
            
            // When
            _nodeSystem.Stop();

            // Then
            Assert.AreEqual(false, pipeConnection.IsConnected);
        }


        [Test, Category("Connection")]
        public void PipeServerLocalAddress()
        {
            // Given
            _nodeSystem.Start();
            NamedPipeServerNetConnection pipeConnection = new NamedPipeServerNetConnection();
            pipeConnection.LinkTo(_nodeSystem);

            // When
            Uri address = pipeConnection.LocalAddress;

            // Then
            Assert.AreEqual(string.Format("magic://{0}/{1}", Environment.MachineName, _nodeSystem.SystemName).ToLower(), address.ToString().ToLower());
        }

        [Test, Category("Connection")]
        public void PipeServerRemoteAddress()
        {
            // Given
            _nodeSystem.Start();
            NamedPipeServerNetConnection pipeConnection = new NamedPipeServerNetConnection();
            pipeConnection.LinkTo(_nodeSystem);

            // When Then
            Assert.Throws<NotSupportedException>(() =>
            {
                var pipeConnectionRemoteAddress = pipeConnection.RemoteAddress;
                Console.WriteLine(pipeConnectionRemoteAddress);
            });
        }

        [Test, Category("Connection")]
        public void PipeServerSendByteArray()
        {
            // Given
            _nodeSystem.Start();
            NamedPipeServerNetConnection pipeConnection = new NamedPipeServerNetConnection();
            pipeConnection.LinkTo(_nodeSystem);

            // When Then
            Assert.Throws<NotSupportedException>(() =>
            {
                 pipeConnection.Send(new byte[0][]);
            });
        }

        [Test, Category("Connection")]
        public void PipeServerSendArraySegment()
        {
            // Given
            _nodeSystem.Start();
            NamedPipeServerNetConnection pipeConnection = new NamedPipeServerNetConnection();
            pipeConnection.LinkTo(_nodeSystem);

            // When Then
            Assert.Throws<NotSupportedException>(() =>
            {
                pipeConnection.Send(new ArraySegment<byte>[0]);
            });
        }

        [Test, Category("Connection")]
        public void PipeServerGetTimeoutMilliseconds()
        {
            // Given
            _nodeSystem.Start();
            NamedPipeServerNetConnection pipeConnection = new NamedPipeServerNetConnection();
            pipeConnection.LinkTo(_nodeSystem);

            // When Then
            Assert.Throws<NotSupportedException>(() =>
            {
                var value = pipeConnection.TimeoutMilliseconds;
                Console.WriteLine(value);
            });
        }

        [Test, Category("Connection")]
        public void PipeServerGetDefaultSerializeFormat()
        {
            // Given
            _nodeSystem.Start();
            NamedPipeServerNetConnection pipeConnection = new NamedPipeServerNetConnection();
            pipeConnection.LinkTo(_nodeSystem);

            // When 
            DataSerializeFormat value = pipeConnection.DefaultSerializeFormat;

            // Then
            Assert.AreEqual(DataSerializeFormat.MsBinary, value);
        }
    }
}