using JetBrains.Annotations;

namespace Magic.Net
{
    public interface IDataPackageHandler
    {
        void ReceiveCommand([NotNull]NetCommand package);

        void ReceiveCommandStream([NotNull]NetDataPackage package);
    }
}