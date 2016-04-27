using System;

namespace Magic.Net
{
    public class NetCommandException : Exception
    {
        private readonly NetCommandExceptionReasonses _reasonses;
        
        public NetCommandException(NetCommandExceptionReasonses reasonses, string message)
            :base(message)
        {
            this._reasonses = reasonses;
        }

        public NetCommandExceptionReasonses Reasonses
        {
            get
            {
                return _reasonses;
            }
        }
    }
}