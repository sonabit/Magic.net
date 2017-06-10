using System;
using System.Collections.Generic;
using Magic.Net.Data;
using Magic.Serialization;

namespace Magic.Net
{
    internal sealed class ObjectStreamManager : IObjectStreamManager
    {
        private readonly Dictionary<Uri, ObjectStream> _objectStreams = new Dictionary<Uri, ObjectStream>();

        internal ObjectStreamManager(ISerializeFormatterCollection formatterCollection)
        {
            FormatterCollection = formatterCollection;
        }

        public void Add(ObjectStream objectStream)
        {
            _objectStreams.Add(objectStream.RemoteAddress, objectStream);
        }

        #region Implementation of IObjectStreamManager

        public ObjectObservableSender CreateSender(Uri remoteAddress, Guid streamId)
        {
            throw new NotImplementedException();
        }

        public ISerializeFormatterCollection FormatterCollection { get; private set; }

        public ObjectStream GetObjectStream(Uri remoteAddress)
        {
            ObjectStream result;
            _objectStreams.TryGetValue(remoteAddress, out result);
            return result;
        }

        #endregion
    }
}