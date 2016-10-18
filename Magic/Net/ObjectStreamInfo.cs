using System;

namespace Magic.Net.Data
{
    [Serializable]
    internal sealed class ObjectStreamInfo
    {
        public Guid StreamId { get; private set; }
        public ObjectStreamState State { get; set; }
    }
}