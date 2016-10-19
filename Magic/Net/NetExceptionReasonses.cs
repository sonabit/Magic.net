using System;

namespace Magic.Net
{
    [Flags]
    public enum NetExceptionReasonses
    {
        UnknownPackageContentType,
        PackeContentTypeRejected,
        NoConnectionFound
    }
}