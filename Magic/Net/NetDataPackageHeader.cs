using System;

namespace Magic.Net
{
    public class NetDataPackageHeader
    {
        public static NetDataPackageHeader CreateNetDataPackageHeader(DataPackageContentType packageContentType, DataSerializeFormat serializeFormat, byte version = 1)
        {
            return new NetDataPackageHeader(version, packageContentType, serializeFormat);
        }

        private readonly int _headerLength;

        public NetDataPackageHeader(ref byte[] buffer)
        {
            int len = 0;
            if (buffer.Length > 0)
            {
                Version = buffer[0];
                len++;
            }

            if (buffer.Length > 1)
            {
                PackageContentType = (DataPackageContentType)buffer[1];
                len++;
            }
            if (buffer.Length > 2)
            {
                SerializeFormat = (DataSerializeFormat)buffer[2];
                len++;
            }
            _headerLength = len;
        }

        private NetDataPackageHeader(byte version, DataPackageContentType packageContentType, DataSerializeFormat serializeFormat)
        {
            Version = version;
            PackageContentType = packageContentType;
            SerializeFormat = serializeFormat;
            _headerLength = 3;
        }

        internal int HeaderLength
        {
            get { return _headerLength; }
        }

        public byte Version { get; private set; }

        public DataPackageContentType PackageContentType { get; private set; }

        public DataSerializeFormat SerializeFormat { get; private set; }

        public byte[] ToBytes()
        {
            return new byte[] {Version, (byte)PackageContentType, (byte)SerializeFormat};
        }
    }
}