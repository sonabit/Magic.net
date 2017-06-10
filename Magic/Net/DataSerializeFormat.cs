using JetBrains.Annotations;

namespace Magic.Net
{
    [PublicAPI]
    public enum DataSerializeFormat : byte
    {
        Magic = 0,
        MsXml = 1,
        MsBinary = 2,
        Json = 3,

        Custom = byte.MaxValue,
    }
}