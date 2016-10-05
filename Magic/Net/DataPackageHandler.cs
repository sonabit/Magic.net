using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using Magic.Serialization;

namespace Magic.Net
{
    public sealed class DataPackageHandler : IDataPackageHandler
    {

        #region Fields

        private readonly ISerializeFormatterCollection _formatterCollection;
        private readonly IServiceProvider _serviceProvider;
        private Action<object> _commandResult;
        private const int version = 1;

        #endregion Fields


        public DataPackageHandler(IServiceProvider serviceProvider, ISerializeFormatterCollection formatterCollection)
        {
            _serviceProvider = serviceProvider;
            _formatterCollection = formatterCollection;
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

        public void ReceiveCommandStream([NotNull] RequestState package)
        {
            throw new NotImplementedException();
        }

        public void ReceiveCommandResult(RequestState requestState)
        {
            var commandResult =
                _formatterCollection[requestState.Package.SerializeFormat].Deserialize<NetCommandResult>(
                    requestState.Package.Buffer.Array, requestState.Package.Buffer.Offset);

            if (_commandResult == null) return;

            foreach (var subscriber in _commandResult.GetInvocationList())
            {
                var com = subscriber.Target as CommandResultAwait;
                if (com != null && com.Id == commandResult.CommandId)
                {
                    subscriber.DynamicInvoke(commandResult.Result);
                    break;
                }
            }
        }

        #endregion

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

            var command =
                _formatterCollection[requestState.Package.SerializeFormat].Deserialize<NetCommand>(
                    requestState.Package.Buffer.Array, requestState.Package.Buffer.Offset);

            var service = _serviceProvider.GetService(command.ServiceType);

            object result;
            try
            {
                result = command.MethodName.Invoke(service, command.ParameterValues);
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
            var serializeFormatter = _formatterCollection[serializeFormat];
            byte[] buffer = serializeFormatter.Serialize(commandResult);

            var package =
                new NetDataPackage(
                    new NetDataPackageHeader(version, DataPackageContentType.NetCommandResult, serializeFormat),
                    buffer, 0, buffer.Length);

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
    }
}