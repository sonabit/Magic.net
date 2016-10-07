namespace Magic.Net.Data
{
    public sealed class RequestState
    {
        private readonly INetConnection _connetion;
        private readonly NetOjectPackage _package;

        internal RequestState(INetConnection connection, NetOjectPackage package)
        {
            _connetion = connection;
            _package = package;
        }

        public INetConnection Connetion { get { return _connetion; } }
        internal NetOjectPackage Package { get { return _package; } }
    }
}