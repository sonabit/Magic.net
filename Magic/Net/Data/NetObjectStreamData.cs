using System;

namespace Magic.Net.Data
{
    public sealed class NetObjectStreamData
    {
        public NetObjectStreamData(Guid id, object data)
        {
            Id = id;
            Data = data;
        }

        public Guid Id { get; private set; }

        public object Data { get; private set; }
    }
}