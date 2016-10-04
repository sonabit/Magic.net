using System;
using System.ServiceModel.Channels;
using JetBrains.Annotations;
using Magic.Net.Server;

namespace Magic.Net
{
    public interface ISystem
    {
        string SystemName { get; }

        BufferManager BufferManager { get; }

        IDataPackageHandler PackageHandler { get; }

        void AddConnection([NotNull] INetConnection connection);
    }
}