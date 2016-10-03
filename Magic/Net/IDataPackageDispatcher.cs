using JetBrains.Annotations;

namespace Magic.Net
{
    internal interface IDataPackageDispatcher
    {
        void Handle([NotNull]RequestState requestState);
    }
}