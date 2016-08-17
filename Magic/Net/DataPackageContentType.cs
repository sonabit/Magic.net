namespace Magic.Net
{
    /// <summary>
    ///     Specifies a data package of NetConnection
    /// </summary>
    public enum DataPackageContentType : byte
    {
        NetCommand = 1,
        NetCommandStream = 10
    }
}