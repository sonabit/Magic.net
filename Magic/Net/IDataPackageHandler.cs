using JetBrains.Annotations;

namespace Magic.Net
{
    public interface IDataPackageHandler
    {
        void ReceiveCommand([NotNull]NetDataPackage package);

        void ReceiveCommandStream([NotNull]NetDataPackage package);
    }
}