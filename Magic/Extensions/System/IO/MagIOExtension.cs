using System.Security.Cryptography;

namespace System.IO
{
    public static class MagIOExtension
    {
        /// <summary>
        /// Get the MD5 hash value of a file as a HEX-string
        /// </summary>
        /// <param name="fileStream">The input stream for the hash code is to be calculated.</param>
        /// <returns>The calculated hash code as Hex-string</returns>
        public static string Md5Hash(this Stream stream)
        {
            using (MD5 md5 = MD5.Create())
            {
                return md5.ComputeHash(stream).ToHexString();
            }
        }
    }
}
 