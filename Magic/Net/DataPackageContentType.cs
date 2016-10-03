using JetBrains.Annotations;

namespace Magic.Net
{
    /// <summary>
    ///     Specifies a data package of NetConnection
    /// </summary>
    public enum DataPackageContentType : byte
    {
        NetCommand = 1,
        NetCommandResult = 2,
        NetCommandStream = 10,
        ConnectionMetaData = 20
    }

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