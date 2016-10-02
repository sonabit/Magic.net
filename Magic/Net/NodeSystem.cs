using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceModel.Channels;
using JetBrains.Annotations;
using Magic.Serialization;

namespace Magic.Net
{
    public sealed class NodeSystem : ISystem
    {
        private readonly BufferManager _bufferManager = BufferManager.CreateBufferManager(300*1024*1024, 2*1024*1024);

        [NotNull, ItemNotNull] private readonly List<INetConnection> _connections = new List<INetConnection>();
        private readonly ISerializeFormatterCollection _formatterCollection = new DefaulSerializeFormatter();

        private readonly DataPackageHandler _packageHandler;

        public NodeSystem()
        {
            //
        }

        public NodeSystem(IServiceProvider serviceProvider)
        {
            _packageHandler = new DataPackageHandler(serviceProvider, _formatterCollection);
        }

        public void AddConnection([NotNull] INetConnection connection)
        {
            lock (_connections)
            {
                _connections.Add(connection);
            }
        }

        public void Start()
        {
            lock (_connections)
            {
                foreach (var netConnection in _connections)
                {
                    if (!netConnection.IsConnected)
                    {
                        netConnection.Open();
                    }
                }
            }
        }

        public void Stop()
        {
            lock (_connections)
            {
                foreach (var netConnection in _connections.ToArray())
                {
                    netConnection.Close();
                    var disposable = netConnection as IDisposable;
                    if (disposable != null)
                        disposable.Dispose();
                }
            }
        }

        public TResult Exc<TResult>(Uri remoteAddress, Expression<Func<TResult>> expression)
        {
            var connection = FindConnection(remoteAddress);

            if (connection == null)
                throw new Exception();

            var command = expression.ToNetCommand();

            return Execute<TResult>(connection, command);
        }

        public TResult Exc2<TRemoteType, TResult>(Uri remoteAddress, Expression<Func<TRemoteType, TResult>> expression)
        {
            var connection = FindConnection(remoteAddress);

            var command = expression.ToNetCommand();

            return default(TResult);
        }

        private INetConnection FindConnection(Uri remoteAddress)
        {
            lock (_connections)
            {
                return _connections.FirstOrDefault(c => c.RemoteAddress.Host == remoteAddress.Host &&
                                                        c.RemoteAddress.GetStringOfSegment(1) ==
                                                        remoteAddress.GetStringOfSegment(1));
            }
        }

        private TResult Execute<TResult>(INetConnection connection, INetCommand command)
        {
            NetDataPackageHeader header = null;
            byte[] package = null;
            var magicSerialization = command as IMagicSerialization;
            if (magicSerialization != null)
            {
                header = new NetDataPackageHeader(1, DataPackageContentType.NetCommand, DataSerializeFormat.Magic);
                package = magicSerialization.ToBytes();
            }
            else
            {
                header = new NetDataPackageHeader(1, DataPackageContentType.NetCommand, DataSerializeFormat.MsBinary);
                ISerializeFormatter s = _formatterCollection[DataSerializeFormat.MsBinary];
                package = s.Serialize(command);
            }
            connection.Send(new[] { header.ToBytes(), package});

            return default(TResult);
        }

        #region Implementation of ISystem

        public BufferManager BufferManager
        {
            get { return _bufferManager; }
        }

        public IDataPackageHandler PackageHandler
        {
            get { return _packageHandler; }
        }

        #endregion
    }

    public static class Proxy<TTarget>
    {
        public static TTarget Target { get { return default(TTarget); } }
    }
}