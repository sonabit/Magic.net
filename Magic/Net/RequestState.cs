namespace Magic.Net
{
    public class RequestState
    {
        private readonly INetConnection _connetion;
        private readonly NetOjectPackage _package;

        public RequestState(INetConnection connection, NetOjectPackage package)
        {
            _connetion = connection;
            _package = package;
        }

        internal INetConnection Connetion { get { return _connetion; } }
        public NetOjectPackage Package { get { return _package; } }
    }
}