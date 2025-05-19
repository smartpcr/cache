// -----------------------------------------------------------------------
// <copyright file="DefaultBinarySerializer.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Common.Cache.Serialization
{
    using System.Buffers;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Threading;
    using System.Threading.Tasks;

    public class DefaultBinarySerializer<T> : ICachedItemSerializer<T>
    {
        public void Serialize(T value, IBufferWriter<byte> target)
        {
            var stream = new MemoryStream();
            var formatter = new BinaryFormatter();
            formatter.Serialize(stream, value);
            stream.Position = 0;
            var buffer = stream.ToArray();
            target.Write(buffer);
        }

        public byte[] Serialize(T obj)
        {
            var stream = new MemoryStream();
            var formatter = new BinaryFormatter();
            formatter.Serialize(stream, obj);
            stream.Position = 0;
            var buffer = stream.ToArray();
            return buffer;
        }

        public ValueTask<byte[]> SerializeAsync(T obj, CancellationToken token = default)
        {
            var stream = new MemoryStream();
            var formatter = new BinaryFormatter();
            formatter.Serialize(stream, obj);
            stream.Position = 0;
            var buffer = stream.ToArray();
            return new ValueTask<byte[]>(buffer);
        }

        public T Deserialize(ReadOnlySequence<byte> source)
        {
            var buffer = new byte[source.Length];
            source.Slice(0, source.Length).CopyTo(buffer);
            var stream = new MemoryStream(buffer);
            var formatter = new BinaryFormatter();
            stream.Position = 0;
            var item = (T)formatter.Deserialize(stream);
            return item;
        }

        public T Deserialize(byte[] data)
        {
            var stream = new MemoryStream(data);
            var formatter = new BinaryFormatter();
            stream.Position = 0;
            var item = (T)formatter.Deserialize(stream);
            return item;
        }

        public ValueTask<T> DeserializeAsync(byte[] data, CancellationToken token = default)
        {
            var stream = new MemoryStream(data);
            var formatter = new BinaryFormatter();
            stream.Position = 0;
            var item = (T)formatter.Deserialize(stream);
            return new ValueTask<T>(item);
        }
    }
}