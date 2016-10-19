using System.ServiceModel.Channels;
using JetBrains.Annotations;
using Magic.Serialization;

namespace Magic.Net
{
    public interface ISystem
    {
        string SystemName { get; }

        [NotNull]
        BufferManager BufferManager { get; }

        [NotNull]
        IDataPackageHandler PackageHandler { get; }

        [NotNull]
        ISerializeFormatterCollection FormatterCollection { get; }

        void AddConnection([NotNull] INetConnection connection);
    }
}