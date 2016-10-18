namespace Magic.Net
{
    /// <summary>
    ///     Specifies a data package of NetConnection
    /// </summary>
    public enum DataPackageContentType : byte
    {
        NetCommand = 1,
        NetCommandResult = 2,
        NetObjectStreamInitialize = 10,
        NetObjectStreamData = 12,
        NetObjectStreamClose = 11,
        ConnectionMetaData = 20
    }
}