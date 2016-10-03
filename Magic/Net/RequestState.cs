namespace Magic.Net
{
    public class RequestState
    {
        private INetConnection _connetion;
        private readonly NetDataPackage _package;

        public RequestState(INetConnection connection, NetDataPackage package)
        {
            _connetion = connection;
            _package = package;
        }

        internal INetConnection Connetion { get { return _connetion; } }
        internal NetDataPackage Package { get { return _package; } }
    }
}