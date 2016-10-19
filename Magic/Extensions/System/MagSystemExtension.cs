using System.Linq;
using System.Text;

// ReSharper disable once CheckNamespace
namespace System
{
    public static class MagSystemExtension
    {
        /// <summary>
        /// Returns the ticks from 1-1-1970
        /// </summary>
        /// <param name="date">Date to convert to Unix ticks</param>
        /// <returns>UnixTimestamp in Int64</returns>
        public static long UnixTimestampFromDateTime(this DateTime date)
        {
            long unixTimestamp = date.Ticks - new DateTime(1970, 1, 1).Ticks;
            unixTimestamp /= TimeSpan.TicksPerSecond;
            return unixTimestamp;
        }

        /// <summary>
        /// Returns a Datetime from unixTimestamp based of 1-1-1970
        /// </summary>
        /// <param name="unixTimestamp">UnixTimestamp to Convert into Datetime </param>
        /// <returns>Converted Datetime</returns>
        public static DateTime TimeFromUnixTimestamp(this long unixTimestamp)
        {
            DateTime unixYear0 = new DateTime(1970, 1, 1);
            long unixTimeStampInTicks = unixTimestamp * TimeSpan.TicksPerSecond;
            DateTime dtUnix = new DateTime(unixYear0.Ticks + unixTimeStampInTicks);
            return dtUnix;
        }

        /// <summary>
        /// Convert the bytes to a HEX string
        /// </summary>
        /// <param name="bytes">bytes to convert</param>
        /// <returns>A Hex String</returns>
        public static string ToHexString(this byte[] bytes)
        {
            if (bytes == null) throw new ArgumentNullException("bytes");

            var result = new StringBuilder(bytes.Count() * 2);
            foreach (var b in bytes) result.AppendFormat("{0:x2}", b);
            return result.ToString();
        }


        /// <summary>
        /// Convert HEX string to an array of byte
        /// </summary>
        /// <param name="hexstring"></param>
        /// <returns></returns>
        public static byte[] FromHexString(this string hexstring)
        {
            int numberChars = hexstring.Length;
            byte[] bytes = new byte[numberChars / 2];
            for (int i = 0; i < numberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hexstring.Substring(i, 2), 16);
            return bytes;
        }

        /// <summary>
        /// Convert an array of bytes to a Base64 string
        /// </summary>
        /// <remarks>This function use <see cref="Convert.ToBase64String(byte[])"/></remarks>
        /// <param name="source">The array of bytes to convert</param>
        /// <returns>A string with converted bytes in base64 format</returns>
        public static string ToBase64(this byte[] source)
        {
            return Convert.ToBase64String(source);
        }

        /// <summary>
        /// Reads a string from an array of byte until a zero byte (byte.MinValue)
        /// </summary>
        /// <param name="source">An array of byte</param>
        /// <param name="result">A string variable, where result string is returning. For encoding UTF8 will used.</param>
        /// <returns>The count of read bytes</returns>
        public static int ReadStringNullTerminated(this byte[] source, out string result)
        {
            return ReadStringNullTerminated(source, 0, out result);
        }

        /// <summary>
        /// Reads a string from an array of byte until a zero byte (byte.MinValue)
        /// </summary>
        /// <param name="source">An array of byte</param>
        /// <param name="offset">the count of bytes to ignore</param>
        /// <param name="result">A string variable, where result string is returning. For encoding UTF8 will used.</param>
        /// <returns>The count of read bytes</returns>
        public static int ReadStringNullTerminated(this byte[] source, int offset, out string result)
        {
            return ReadStringNullTerminated(source, offset, out result, Encoding.UTF8);
        }

        /// <summary>
        /// Reads a string from an array of byte until a zero byte (byte.MinValue)
        /// </summary>
        /// <param name="source">An array of byte</param>
        /// <param name="offset">the count of bytes to ignore</param>
        /// <param name="result">A string variable, where result string is returning. For encoding UTF8 will used.</param>
        /// <param name="encoding">This encoding will used for converting</param>
        /// <returns>The count of read bytes</returns>
        public static int ReadStringNullTerminated(this byte[] source, int offset, out string result, Encoding encoding)
        {
            var len = Array.FindIndex(source, (int)offset, b => b == byte.MinValue) - (int)offset;
            result = encoding.GetString(source, offset, len);
            return len + 1/*null-byte*/;
        }
    }
}