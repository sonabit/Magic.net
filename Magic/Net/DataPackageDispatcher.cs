using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Xml.Serialization;
using JetBrains.Annotations;
using Magic.Serialization;
using Microsoft.SqlServer.Server;

namespace Magic.Net
{
    sealed class DataPackageDispatcher : IDataPackageDispatcher
    {
        private readonly IDataPackageHandler _netCommandHandle;
        private readonly ISerializeFormatterCollection _formatterCollection;
        private static readonly MagicSerializeFormatter MagicSerializeFormatter = new MagicSerializeFormatter();

        public DataPackageDispatcher(IDataPackageHandler netCommandHandle)
        {
            _netCommandHandle = netCommandHandle;
            _formatterCollection = new DefaulSerializeFormatter();
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
                    //NetCommand command = _formatterCollection[package.SerializeFormat].Deserialize<NetCommand>(package.Buffer, 3);
                    NetCommand command = MagicSerializeFormatter.Deserialize<NetCommand>(package.Buffer.Array, package.Buffer.Offset);
                    _netCommandHandle.ReceiveCommand(command);
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

        public void ReceiveCommand([NotNull]NetCommand package)
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