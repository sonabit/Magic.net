using System;

namespace Magic.Net.Data
{
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