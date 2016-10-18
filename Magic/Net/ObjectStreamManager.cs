using System;
using System.Collections.Generic;
using Magic.Net.Data;
using Magic.Serialization;

namespace Magic.Net
{
    internal sealed class ObjectStreamManager : IObjectStreamManager
    {
        private readonly Dictionary<Uri, ObjectStream> objectStreams = new Dictionary<Uri, ObjectStream>();

        internal ObjectStreamManager(ISerializeFormatterCollection formatterCollection)
        {
            FormatterCollection = formatterCollection;
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
            objectStreams.TryGetValue(remoteAddress, out result);
            return result;
        }

        #endregion

        public void Add(ObjectStream objectStream)
        {
            objectStreams.Add(objectStream.RemoteAddress, objectStream);
        }
    }
}