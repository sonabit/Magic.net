using System;

namespace Magic.Net
{
    [Flags]
    public enum NetExceptionReasonses : int
    {
        UnknownPackageContentType,
        PackeContentTypeRejected,
        NoConnectionFound
    }
}