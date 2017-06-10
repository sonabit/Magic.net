using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;

namespace Magic.Net.Server
{
    public sealed class NamedPipeServerNetConnection : INetConnection, IDisposable
    {
        private readonly ManualResetEventSlim _closeEvent = new ManualResetEventSlim(true);
        private readonly List<INetConnection> _connectionHosts = new List<INetConnection>();
        private readonly ManualResetEventSlim _connectionInLimitEvent = new ManualResetEventSlim(true);
        private MagicNetEndPoint _localEndPoint;
        private ISystem _nodeSystem;

        private bool _disposedValue; // Dient zur Erkennung redundanter Aufrufe.
        private bool _isInitialized;
        
        public void LinkTo([NotNull]ISystem system)
        {
            if (system == null) throw new ArgumentNullException("system");

            if (_isInitialized) return;
            _isInitialized = true;
            _localEndPoint = new MagicNetEndPoint(MagicNetEndPoint.BuildUri(Environment.MachineName, system.SystemName));
            _nodeSystem = system;
            system.AddConnection(this);
        }

        public Uri RemoteAddress
        {
            get { throw new NotSupportedException("This instance is a listener."); }
        }

        public Uri LocalAddress
        {
            get { return _localEndPoint.OriginUri; }
        }

        public int TimeoutMilliseconds
        {
            get { throw new NotSupportedException("This instance is a listener."); }
        }

        public DataSerializeFormat DefaultSerializeFormat
        {
            get { return DataSerializeFormat.MsBinary; }
        }


        public event EventHandler<INetConnection> ConnectionAccepted;

        private void OnDisconnected()
        {
            Action<INetConnection> handler = Disconnected;
            if (handler != null) handler(this);
        }

        private void OnConnectionAccepted(INetConnection connection)
        {
            EventHandler<INetConnection> handler = ConnectionAccepted;
            if (handler != null) handler(this, connection);
        }

        #region Implementation of INetConnectionAdapter

        public bool IsConnected
        {
            get { return !_closeEvent.IsSet; }
        }

        public void Open()
        {
            if (IsConnected)
            {
                return;
            }
            OpenInternal(true);
        }

        public void Close()
        {
            CloseInternal();
        }

        public event Action<INetConnection> Disconnected;

        public void Send(params byte[][] bytes)
        {
            throw new NotSupportedException("This instance is a listener.");
        }

        public void Send(params ArraySegment<byte>[] segments)
        {
            throw new NotSupportedException("This instance is a listener.");
        }

        private void OpenInternal(bool withOwnThread)
        {
            if (!_nodeSystem.IsRunning)
                return;

            CloseInternal();
            _closeEvent.Reset();
            if (withOwnThread)
            {
                new Thread(AcceptConnections).Start();
            }
            else
            {
                AcceptConnections();
            }
        }

        private void CloseInternal()
        {
            _closeEvent.Set();

            INetConnection[] connections;
            lock (_connectionHosts)
            {
                connections = _connectionHosts.ToArray();
            }
            connections.Each(adp => adp.Close());
            OnDisconnected();
        }

        private void AcceptConnections()
        {
            while (!_closeEvent.IsSet)
            {
                try
                {
                    var pipeServerStream = new NamedPipeServerStream(_localEndPoint.SystemName, PipeDirection.InOut,
                        NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte,
                        PipeOptions.Asynchronous | PipeOptions.WriteThrough);
                    var asyncResult = pipeServerStream.BeginWaitForConnection(AcceptConnection_OnCallback,
                        pipeServerStream);
                    WaitHandle.WaitAny(new[] {asyncResult.AsyncWaitHandle, _closeEvent.WaitHandle});
                    if (!asyncResult.IsCompleted)
                    {
                        pipeServerStream.Close();
                    }
                }
                catch (IOException ioException)
                {
                    if ((uint) ioException.HResult == 0x800700E7) //(-2147024665) All pipe instances are busy
                    {
                        _connectionInLimitEvent.Reset();

                        Trace.WriteLine(string.Format("{0:G} ERROR :{1}", DateTime.Now, ioException.Message));
                        // Simple wait if some connections are closed.
                        _connectionInLimitEvent.Wait(10000);

                        continue;
                    }
                    throw;
                }
                catch (ObjectDisposedException)
                {
                }
                catch (ThreadAbortException)
                {
                    return;
                }
            }
        }

        private void AcceptConnection_OnCallback(IAsyncResult ar)
        {
            var stream = (NamedPipeServerStream) ar.AsyncState;
            try
            {
                stream.EndWaitForConnection(ar);
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            try
            {
                var adapter = new NamedPipeClientAdapterHost(stream, _nodeSystem);
                adapter.Initialize();

                NetConnection connection = new NetConnectionStreamHost(adapter);

                Trace.WriteLine("NetCommandPipeServer: new StreamNetConnection ");

                connection.Disconnected += HostOnDisconnected;
                lock (_connectionHosts)
                {
                    _connectionHosts.Add(connection);
                }
                connection.InitializeConnection(true);

                OnConnectionAccepted(connection);
            }
            catch (NetException ex)
            {
                // NetCommandException
                Trace.WriteLine(string.Format("{0} - {1}: {2}", ex.GetType().Name, ex.Reasonses, ex.Message));
            }
            catch (Exception ex)
            {
                // Exception
                Trace.WriteLine(string.Format("{0}: {1}", ex.GetType().Name, ex.Message));
            }
        }

        private void HostOnDisconnected(INetConnection sender)
        {
            sender.Disconnected -= HostOnDisconnected;
            lock (_connectionHosts)
            {
                _connectionHosts.Remove(sender);
            }
            if (!_connectionInLimitEvent.IsSet)
                _connectionInLimitEvent.Set();
        }

        #region IDisposable Support

        private void Dispose(bool disposingManagedObjects)
        {
            if (!_disposedValue)
            {
                if (disposingManagedObjects)
                {
                    CloseInternal();
                }

                // TODO: nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer weiter unten überschreiben.
                // TODO: große Felder auf Null setzen.

                _disposedValue = true;
            }
        }

        // Finalizer nur überschreiben, wenn Dispose(bool disposing) weiter oben Code für die Freigabe nicht verwalteter Ressourcen enthält.
        // ~NamedPipeServerNetConnection() {
        //   // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in Dispose(bool disposing) weiter oben ein.
        //   Dispose(false);
        // }

        // Dieser Code wird hinzugefügt, um das Dispose-Muster richtig zu implementieren.
        public void Dispose()
        {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in Dispose(bool disposing) weiter oben ein.
            Dispose(true);
            // Auskommentierung der folgenden Zeile aufheben, wenn der Finalizer weiter oben überschrieben wird.
            // GC.SuppressFinalize(this);
        }

        #endregion

        #endregion
    }
}