using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magic.Serialization
{
    public static class MagicSerializationExtensions
    {
        public static byte[] ToBytes(this IMagicSerialization serialization)
        {
            using (var ms = new MemoryStream())
            {
                serialization.Deserialize(ms);
                return ms.ToArray();
            }
        }
    }
}
