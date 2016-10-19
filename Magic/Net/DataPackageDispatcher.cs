using System;
using JetBrains.Annotations;
using Magic.Net.Data;

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

        public void Handle([NotNull] RequestState requestState)
        {
            if (requestState == null) throw new ArgumentNullException("requestState");

            switch (requestState.Package.Version)
            {
                case 1:
                    HandleVersion1(requestState);
                    break;
                default:
                    throw new NotSupportedException(
                        string.Format("NetDataPackage version {0} not supported!", requestState.Package.Version));
            }
        }

        #endregion

        private void HandleVersion1([NotNull] RequestState requestState)
        {
            switch (requestState.Package.PackageContentType)
            {
                case DataPackageContentType.NetCommand:
                    _netCommandHandle.ReceiveCommand(requestState);
                    break;
                case DataPackageContentType.NetCommandResult:
                    _netCommandHandle.ReceiveCommandStream(requestState);
                    break;
                case DataPackageContentType.NetObjectStreamInitialize:
                    _netCommandHandle.ReceiveCommandStream(requestState);
                    break;
                case DataPackageContentType.NetObjectStreamData:
                    _netCommandHandle.ReceiveCommandStream(requestState);
                    break;
                default:
                    // this case should never happened
                    throw new NetException(NetExceptionReasonses.UnknownPackageContentType,
                        string.Format("PackageContentType {0} unknown.", requestState.Package.PackageContentType));
            }
        }
    }
}