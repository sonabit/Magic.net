namespace Magic.Net
{
    public class NetCommandPackage
    {
        private readonly byte[] _buffer;

        public NetCommandPackage(byte[] buffer)
        {
            _buffer = buffer;
        }

        public byte[] Buffer
        {
            get { return _buffer; }
        }
    }
}