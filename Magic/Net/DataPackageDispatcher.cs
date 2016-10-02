using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Xml.Serialization;
using JetBrains.Annotations;
using Microsoft.SqlServer.Server;

namespace Magic.Net
{
    sealed class DataPackageDispatcher : IDataPackageDispatcher
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
    }
}