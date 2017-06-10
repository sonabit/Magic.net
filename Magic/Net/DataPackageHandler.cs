using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using Magic.Net.Data;
using Magic.Serialization;

namespace Magic.Net
{
    internal sealed class DataPackageHandler : IDataPackageHandler
    {
        #region Fields

        private readonly _ISender _hub;
        private readonly ISerializeFormatterCollection _formatterCollection;
        private readonly IServiceProvider _serviceProvider;
        private Action<object> _commandResult;

        #endregion Fields

        public DataPackageHandler([NotNull] _ISender hub, [NotNull] IServiceProvider serviceProvider, [NotNull] ISerializeFormatterCollection formatterCollection)
        {
            if (hub == null) throw new ArgumentNullException("hub");
            if (serviceProvider == null) throw new ArgumentNullException("serviceProvider");
            if (formatterCollection == null) throw new ArgumentNullException("formatterCollection");

            _hub = hub;
            _formatterCollection = formatterCollection;
            _serviceProvider = serviceProvider;
        }

        public event Action<object> CommandResult
        {
            add { _commandResult += value; }
            remove { _commandResult += value; }
        }

        private void HandelReceivedCommandCallBack([NotNull] object state)
        {
            Trace.WriteLine("HandelReceivedCommandCallBack " + ((RequestState) state).Connetion.RemoteAddress);
            InvokeCommand((RequestState) state);
        }

        private void InvokeCommand([NotNull] RequestState requestState)
        {
            if (requestState == null) throw new ArgumentNullException("requestState");

            var command = (NetCommand) requestState.Package.Data;

            object serviceInstance = _serviceProvider.GetService(command.ServiceType);
            HubRemoteService hubRemoteService = serviceInstance as HubRemoteService;
            if (hubRemoteService != null)
            {
                hubRemoteService.Connection = requestState.Connetion;
            }

            object result;
            try
            {
                Trace.WriteLine(string.Format("InvokeCommand {0}.{1}", command.ServiceType.FullName, command.MethodName.Name));
                result = command.MethodName.Invoke(serviceInstance, command.ParameterValues);
            }
            catch (Exception ex)
            {
                result = ex;
            }

            // ReSharper disable once OperatorIsCanBeUsed
            if (result != null && result.GetType() == typeof (void))
            {
                result = null;
            }

            var commandResult = new NetCommandResult(command.Id, result);
            var serializeFormat = requestState.Connetion.DefaultSerializeFormat;
            var serializeFormatter = _formatterCollection.GetFormatter(serializeFormat);
            byte[] buffer = serializeFormatter.Serialize(commandResult);

            var package =
                new NetDataPackage(
                    NetDataPackageHeader.CreateNetDataPackageHeader(DataPackageContentType.NetCommandResult, serializeFormat), buffer,
                    0, buffer.Length);

            requestState.Connetion.Send(package.DataSegments().ToArray());
        }

        internal sealed class CommandResultAwait
        {
            public CommandResultAwait(Guid id, IDataPackageHandler packageHandler)
            {
                _id = id;
                _packageHandler = packageHandler;
                packageHandler.CommandResult += OnCommandResult;
            }

            public Guid Id
            {
                get { return _id; }
            }

            public WaitHandle WaitHandle
            {
                get { return _onResultEvent.WaitHandle; }
            }

            public object Result { get; private set; }

            private void OnCommandResult(object result)
            {
                Result = result;
                _onResultEvent.Set();
                _packageHandler.CommandResult -= OnCommandResult;
            }

            #region Fields

            private readonly Guid _id;
            private readonly IDataPackageHandler _packageHandler;
            private readonly ManualResetEventSlim _onResultEvent = new ManualResetEventSlim(false);

            #endregion Fields
        }

        #region Implementation of IDataPackageHandler

        public void ReceiveCommand([NotNull] RequestState requestState)
        {
            if (requestState == null) throw new ArgumentNullException("requestState");

            while (!ThreadPool.QueueUserWorkItem(HandelReceivedCommandCallBack, requestState))
            {
                Trace.WriteLine("ThreadPool.QueueUserWorkItem unsuccessful");
                Thread.Sleep(50);
            }
        }

        public void ReceiveCommandStream([NotNull] RequestState request)
        {
            switch (request.Package.PackageContentType)
            {
                case DataPackageContentType.NetCommand:
                    ReceiveCommand(request);
                    break;
                case DataPackageContentType.NetCommandResult:
                    ReceiveCommandResult(request);
                    break;
                case DataPackageContentType.NetObjectStreamInitialize:
                    InitializeNewObjectStream(request);
                    break;
                case DataPackageContentType.NetObjectStreamData:
                    HandelNetObjectStreamData(request);
                    break;
                case DataPackageContentType.NetObjectStreamClose:
                    break;
                case DataPackageContentType.ConnectionMetaData:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void InitializeNewObjectStream(RequestState request)
        {
            ObjectStreamInfo info = request.Package.Data as ObjectStreamInfo;
            if (info != null && info.State == ObjectStreamState.Creating)
            {
                //ObjectStream objectStream = _objectStreamManager.Create(request.Connetion.RemoteAddress, info.StreamId);
                //info.State = ObjectStreamState.Established;
                //ISerializeFormatter serializeFormatter = _objectStreamManager.FormatterCollection.GetFormatter(request.Package.SerializeFormat);
                //byte[] bytes = serializeFormatter.Serialize(info);
                //NetDataPackage package = new NetDataPackage(NetDataPackageHeader.CreateNetDataPackageHeader(DataPackageContentType.NetObjectStreamInitialize,
                //    request.Package.SerializeFormat), bytes, 0, bytes.Length);
                //request.Connetion.Send(package.DataSegments().ToArray());
                
            }
        }

        private void HandelNetObjectStreamData(RequestState requestState)
        {
            NetObjectStreamData streamData = requestState.Package.Data as NetObjectStreamData;
            if (streamData != null)
            {
                ObjectStream objectStream =
                    _hub.ObjectStreamManager.GetObjectStream(
                        requestState.Connetion.RemoteAddress.AddPath(streamData.Id.ToString()));
                if (objectStream != null)
                {
                    objectStream.Push(streamData.Data);
                    return;
                }
            }
            Console.Error.WriteLine("Drop " + typeof(NetObjectStreamData) + ", " + typeof(ObjectStream) +"  not found.");
        }

        private void ReceiveCommandResult(RequestState requestState)
        {
            NetCommandResult commandResult = (NetCommandResult) requestState.Package.Data;

            if (_commandResult == null) return;

            Delegate[] delegates = _commandResult.GetInvocationList()
                .Where(
                    s => s.Target is CommandResultAwait && ((CommandResultAwait) s.Target).Id == commandResult.CommandId).ToArray();
            if (delegates.Length == 0)
            {
                Debug.WriteLine("DataPackageHandler.ReceiveCommandResult CommandId {0} not found.", commandResult.CommandId);
                return;
            }
            foreach (var subscriber in delegates)
            {
                CommandResultAwait com = subscriber.Target as CommandResultAwait;
                if (com != null && com.Id == commandResult.CommandId)
                {
                    subscriber.DynamicInvoke(commandResult.Result);
                }
            }
        }

        #endregion
    }
}
