using System.IO;
using System.Text;
using NUnit.Framework;

namespace NUnit.MagicTest
{
    [TestFixture]
    public class MagicIoExtensionCalcHashTests
    {
        [Test]
        public void Is_empty_Md5_calculation_valid()
        {
            const string expectedHash = "d41d8cd98f00b204e9800998ecf8427e";
            using (MemoryStream mem = new MemoryStream())
            using (StreamWriter text = new StreamWriter(mem))
            {
                text.WriteLine("");

                Assert.AreEqual(expectedHash, mem.Md5Hash());
                Assert.AreEqual(0, mem.Position);
            }

        }
        
        [Test]
        public void Is_Md5_calculation_valid()
        {
            const string expectedHash = "818e0aceccf335b27f953706cb53ebfa";
            using (MemoryStream mem = new MemoryStream(1))
            using (var text = new StreamWriter(mem))
            {
                text.Write("Hunger. Stufe für Stufe schob sie sich die Treppe hinauf. Pizza Funghi Salami, Sternchen Salami gleich Blockwurst. Die P");
                text.Flush();
                mem.Position = 0;
                Assert.AreEqual(expectedHash, mem.Md5Hash());
                Assert.AreEqual(121, mem.Position);
            }

        }
    }
}