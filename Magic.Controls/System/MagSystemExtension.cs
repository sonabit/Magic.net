using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        public static string ToHexString(this IEnumerable<byte> bytes)
        {
            if (bytes == null) throw new ArgumentNullException("bytes");

            var result = new StringBuilder(bytes.Count() * 2);
            foreach (var b in bytes) result.AppendFormat("{0:x2}", b);
            return result.ToString();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="hexstring"></param>
        /// <returns></returns>
        public static byte[] FromHexString(this String hexstring)
        {
            int NumberChars = hexstring.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hexstring.Substring(i, 2), 16);
            return bytes;
        }

        /// <summary>
        /// Convert an array of bytes to a Base64 string
        /// </summary>
        /// <remarks>This function use <see cref="Convert.ToBase64String(byte[])"/></remarks>
        /// <param name="bytes">The array of bytes to convert</param>
        /// <returns>A string with converted bytes in base64 format</returns>
        public static string ToBase64(this byte[] inArray)
        {
            return Convert.ToBase64String(inArray);
        }
    }
}