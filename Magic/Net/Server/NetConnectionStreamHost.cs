namespace Magic.Net.Server
{
    internal class NetConnectionStreamHost : NetConnection
    {
        private readonly INetConnectionAdapter _adapter;

        public NetConnectionStreamHost(INetConnectionAdapter adapter)
        {
            this._adapter = adapter;
        }
        #region Overrides of NetConnection

        protected override INetConnectionAdapter CreateAdapter(ISystem system)
        {
            return _adapter;
        }

        #endregion
    }
}