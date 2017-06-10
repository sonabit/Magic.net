using System;

namespace Magic.Net.Data
{
    [Serializable]
    internal sealed class ObjectStreamInfo
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public Guid StreamId { get; private set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public ObjectStreamState State { get; set; }
    }
}