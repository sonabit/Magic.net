﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.ServiceModel.Channels;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Magic.Net.Data;
using Magic.Serialization;

namespace Magic.Net
{
    public sealed class NodeSystem : ISystem, _ISender
    {
        #region Fields

        private readonly BufferManager _bufferManager = BufferManager.CreateBufferManager(300 * 1024 * 1024, 2 * 1024 * 1024);

        [NotNull, ItemNotNull]
        private readonly List<INetConnection> _connections = new List<INetConnection>();
        private readonly ISerializeFormatterCollection _formatterCollection;

        private readonly IDataPackageHandler _packageHandler;
        private bool _isRunning;
        private readonly string _systemName;
        private readonly ObjectStreamManager _objectStreamManager;
        private readonly InternalServiceCollection _serviceCollectionProvider;

        #endregion Fields

        public NodeSystem()
            : this(Environment.MachineName, new ServiceCollection(), new DefaulSerializeFormatter())
        {
        }

        public NodeSystem([NotNull] string systemName)
            : this(systemName, new ServiceCollection(), new DefaulSerializeFormatter())
        {
        }

        public NodeSystem([NotNull] string systemName, [NotNull] IServiceProvider serviceProvider)
            : this(systemName, serviceProvider, new DefaulSerializeFormatter())
        {
        }

        public NodeSystem([NotNull] string systemName, [NotNull] IServiceProvider serviceProvider, ISerializeFormatterCollection formatterCollection)
        {
            if (string.IsNullOrWhiteSpace(systemName))
                throw new ArgumentException("Argument is null or whitespace", "systemName");
            if (serviceProvider == null) throw new ArgumentNullException("serviceProvider");
            if (string.IsNullOrWhiteSpace(systemName))
                throw new ArgumentException("Argument is null or whitespace", "systemName");

            _objectStreamManager = new ObjectStreamManager(formatterCollection);
            _formatterCollection = formatterCollection;
            _systemName = systemName;
            _serviceCollectionProvider = new InternalServiceCollection(serviceProvider);
            _packageHandler = new DataPackageHandler(this, _serviceCollectionProvider, formatterCollection);
            _serviceCollectionProvider[typeof(IObjectStreamService)] = typeof(ObjectStreamRemoteService);
        }

        /// <summary>
        /// Only for Tests
        /// </summary>
        internal NodeSystem([NotNull] string systemName, ISerializeFormatterCollection formatterCollection,
            [NotNull] IDataPackageHandler dataPackageHandler)
        {
            if (dataPackageHandler == null) throw new ArgumentNullException("dataPackageHandler");
            if (string.IsNullOrWhiteSpace(systemName))
                throw new ArgumentException("Argument is null or whitespace", "systemName");


            _objectStreamManager = new ObjectStreamManager(formatterCollection);
            _formatterCollection = formatterCollection;
            _systemName = systemName;
            _packageHandler = dataPackageHandler;
        }

        public void Start()
        {
            lock (_connections)
            {
                _isRunning = true;
                foreach (var netConnection in _connections)
                {
                    OpenConnection(netConnection);
                }
            }
        }

        private static void OpenConnection(INetConnection netConnection)
        {
            if (!netConnection.IsConnected)
            {
                netConnection.Open();
            }
        }

        public void Stop()
        {
            lock (_connections)
            {
                _isRunning = false;
                foreach (var netConnection in _connections.ToArray())
                {
                    netConnection.Close();
                    var disposable = netConnection as IDisposable;
                    if (disposable != null)
                        disposable.Dispose();
                }
            }
        }

        [PublicAPI]
        public async Task<TResult> ExcAsync<TResult>(Uri remoteAddress, Expression<Func<TResult>> expression,
            int imeoutMilliseconds = Timeout.Infinite)
        {
            return
                await
                    TaskHelper.Run<Uri, Expression<Func<TResult>>, int, TResult>(Execute, remoteAddress, expression,
                        imeoutMilliseconds);
        }

        public TResult Execute<TResult>(Uri remoteAddress, Expression<Func<TResult>> expression,
            int imeoutMilliseconds = Timeout.Infinite)
        {
            var connection = FindConnection(remoteAddress);

            if (connection == null)
                throw new Exception();

            var command = expression.ToNetCommand();
            return Execute<TResult>(connection, command, imeoutMilliseconds);
        }

        /// <summary>
        ///     Experimental
        /// </summary>
        /// <typeparam name="TRemoteType"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="remoteAddress"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        // ReSharper disable once UnusedMember.Global
        public TResult Exc2<TRemoteType, TResult>(Uri remoteAddress, Expression<Func<TRemoteType, TResult>> expression)
        {
            var connection = FindConnection(remoteAddress);

            var command = expression.ToNetCommand();

            return default(TResult);
        }

        private INetConnection FindConnection(Uri remoteAddress)
        {
            string path = remoteAddress.GetLeftPart(UriPartial.Path);
            INetConnection result = null;
            lock (_connections)
            {

                result = _connections.FirstOrDefault(
                        c =>
                            string.Equals(c.RemoteAddress.Host, remoteAddress.Host,
                                StringComparison.InvariantCultureIgnoreCase)
                            && c.RemoteAddress.Port == remoteAddress.Port
                            && string.Equals(c.RemoteAddress.GetLeftPart(UriPartial.Path), path,
                                StringComparison.InvariantCultureIgnoreCase));
            }
            if (result == null)
            {
                throw new NetException(NetExceptionReasonses.NoConnectionFound, "No connection with remote address '"+remoteAddress+"' found.");
            }
            return result;
        }

