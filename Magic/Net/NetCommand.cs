using System;
using System.IO;
using System.Reflection;
using JetBrains.Annotations;

namespace Magic.Net
{
    [Serializable]
    public sealed class NetCommand : INetCommand //, IMagicSerialization
    {
        [UsedImplicitly]
        public NetCommand()
        {
            // used for serialization
        }

        public NetCommand(Type serviceType, MethodInfo methodName, ParameterInfo[] parameterInfos,
            object[] parameterValues)
        {
            _id = Guid.NewGuid();
            _serviceType = serviceType;
            _methodName = methodName;
            _parameterInfos = parameterInfos;
            _parameterValues = parameterValues;
        }

        #region Fields

        private readonly Guid _id;
        private readonly MethodInfo _methodName;
        private readonly ParameterInfo[] _parameterInfos;
        private readonly object[] _parameterValues;
        private readonly Type _serviceType;

        #endregion Fields

        #region Implementation of INetCommand

        public Guid Id
        {
            get { return _id; }
        }

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
        }

        public void Serialize(Stream stream)
        {
        }

        #endregion  Implementation of IMagicSerialization<NetCommand>
    }

    [Serializable]
    public sealed class NetCommandResult
    {
        #region Fields
        private readonly Guid _commandId;
        private readonly object _result;

        #endregion Fields

        #region Ctors

        public NetCommandResult(Guid commandId, object result)
        {
            _commandId = commandId;
            _result = result;
        }

        #endregion Ctors

        public Guid CommandId
        {
            get { return _commandId; }
        }

        public object Result
        {
            get { return _result; }
        }
    }
}