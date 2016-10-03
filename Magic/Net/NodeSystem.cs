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
        private readonly MagicNetEndPoint _systemEndPoint;

        #endregion Fields

        public NodeSystem([NotNull] Uri systemUri)
            : this(systemUri, new ServiceCollection())
        {
        }

        public NodeSystem([NotNull] Uri systemUri, [NotNull] IServiceProvider serviceProvider)
        {
            if (systemUri == null) throw new ArgumentNullException("systemUri");
            if (serviceProvider == null) throw new ArgumentNullException("serviceProvider");

            _systemEndPoint = new MagicNetEndPoint(systemUri);

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

        public Uri SystemAddress
        {
            get { return _systemEndPoint.AsRemoteUri(); }
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

    [PublicAPI]
    public sealed class MagicNetEndPoint
    {
        public const string SchemaPrefix = "magic://";

        private readonly Uri _uri;

        public MagicNetEndPoint([NotNull] Uri uri)
        {
            _uri = uri;
            Direction =
                (MagicNetEndPointDirection)
                    Enum.Parse(typeof (MagicNetEndPointDirection), _uri.GetStringOfSegment(1), true);
            SystemName = _uri.GetStringOfSegment(2);
        }

        public Uri OriginUri
        {
            get { return _uri; }
        }

        public string Host
        {
            get { return _uri.Host; }
        }

        public int Port
        {
            get { return _uri.Port; }
        }

        public MagicNetEndPointDirection Direction { get; private set; }

        public string SystemName { get; private set; }

        public static Uri BuildRemoteUri(string host, int port, string systemName)
        {
            return BuildBaseUri(host, port, MagicNetEndPointDirection.Remote, systemName);
        }

        public static Uri BuildBaseUri(string host, int port, MagicNetEndPointDirection direction, string systemName)
        {
            return new Uri(string.Format(SchemaPrefix + "{0}:{1}/{2}/{3}", host, port, direction, systemName));
        }

        public Uri AsRemoteUri()
        {
            return BuildRemoteUri(Host, Port, SystemName);
        }
    }

    public enum MagicNetEndPointDirection
    {
        Local,
        Remote
    }
}