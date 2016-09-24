using Magic.Net;
using NUnit.Framework;

namespace NUnit.Magic.Net.Test
{
    [TestFixture]
    public class NetCommandPackageTests
    {
        [Test]
        public void Create_NetCommandPackage()
        {
            var bytes = new byte[] {2, 1, 0, 0, 0,};
            NetDataPackage package = new NetDataPackage(bytes);
            Assert.AreEqual(2, package.Version);
            Assert.AreEqual(DataPackageContentType.NetCommand, package.PackageContentType);
            Assert.AreEqual(2, package.Buffer.Count);
            Assert.AreEqual(bytes, package.Buffer.Array);
        }
    }
}