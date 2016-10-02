using System;
using System.Diagnostics;
using System.Threading;
using JetBrains.Annotations;
using Magic.Serialization;

namespace Magic.Net
{
    public class DataPackageHandler : IDataPackageHandler
    {
        private readonly ISerializeFormatterCollection _formatterCollection;
        private IServiceProvider _serviceProvider;

        public DataPackageHandler(IServiceProvider serviceProvider, ISerializeFormatterCollection formatterCollection)
        {
            _serviceProvider = serviceProvider;
            _formatterCollection = formatterCollection;
        }

        #region Implementation of IDataPackageHandler

        public void ReceiveCommand([NotNull]NetDataPackage package)
        {
            while (!ThreadPool.QueueUserWorkItem(HandelReceivedDataCallBack, package))
            {
                Trace.WriteLine("ThreadPool.QueueUserWorkItem unsuccessful");
                Thread.Sleep(50);
            }
        }

        public void ReceiveCommandStream([NotNull]NetDataPackage package)
        {
            throw new NotImplementedException();
        }

        #endregion

        private void HandelReceivedDataCallBack([NotNull] object package)
        {
            Console.WriteLine("HandelReceivedDataCallBack");
            Execute((NetDataPackage)package);
        }

        private void Execute(NetDataPackage package)
        {
            NetCommand command = _formatterCollection[package.SerializeFormat].Deserialize<NetCommand>(package.Buffer.Array, package.Buffer.Offset);

            object service = _serviceProvider.GetService(command.ServiceType);

            object result = command.MethodName.Invoke(service, command.ParameterValues);

            //NetCommand command = MagicSerializeFormatter.Deserialize<NetCommand>(package.Buffer.Array, package.Buffer.Offset);
        }
    }
}