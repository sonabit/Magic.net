using System;
using JetBrains.Annotations;
using Magic.Net.Data;
using Magic.Serialization;

namespace Magic.Net
{
    internal interface IObjectStreamManager
    {

        ObjectObservableSender CreateSender(Uri remoteAddress, Guid streamId);

        ISerializeFormatterCollection FormatterCollection { get; }

        [CanBeNull]
        ObjectStream GetObjectStream(Uri remoteAddress);
    }
}