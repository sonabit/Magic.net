using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Magic.Net.Server;

namespace Magic.Net
{
    public sealed class NodeSystem : ISystem
    {
        [NotNull,ItemNotNull]
        private readonly List<INetConnection> _connections = new List<INetConnection>();

        private readonly IBufferManager _bufferManager = new BufferManager();
        private  readonly DataPackageHandler _packageHandler = new DataPackageHandler();

        public NodeSystem()
        {
            //this.channels = channels;
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

        public void AddConnection([NotNull]INetConnection connection)
        {
            lock (_connections)
            {
                _connections.Add(connection);
            }
        }

        #region Implementation of ISystem

        public IBufferManager BufferManager { get { return _bufferManager; } }
        public IDataPackageHandler PackageHandler { get { return _packageHandler; } }

        #endregion

        public void Stop()
        {
            lock (_connections)
            {
                foreach (INetConnection netConnection in _connections.ToArray())
                {
                    netConnection.Close();
                    IDisposable disposable = netConnection as IDisposable;
                    if (disposable != null)
                        disposable.Dispose();
                }
                _connections.Clear();
            }
        }
    }

    public interface ISystem
    {
        IBufferManager BufferManager { get; }

        IDataPackageHandler PackageHandler { get; }
    }

    //public class ConnectionFactory
    //{
    //    private readonly Dictionary<ConnectionSchema, ConnectionActivator> schemas = new Dictionary<ConnectionSchema, ConnectionActivator>
    //    {
    //        //{ new ConnectionSchema("MGPIP",ConnectionType.Server) , new ConnectionFuncActivator(node => new NamedPipeServerNetConnection(node) )}
    //    };
        
    //    [CanBeNull]
    //    public INetConnection Create(ConnectionSchema schema, ISystem system)
    //    {
    //        ConnectionActivator activator;
    //        if (schemas.TryGetValue(schema, out activator))
    //        {
    //            return activator.Create(system);
    //        }

    //        return null;
    //    }
    //}


    //public struct ConnectionSchema
    //{
    //    private readonly string _schema;
    //    private readonly ConnectionType _connectionType;

    //    public ConnectionSchema(string schema, ConnectionType type)
    //    {
    //        _schema = schema;
    //        _connectionType = type;
    //    }

    //    public string Schema
    //    {
    //        get { return _schema; }
    //    }

    //    public ConnectionType ConnectionType
    //    {
    //        get { return _connectionType; }
    //    }
    //}

    //public enum ConnectionType
    //{
    //    Client = 0,
    //    Server =1 
    //}


    //public abstract class ConnectionActivator
    //{
    //    public abstract INetConnection Create(ISystem system);
    //}

    //class ConnectionFuncActivator : ConnectionActivator
    //{
    //    private readonly Func<ISystem, INetConnection> _func;

    //    public ConnectionFuncActivator(Func<ISystem,INetConnection> func)
    //    {
    //        _func = func;
    //    }

    //    #region Overrides of ConnectionActivator

    //    public override INetConnection Create(ISystem system)
    //    {
    //        return _func(system);
    //    }

    //    #endregion
    //}
}

