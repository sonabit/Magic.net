namespace Magic.Net
{
    // ReSharper disable once InconsistentNaming
    internal interface _ISender
    {
        void Send(object package);
        IObjectStreamManager ObjectStreamManager { get; }
    }
}