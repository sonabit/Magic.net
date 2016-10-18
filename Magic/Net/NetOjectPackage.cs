using System;
using JetBrains.Annotations;
using Magic.Net.Data;

namespace Magic.Net
{
    internal sealed class NetOjectPackage : NetPackage
    {
        private readonly NetDataPackageHeader _header;
        private readonly object _data;

        public NetOjectPackage([NotNull] NetDataPackageHeader header, [NotNull] object data)
        {
            if (header == null) throw new ArgumentNullException("header");
            if (data == null) throw new ArgumentNullException("data");

            _header = header;
            _data = data;
        }

        public override bool IsEmpty { get { return false; } }

        public override byte Version { get { return _header.Version; } }

        public override DataPackageContentType PackageContentType { get { return _header.PackageContentType; } }
        
        public object Data { get { return _data; } }
    }
}