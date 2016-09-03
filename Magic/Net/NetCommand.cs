using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using Magic.Net;
using Magic.Serialization;

namespace Magic.Net
{
    [Serializable]
    public sealed class NetCommand : INetCommand, IMagicSerialization
    {
        #region Implementation of IMagicSerialization<NetCommand>

        public void Deserialize(Stream stream)
        {
            return;
        }

        public void Serialize(Stream stream)
        {
            return;
        }

        #endregion  Implementation of IMagicSerialization<NetCommand>
    }

    public interface INetCommand
    {

    }
}
