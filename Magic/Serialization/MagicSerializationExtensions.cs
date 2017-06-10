using System.IO;

namespace Magic.Serialization
{
    public static class MagicSerializationExtensions
    {
        public static byte[] ToBytes(this IMagicSerialization serialization)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                serialization.Deserialize(ms);
                return ms.ToArray();
            }
        }
    }
}