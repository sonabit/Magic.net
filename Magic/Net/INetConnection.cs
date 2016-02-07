namespace Magic.Net
{
    public interface INetConnection
    {
        bool IsConnected { get; }

        void Connect();
    }
}