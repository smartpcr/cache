// -----------------------------------------------------------------------
// <copyright file="MsgPackSerializer.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Common.Cache.Serialization
{
    using System.Buffers;
    using System.Threading;
    using System.Threading.Tasks;
    using MessagePack;

    public class MsgPackSerializer<T> : ICachedItemSerializer<T>
    {
        private readonly MessagePackSerializerOptions serializerOptions;

        public MsgPackSerializer()
        {
            this.serializerOptions = MessagePackSerializerOptions.Standard
                .WithResolver(MessagePack.Resolvers.ContractlessStandardResolver.Instance);
        }

        public byte[] Serialize(T item)
        {
            if (item == null)
            {
                return null;
            }

            return MessagePackSerializer.Serialize(item, this.serializerOptions);
        }

        public T Deserialize(byte[] value)
        {
            if (value == null || value.Length == 0)
            {
                return default;
            }

            return MessagePackSerializer.Deserialize<T>(value, this.serializerOptions);
        }

        public ValueTask<byte[]> SerializeAsync(T obj, CancellationToken token = default)
        {
            return new ValueTask<byte[]>(this.Serialize(obj));
        }

        public ValueTask<T> DeserializeAsync(byte[] data, CancellationToken token = default)
        {
            return new ValueTask<T>(this.Deserialize(data));
        }

        public T Deserialize(ReadOnlySequence<byte> source)
        {
            byte[] buffer = new byte[source.Length];
            source.Slice(0, source.Length).CopyTo(buffer);
            return this.Deserialize(buffer);
        }

        public void Serialize(T value, IBufferWriter<byte> target)
        {
            var data = this.Serialize(value);
            if (data != null)
            {
                target.Write(data);
            }
        }
    }
}