namespace Magic.Net
{
    public abstract class HubRemoteService
    {
        #region Properties
        
        public INetConnection Connection { get; internal set; }

        #endregion Properties
    }
}