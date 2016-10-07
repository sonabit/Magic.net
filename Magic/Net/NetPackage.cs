namespace Magic.Net
{
    public abstract class NetPackage
    {
        public abstract bool IsEmpty { get; }

        public abstract byte Version { get; }

        public abstract DataPackageContentType PackageContentType { get; }

        public abstract DataSerializeFormat SerializeFormat { get; }
    }
}