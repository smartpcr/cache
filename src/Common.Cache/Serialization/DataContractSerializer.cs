// -----------------------------------------------------------------------
// <copyright file="DataContractSerializer.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Common.Cache.Serialization
{
    using System.Buffers;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;

    public class DataContractSerializer<T> : ICachedItemSerializer<T>
    {
        public void Serialize(T value, IBufferWriter<byte> target)
        {
            var serializer = new DataContractSerializer(typeof(T));
            string xml;

            // Serialize the person instance to XML.
            using (var memoryStream = new MemoryStream())
            {
                using (var writer = XmlDictionaryWriter.CreateTextWriter(memoryStream))
                {
                    serializer.WriteObject(writer, value);
                }
                xml = System.Text.Encoding.UTF8.GetString(memoryStream.ToArray());
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(xml);
            target.Write(bytes);
        }

        public byte[] Serialize(T obj)
        {
            var serializer = new DataContractSerializer(typeof(T));
            string xml;
            using (var memoryStream = new MemoryStream())
            {
                using (var writer = XmlDictionaryWriter.CreateTextWriter(memoryStream))
                {
                    serializer.WriteObject(writer, obj);
                }
                xml = System.Text.Encoding.UTF8.GetString(memoryStream.ToArray());
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(xml);
            return bytes;
        }

        public ValueTask<byte[]> SerializeAsync(T obj, CancellationToken token = default)
        {
            var serializer = new DataContractSerializer(typeof(T));
            string xml;
            using (var memoryStream = new MemoryStream())
            {
                using (var writer = XmlDictionaryWriter.CreateTextWriter(memoryStream))
                {
                    serializer.WriteObject(writer, obj);
                }
                xml = System.Text.Encoding.UTF8.GetString(memoryStream.ToArray());
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(xml);
            return new ValueTask<byte[]>(bytes);
        }

        public T Deserialize(ReadOnlySequence<byte> source)
        {
            var serializer = new DataContractSerializer(typeof(T));
            var buffer = new byte[source.Length];
            source.Slice(0, source.Length).CopyTo(buffer);
            using var memoryStream = new MemoryStream(buffer);
            using var reader = XmlDictionaryReader.CreateTextReader(memoryStream, new XmlDictionaryReaderQuotas());
            T item = (T)serializer.ReadObject(reader);
            return item;
        }

        public T Deserialize(byte[] data)
        {
            var serializer = new DataContractSerializer(typeof(T));
            using var memoryStream = new MemoryStream(data);
            using var reader = XmlDictionaryReader.CreateTextReader(memoryStream, new XmlDictionaryReaderQuotas());
            T item = (T)serializer.ReadObject(reader);
            return item;
        }

        public ValueTask<T> DeserializeAsync(byte[] data, CancellationToken token = default)
        {
            var serializer = new DataContractSerializer(typeof(T));
            using var memoryStream = new MemoryStream(data);
            using var reader = XmlDictionaryReader.CreateTextReader(memoryStream, new XmlDictionaryReaderQuotas());
            T item = (T)serializer.ReadObject(reader);
            return new ValueTask<T>(item);
        }
    }
}