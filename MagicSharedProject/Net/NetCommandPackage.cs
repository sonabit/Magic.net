namespace Magic.Net
{
    public class NetCommandPackage
    {
        private readonly byte[] _buffer;

        public NetCommandPackage(byte[] buffer)
        {
            _buffer = buffer;
            if (_buffer.Length > 1)
                PackageContenttyp = (DataPackageContenttyp)Buffer[1]; 
        }

        public byte[] Buffer
        {
            get { return _buffer; }
        }

        public bool IsEmpty { get { return Buffer.Length == 0; } }

        public byte Version { get { return Buffer[0]; } }

        public DataPackageContenttyp PackageContenttyp { get; private set; }
         
    }
}