using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using Magic.Net;

namespace Magic.Serialization
{
    public interface IMagicSerialization
    {
        void Deserialize(Stream stream);

        void Serialize(Stream stream);
    }

    internal interface ISerializeFormatterCollection
    {
        ISerializeFormatter this[DataSerializeFormat index] { get; }
    }

    internal interface ISerializeFormatter
    {
        TResult Deserialize<TResult>(byte[] bytes, long startPosition);
        byte[] Serialize<TResult>(TResult value);
    }

    internal sealed class MagicSerializeFormatter : ISerializeFormatter
    {
        #region Implementation of ISerializeFormatter

        public TResult Deserialize<TResult>(byte[] bytes, long startPosition)
        {
            TResult result = Activator.CreateInstance<TResult>();

            IMagicSerialization magicSerialization = result as IMagicSerialization;
            if (magicSerialization == null)
                throw new SerializationException(
                    string.Format("The type {0} does not support the interface {1}", typeof(TResult).Name,
                        typeof(IMagicSerialization).Name));
            using (MemoryStream mStream = new MemoryStream(bytes, false))
            {
                mStream.Seek(startPosition, SeekOrigin.Begin);

                magicSerialization.Deserialize(mStream);
            }
            return result;
        }

        public byte[] Serialize<TResult>(TResult value)
        {
            IMagicSerialization magicSerialization = value as IMagicSerialization;
            if (magicSerialization == null)
                throw new SerializationException(string.Format("The type {0} does not support the interface {1}",
                    typeof(TResult).Name, typeof(IMagicSerialization).Name));

            using (MemoryStream mStream = new MemoryStream())
            {
                magicSerialization.Serialize(mStream);
                return mStream.ToArray();
            }
        }

        #endregion
    }

    sealed class DefaulSerializeFormatter : ISerializeFormatterCollection
    {
        Dictionary<DataSerializeFormat, ISerializeFormatter> _formatters = new Dictionary<DataSerializeFormat, ISerializeFormatter>();

        public DefaulSerializeFormatter()
        {
            _formatters[DataSerializeFormat.Magic] = new MagicSerializeFormatter();
            _formatters[DataSerializeFormat.MsXml] = new XmlFormatter();
            _formatters[DataSerializeFormat.MsBinary] = new GFormatter<BinaryFormatter>();
        }

        public ISerializeFormatter this[DataSerializeFormat index]
        {
            get { return _formatters[index]; }
        }
    }

    internal class GFormatter<TFormatter> : ISerializeFormatter
         where TFormatter : IFormatter, new()
    {
        private readonly Dictionary<Type, IFormatter> _serializers = new Dictionary<Type, IFormatter>();

        public TResult Deserialize<TResult>(byte[] bytes, long startPosition)
        {
            IFormatter serializer;
            if (!_serializers.TryGetValue(typeof(TResult), out serializer))
            {
                serializer = new TFormatter();
                _serializers[typeof(TResult)] = serializer;
            }
            using (var stream = new MemoryStream(bytes, false))
            {
                stream.Position = startPosition;
                return (TResult)serializer.Deserialize(stream);
            }
        }

        public byte[] Serialize<T>(T value)
        {
            IFormatter serializer;
            if (!_serializers.TryGetValue(typeof(T), out serializer))
            {
                serializer = new TFormatter();
                _serializers[typeof(T)] = serializer;
            }
            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream, value);
                return stream.ToArray();
            }
        }
    }

    internal class XmlFormatter : ISerializeFormatter
    {
        readonly Dictionary<Type, XmlSerializer> _serializers = new Dictionary<Type, XmlSerializer>();

        public TResult Deserialize<TResult>(byte[] bytes, long startPosition)
        {
            XmlSerializer serializer;
            if (!_serializers.TryGetValue(typeof(TResult), out serializer))
            {
                serializer = new XmlSerializer(typeof(TResult));
                _serializers[typeof(TResult)] = serializer;
            }
            using (var stream = new MemoryStream(bytes, false))
            {
                stream.Position = startPosition;
                return (TResult)serializer.Deserialize(stream);
            }
        }

        public byte[] Serialize<T>(T value)
        {
            XmlSerializer serializer;
            if (!_serializers.TryGetValue(typeof(T), out serializer))
            {
                serializer = new XmlSerializer(typeof(T));
                _serializers[typeof(T)] = serializer;
            }
            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream, value);
                return stream.ToArray();
            }
        }
    }
}