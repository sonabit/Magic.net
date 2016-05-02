namespace Magic.Net
{
    public class NetDataPackage
    {
        private readonly byte[] _buffer;

        public NetDataPackage(byte[] buffer)
        {
            _buffer = buffer;
            if (_buffer.Length > 1)
                PackageContentType = (DataPackageContentType)Buffer[1]; 
        }

        public byte[] Buffer
        {
            get { return _buffer; }
        }

        public bool IsEmpty { get { return Buffer.Length == 0; } }

        public byte Version { get { return Buffer[0]; } }

        public DataPackageContentType PackageContentType { get; private set; }
         
    }
}