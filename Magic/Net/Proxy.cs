namespace Magic.Net
{
    public static class Proxy<TTarget>
    {
        public static TTarget Target
        {
            get { return default(TTarget); }
        }
    }
}