namespace Magic.Net
{
    /// <summary>
    ///     Specifies a data package of NetConnection
    /// </summary>
    public enum DataPackageContentType : byte
    {
        NetCommand = 0x1,
        NetCommandStream = 0x10
    }
}