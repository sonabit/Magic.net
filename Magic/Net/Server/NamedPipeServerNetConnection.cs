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
        [NotNull]
        private readonly PipeSettings _settings;

        private readonly ISystem _nodeSystem;
        
        private bool _disposedValue = false; // Dient zur Erkennung redundanter Aufrufe.
        private readonly ManualResetEventSlim _closeEvent = new ManualResetEventSlim(true);
        private readonly ManualResetEventSlim _connectionInLimitEvent = new ManualResetEventSlim(true);
        private readonly List<INetConnection> _connectionHosts = new List<INetConnection>();


        public NamedPipeServerNetConnection([NotNull]PipeSettings settings, ISystem nodeSystem)
        {
            _settings = settings;
            _nodeSystem = nodeSystem;
        }

        public Uri Address
        {
            get { return _settings.Uri; }
        }

        #region Implementation of INetConnectionAdapter

        public bool IsConnected { get { return !_closeEvent.IsSet; } }
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

        private void OpenInternal(bool withOwnThread)
        {
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
                    NamedPipeServerStream pipeServerStream = new NamedPipeServerStream(_settings.PipeName, PipeDirection.InOut,
                        NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.Asynchronous | PipeOptions.WriteThrough);
                    IAsyncResult asyncResult = pipeServerStream.BeginWaitForConnection(AcceptConnection_OnCallback, pipeServerStream);
                    WaitHandle.WaitAny(new[] { asyncResult.AsyncWaitHandle, _closeEvent.WaitHandle });
                    if (!asyncResult.IsCompleted)
                    {
                        pipeServerStream.Close();
                    }
                }
                catch (IOException ioException)
                {
                    if ((UInt32)ioException.HResult == 0x800700E7) //(-2147024665) All pipe instances are busy
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
                    continue;
                }
                catch (ThreadAbortException)
                {
                    return;
                }
            }
        }

        private void AcceptConnection_OnCallback(IAsyncResult ar)
        {
            NamedPipeServerStream stream = (NamedPipeServerStream)ar.AsyncState;
            try
            {
                stream.EndWaitForConnection(ar);
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            ReadWriteStreamAdapter adapter = new NamedPipeAdapter(stream, _nodeSystem.BufferManager)
            {
                //ReleaseDataAfterSend = true,
            };
            INetConnection connection = new StreamNetConnection(adapter, _nodeSystem.PackageHandler);

            Trace.WriteLine("NetCommandPipeServer: new StreamNetConnection");

            connection.Disconnected += HostOnDisconnected;
            lock (_connectionHosts)
            {
                _connectionHosts.Add(connection);
            }
            connection.Open();

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

        private void OnDisconnected()
        {
            var handler = Disconnected;
            if (handler != null) handler(this);
        }
    }

    internal sealed class NamedPipeAdapter : ReadWriteStreamAdapter
    {
        private readonly NamedPipeServerStream _stream;

        public NamedPipeAdapter([NotNull] NamedPipeServerStream stream, [NotNull] IBufferManager bufferManager) 
            : base(stream, bufferManager)
        {
            _stream = stream;
        }

        #region Overrides of ReadWriteStreamAdapter

        public override bool IsConnected
        {
            get { return _stream.IsConnected; }
        }

        #endregion
    }

    internal sealed class StreamNetConnection : NetConnection
    {
        internal StreamNetConnection(INetConnectionAdapter connectionAdapter, IDataPackageHandler dataPackageHandler) : 
            base(connectionAdapter, dataPackageHandler)
        {
        }
    }
}