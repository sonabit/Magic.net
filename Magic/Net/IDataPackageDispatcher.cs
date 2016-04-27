namespace Magic.Net
{
    internal interface IDataPackageDispatcher
    {
        void Handle(NetDataPackage package);
    }
}