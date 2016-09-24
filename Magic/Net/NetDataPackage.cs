using System;
using JetBrains.Annotations;

namespace Magic.Net
{
    public class NetDataPackage
    {
        #region Header

        [NotNull]
        private readonly ArraySegment<byte> _buffer;
        [NotNull]
        private readonly NetDataPackageHeader _header;

        #endregion

        public NetDataPackage(byte[] buffer)
        {
            _header = new NetDataPackageHeader(buffer);
            _buffer = new ArraySegment<byte>(buffer, _header.ByteLength, buffer.Length - _header.ByteLength);
        }
        
        public ArraySegment<byte> Buffer
        {
            get { return _buffer; }
        }

        public bool IsEmpty { get { return Buffer.Count == 0; } }

        public byte Version { get { return _header.Version; } }

        public DataPackageContentType PackageContentType { get { return _header.PackageContentType; } }

        public DataSerializeFormat SerializeFormat { get { return _header.SerializeFormat; } }

    }

    class NetDataPackageHeader
    {
        private readonly int _byteLength;

        public NetDataPackageHeader(byte[] buffer)
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
            _byteLength = len;
        }

        internal int ByteLength
        {
            get { return _byteLength; }
        }

        public byte Version { get; private set; }

        public DataPackageContentType PackageContentType { get; private set; }

        public DataSerializeFormat SerializeFormat { get; private set; }
    }
}