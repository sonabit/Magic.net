using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using JetBrains.Annotations;
using Magic.Net;
using Magic.Serialization;

namespace Magic.Net
{
    [Serializable]
    public sealed class NetCommand : INetCommand//, IMagicSerialization
    {
        private readonly MethodInfo _methodName;
        private readonly ParameterInfo[] _parameterInfos;
        private readonly object[] _parameterValues;
        private readonly Type _serviceType;

        [UsedImplicitly]
        public NetCommand()
        {
            // used for serialization
        }

        public NetCommand(Type serviceType, MethodInfo methodName, ParameterInfo[] parameterInfos, object[] parameterValues)
        {
            this._serviceType = serviceType;
            this._methodName = methodName;
            this._parameterInfos = parameterInfos;
            this._parameterValues = parameterValues;
        }

        #region Implementation of INetCommand

        public Type ServiceType
        {
            get { return _serviceType; }
        }

        public MethodInfo MethodName
        {
            get { return _methodName; }
        }

        public ParameterInfo[] ParameterInfos
        {
            get { return _parameterInfos; }
        }

        public object[] ParameterValues
        {
            get { return _parameterValues; }
        }

        #endregion Implementation of INetCommand

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
}
