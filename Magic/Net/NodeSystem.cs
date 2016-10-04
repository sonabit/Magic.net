using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.ServiceModel.Channels;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Magic.Serialization;

namespace Magic.Net
{
    public sealed class NodeSystem : ISystem
    {
        #region Fields

        private readonly BufferManager _bufferManager = BufferManager.CreateBufferManager(300 * 1024 * 1024, 2 * 1024 * 1024);

        [NotNull, ItemNotNull]
        private readonly List<INetConnection> _connections = new List<INetConnection>();
        private readonly ISerializeFormatterCollection _formatterCollection = new DefaulSerializeFormatter();

        private readonly DataPackageHandler _packageHandler;
        private bool _isRunning;
        private readonly string _systemName;

        #endregion Fields

        public NodeSystem([NotNull] string systemName)
            : this(systemName, new ServiceCollection())
        {
        }

        public NodeSystem([NotNull] string systemName, [NotNull] IServiceProvider serviceProvider)
        {
            if (serviceProvider == null) throw new ArgumentNullException("serviceProvider");
            if (string.IsNullOrWhiteSpace(systemName))
                throw new ArgumentException("Argument is null or whitespace", "systemName");

            _systemName = systemName;
            _packageHandler = new DataPackageHandler(serviceProvider, _formatterCollection);
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
            lock (_connections)
            {
                string path = remoteAddress.GetLeftPart(UriPartial.Path);

                return
                    _connections.FirstOrDefault(
                        c =>
                            string.Equals(c.RemoteAddress.Host, remoteAddress.Host,
                                StringComparison.InvariantCultureIgnoreCase)
                            && c.RemoteAddress.Port == remoteAddress.Port
                            && string.Equals(c.RemoteAddress.GetLeftPart(UriPartial.Path), path,
                                StringComparison.InvariantCultureIgnoreCase));
            }
        }

        private TResult Execute<TResult>(INetConnection connection, INetCommand command, int imeoutMilliseconds)
        {
            var header = new NetDataPackageHeader(1, DataPackageContentType.NetCommand, DataSerializeFormat.MsBinary);
            byte[] buffer = null;
            var magicSerialization = command as IMagicSerialization;
            if (magicSerialization != null)
            {
                header = new NetDataPackageHeader(1, DataPackageContentType.NetCommand, DataSerializeFormat.Magic);
            }

            var s = _formatterCollection[header.SerializeFormat];
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

        public void AddConnection([NotNull] INetConnection connection)
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

        #endregion
    }

    public static class Proxy<TTarget>
    {
        public static TTarget Target
        {
            get { return default(TTarget); }
        }
    }
}