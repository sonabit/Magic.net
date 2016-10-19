using System.Security.Cryptography;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace System.IO
{
    [PublicAPI]
    // ReSharper disable once InconsistentNaming
    public static class MagIOExtension
    {
        /// <summary>
        /// Get the MD5 hash value of a file as a HEX-string
        /// </summary>
        /// <param name="fileStream">The input file for the hash code is to be calculated.</param>
        /// <returns>The calculated hash code as Hex-string</returns>
        public static string Md5Hash(this FileStream fileStream)
        {
            using (MD5 md5 = MD5.Create())
            {
                return md5.ComputeHash(fileStream).ToHexString();
            }
        }
    }
}
 