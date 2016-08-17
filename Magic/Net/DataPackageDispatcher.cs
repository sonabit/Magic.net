using System;
using System.Diagnostics;
using System.Threading;
using JetBrains.Annotations;

namespace Magic.Net
{
    internal sealed class DataPackageDispatcher : IDataPackageDispatcher
    {
        private readonly IDataPackageHandler _netCommandHandle;

        public DataPackageDispatcher(IDataPackageHandler netCommandHandle)
        {
            _netCommandHandle = netCommandHandle;
        }

        #region Implementation of IDataPackageDispatcher

        public void Handle(NetDataPackage package)
        {
            switch (package.Version)
            {
                case 1:
                    HandleVersion1(package);
                    break;
                default:
                    throw new NotSupportedException(
                        string.Format("NetDataPackage version {0} not supported!", package.Version));
            }
        }

        #endregion

        private void HandleVersion1(NetDataPackage package)
        {
            switch (package.PackageContentType)
            {
                case DataPackageContentType.NetCommand:
                    _netCommandHandle.ReceiveCommand(package);
                    break;
                    case DataPackageContentType.NetCommandStream:
                        _netCommandHandle.ReceiveCommandStream(package);
                    break;
                default:
                    // this case should never happened
                    throw new NetCommandException(NetCommandExceptionReasonses.UnknownPackageContentType, string.Format("package.PackageContentType {0} unknown.", package.PackageContentType));
            }
        }

        public void OnReceivedData([NotNull] NetDataPackage buffer)
        {
        }

        private void HandelReceivedData([NotNull] NetDataPackage package)
        {
            while (!ThreadPool.QueueUserWorkItem(HandelReceivedDataCallBack, package))
            {
                Trace.WriteLine("ThreadPool.QueueUserWorkItem unsuccessful");
                Thread.Sleep(50);
            }
        }

        private void HandelReceivedDataCallBack([NotNull] object package)
        {
            Console.WriteLine("HandelReceivedDataCallBack");
            OnReceivedData((NetDataPackage)package);
        }
    }

    public class DataPackageHandler : IDataPackageHandler
    {
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
            throw new NotImplementedException();
        }
    }
}