        private TResult Execute<TResult>(INetConnection connection, INetCommand command, int imeoutMilliseconds)
        {
            var header = NetDataPackageHeader.CreateNetDataPackageHeader(DataPackageContentType.NetCommand, DataSerializeFormat.MsBinary);
            byte[] buffer = null;
            var magicSerialization = command as IMagicSerialization;
            if (magicSerialization != null)
            {
                header = NetDataPackageHeader.CreateNetDataPackageHeader(DataPackageContentType.NetCommand, DataSerializeFormat.Magic);
            }

            var s = _formatterCollection.GetFormatter(header.SerializeFormat);
            buffer = s.Serialize(command);
            var package = new NetDataPackage(header, buffer, 0, buffer.Length);

            connection.Send(package.DataSegments().ToArray());

            if (typeof (TResult) != typeof (void))
            {
                var commandResult = new DataPackageHandler.CommandResultAwait(command.Id, _packageHandler);
                if (!commandResult.WaitHandle.WaitOne(imeoutMilliseconds))
                {
                    throw new TimeoutException();
                }

                var remoteException = commandResult.Result as Exception;
                if (remoteException != null)
                {
                    throw new Exception("RemoteException: " + remoteException.Message, remoteException);
                }

                return (TResult) commandResult.Result;
            }

            return default(TResult);
        }


        #region Implementation of ISystem

        public ISerializeFormatterCollection FormatterCollection { get { return _formatterCollection; } }

        void ISystem.AddConnection([NotNull] INetConnection connection)
        {
            lock (_connections)
            {
                _connections.Add(connection);
                if (_isRunning)
                {
                    OpenConnection(connection);
                }
            }
        }

        public string SystemName
        {
            get { return _systemName; }
        }

        public BufferManager BufferManager
        {
            get { return _bufferManager; }
        }

        public IDataPackageHandler PackageHandler
        {
            get { return _packageHandler; }
        }

        public IEnumerator<T> CreateObjectStream<T>(Uri remoteAddress)
        {
            return CreateObjectStream<T>(remoteAddress, Timeout.InfiniteTimeSpan);
        }

        public IEnumerator<T> CreateObjectStream<T>(Uri remoteAddress, TimeSpan timeout)
        {
            INetConnection connection = FindConnection(remoteAddress);
            
            RemoteObjectStream<T> remoteObjectStream = this.Execute(remoteAddress, () => Proxy<IObjectStreamService>.Target.Create<T>(), Convert.ToInt32(timeout.TotalMilliseconds));
            _objectStreamManager.Add(remoteObjectStream);

            //ObjectStream<T> objectStream = _objectStreamManager.Create<T>(connection.RemoteAddress, Guid.NewGuid());
            //ObjectStreamInfo info = new ObjectStreamInfo {State = ObjectStreamState.Creating};
            //ISerializeFormatter serializeFormatter = _objectStreamManager.FormatterCollection.GetFormatter(connection.DefaultSerializeFormat);
            //byte[] bytes = serializeFormatter.Serialize(info);
            //NetDataPackage package = new NetDataPackage(NetDataPackageHeader.CreateNetDataPackageHeader(DataPackageContentType.NetObjectStreamInitialize,
            //    connection.DefaultSerializeFormat), bytes, 0, bytes.Length);
            //connection.Send(package.DataSegments().ToArray());

            return remoteObjectStream;
        }

        #endregion

        private class InternalServiceCollection : Dictionary<Type, object>, IServiceProvider
        {
            private readonly IServiceProvider _serviceProvider;

            public InternalServiceCollection(IServiceProvider serviceProvider)
            {
                _serviceProvider = serviceProvider;
            }

            #region Implementation of IServiceProvider

            public object GetService(Type serviceType)
            {
                object service;
                if (this.TryGetValue(serviceType, out service))
                {
                    return GetInstance(service);
                }
                return _serviceProvider.GetService(serviceType);
            }

            #endregion
            
            private static object GetInstance(object service)
            {
                Type serviceType = service as Type;
                if (serviceType != null)
                {
                    return Activator.CreateInstance(serviceType);
                }
                return service;
            }
        }

        private interface IObjectStreamService
        {
            RemoteObjectStream<T> Create<T>();
        }

        private class ObjectStreamRemoteService : HubRemoteService, IObjectStreamService
        {
            #region Implementation of IObjectStreamService

            public RemoteObjectStream<T> Create<T>()
            {
                RemoteObjectStream<T> result = new RemoteObjectStream<T>(new Uri(this.Connection.LocalAddress, Guid.NewGuid().ToString()));
                return result;
            }

            #endregion
        }

        #region Implementation of _ISender

        void _ISender.Send(object package)
        {
            throw new NotImplementedException();
        }

        IObjectStreamManager _ISender.ObjectStreamManager
        {
            get { return _objectStreamManager; }
        }

        #endregion
    }
}