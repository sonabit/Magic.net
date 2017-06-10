using System.Threading;
using Magic.Controls.Converter;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Magic.ControlsTest
{
    [TestClass]
    public class ReadableFilesizeConverterTests
    {
        [TestMethod]
        public void Convert2GbOk()
        {
            const int value = int.MaxValue;

            ReadableFilesizeConverter testConverter = new ReadableFilesizeConverter();

            Assert.AreEqual("2 GB", testConverter.Convert(value, typeof(string), null, Thread.CurrentThread.CurrentCulture));

        }
    }
}
