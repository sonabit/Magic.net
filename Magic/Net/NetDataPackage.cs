using System;
using System.Collections.Generic;
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

        public NetDataPackage([NotNull] NetDataPackageHeader header, [NotNull] byte[] payLoad, int offset, int count)
        {
            if (header == null) throw new ArgumentNullException("header");
            if (payLoad == null) throw new ArgumentNullException("payLoad");

            _header = header;
            _buffer = new ArraySegment<byte>(payLoad, offset, count);
        }

        public NetDataPackage(byte[] buffer)
        {
            _header = new NetDataPackageHeader(ref buffer);
            _buffer = new ArraySegment<byte>(buffer, _header.HeaderLength, buffer.Length - _header.HeaderLength);
        }
        
        public ArraySegment<byte> Buffer
        {
            get { return _buffer; }
        }

        public bool IsEmpty { get { return Buffer.Count == 0; } }

        public byte Version { get { return _header.Version; } }

        public DataPackageContentType PackageContentType { get { return _header.PackageContentType; } }

        public DataSerializeFormat SerializeFormat { get { return _header.SerializeFormat; } }

        public IEnumerable<ArraySegment<byte>> DataSegments()
        {
            yield return new ArraySegment<byte>(_header.ToBytes());
            yield return Buffer;
        } 

    }
}