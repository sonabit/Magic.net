using JetBrains.Annotations;
using Magic.Net;
using Magic.Net.Data;

namespace Magic.Net
{
    internal interface IDataPackageDispatcher
    {
        void Handle([NotNull]RequestState requestState);
    }
}