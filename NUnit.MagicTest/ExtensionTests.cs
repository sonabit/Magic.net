using System;
using System.Text;
using NUnit.Framework;

namespace NUnit.MagicTest
{
    [TestFixture]
    public class ExtensionTests
    {
        [Test]
        public void ValueTypWriteExtensionsInt32ToBuffer()
        {
            // Given
            byte[] buffer = new byte[10];
            int i = 32;

            // When
            i.ToBuffer(buffer, 6);


            // Then
            Assert.AreEqual(0, buffer[0]);
            Assert.AreEqual(0, buffer[1]);
            Assert.AreEqual(0, buffer[2]);
            Assert.AreEqual(0, buffer[3]);
            Assert.AreEqual(0, buffer[4]);
            Assert.AreEqual(0, buffer[5]);
            Assert.AreEqual(32, buffer[6]);
            Assert.AreEqual(0, buffer[7]);
            Assert.AreEqual(0, buffer[8]);
            Assert.AreEqual(0, buffer[9]);
        }

        [Test]
        public void ValueTypWriteExtensionsByteToBuffer()
        {
            // Given
            byte[] buffer = new byte[10];
            byte i = 32;

            // When
            i.ToBuffer(buffer, 9);

            // Then
            Assert.AreEqual(0, buffer[0]);
            Assert.AreEqual(0, buffer[1]);
            Assert.AreEqual(0, buffer[2]);
            Assert.AreEqual(0, buffer[3]);
            Assert.AreEqual(0, buffer[4]);
            Assert.AreEqual(0, buffer[5]);
            Assert.AreEqual(0, buffer[6]);
            Assert.AreEqual(0, buffer[7]);
            Assert.AreEqual(0, buffer[8]);
            Assert.AreEqual(32, buffer[9]);
        }

        [Test]
        public void MagSystemExtensionUnixTimestampFromDateTime()
        {
            // Given
            DateTime dateTime = new DateTime(2016, 10, 19, 13, 20, 42);

            // When
            long test = dateTime.UnixTimestampFromDateTime();

            // Then
            Assert.AreEqual(1476883242, test);
        }

        [Test]
        public void MagSystemExtensionTimeFromUnixTimestamp()
        {
            // Given
            long unixTimeStamp = 1476883242L;

            // When
            DateTime dateTime = unixTimeStamp.TimeFromUnixTimestamp();

            // Then 
            Assert.AreEqual(new DateTime(2016, 10, 19, 13, 20, 42), dateTime);
        }


        [Test]
        public void MagSystemExtensionBytesFromHexString()
        {
            // Given
            string s = "200DFF";

            // When
            byte[] bytes = s.FromHexString();

            // Then 
            Assert.AreEqual(32, bytes[0]);
            Assert.AreEqual(13, bytes[1]);
            Assert.AreEqual(255, bytes[2]);
            Assert.AreEqual(3, bytes.Length);
        }

        [Test]
        public void MagSystemExtensionStringToBase64()
        {
            // Given
            byte[] bytes = { 0x20, 0x0D, 0xFF };

            // When
            string s = bytes.ToBase64();

            // Then 
            Assert.AreEqual("IA3/", s);
        }

        [Test]
        public void MagSystemExtensionReadStringNullTerminated1()
        {
            // Given
            byte[] bytes = { 0x20, 0x0D, 0x00, 49, 32, 84, 101, 115, 116, 0, 45, 120 };

            // When
            string s;
            int readLen = bytes.ReadStringNullTerminated(3, out s);

            // Then 
            Assert.AreEqual(7, readLen);
            Assert.AreEqual("1 Test", s);
        }

        [Test]
        public void MagSystemExtensionReadStringNullTerminated2()
        {
            // Given
            byte[] bytes = { 0x20, 0x0D, 0x00, 49, 32, 84, 101, 115, 116, 0, 45, 120 };

            // When
            string s;
            int readLen = bytes.ReadStringNullTerminated(3, out s, Encoding.ASCII);

            // Then 
            Assert.AreEqual(7, readLen);
            Assert.AreEqual("1 Test", s);
        }

    }
}
