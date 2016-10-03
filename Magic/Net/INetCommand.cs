using System;
using System.Reflection;

namespace Magic.Net
{
    public interface INetCommand
    {
        Guid Id { get; }

        Type ServiceType { get; }

        MethodInfo MethodName { get; }

        ParameterInfo[] ParameterInfos { get; }

        object[] ParameterValues { get; }
    }
}