using JetBrains.Annotations;
using Magic.Net;

namespace Magic.Net
{
    internal interface IDataPackageDispatcher
    {
        void Handle([NotNull]RequestState requestState);
    }
}