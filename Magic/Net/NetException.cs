using System;

namespace Magic.Net
{
    public class NetException : Exception
    {
        private readonly NetExceptionReasonses _reasonses;
        
        public NetException(NetExceptionReasonses reasonses, string message)
            :base(message)
        {
            this._reasonses = reasonses;
        }

        public NetExceptionReasonses Reasonses
        {
            get
            {
                return _reasonses;
            }
        }
    }
}