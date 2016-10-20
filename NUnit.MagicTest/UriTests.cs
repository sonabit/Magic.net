using System;
using Magic.Net;
using NUnit.Framework;

namespace NUnit.MagicTest
{
    [TestFixture]
    public class UriExtensionTests
    {
        [Test]
        public void GetStringOfSegmentShouldByOkay()
        {
            var uri = new Uri("test://horst:23/seg1");
            string s = uri.GetStringOfSegment(1);

            Assert.AreEqual("seg1", s);
        }
    }
}