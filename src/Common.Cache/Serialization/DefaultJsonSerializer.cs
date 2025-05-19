// -----------------------------------------------------------------------
// <copyright file="DefaultJsonSerializer.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Common.Cache.Serialization
{
    using System.Buffers;
    using System.Threading;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    public class DefaultJsonSerializer<T> : ICachedItemSerializer<T>
    {
        public void Serialize(T value, IBufferWriter<byte> target)
        {
            var json = JsonConvert.SerializeObject(value);
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            target.Write(bytes);
        }

        public byte[] Serialize(T obj)
        {
            var json = JsonConvert.SerializeObject(obj);
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            return bytes;
        }

        public ValueTask<byte[]> SerializeAsync(T obj, CancellationToken token = default)
        {
            var json = JsonConvert.SerializeObject(obj);
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            return new ValueTask<byte[]>(bytes);
        }

        public T Deserialize(ReadOnlySequence<byte> source)
        {
            var json = System.Text.Encoding.UTF8.GetString(source.ToArray());
            var item = JsonConvert.DeserializeObject<T>(json);
            return item;
        }

        public T Deserialize(byte[] data)
        {
            var json = System.Text.Encoding.UTF8.GetString(data);
            var item = JsonConvert.DeserializeObject<T>(json);
            return item;
        }

        public ValueTask<T> DeserializeAsync(byte[] data, CancellationToken token = default)
        {
            var json = System.Text.Encoding.UTF8.GetString(data);
            var item = JsonConvert.DeserializeObject<T>(json);
            return new ValueTask<T>(item);
        }
    }
}