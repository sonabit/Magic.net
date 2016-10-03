using System;
using JetBrains.Annotations;

namespace Magic.Net
{
    public interface IDataPackageHandler
    {
        void ReceiveCommand([NotNull]RequestState requestState); 

        void ReceiveCommandStream([NotNull] RequestState package);

        void ReceiveCommandResult([NotNull]RequestState requestState);

        event Action<object> CommandResult;
    }
}