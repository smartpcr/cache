// -----------------------------------------------------------------------
// <copyright file="DefaultSerializer.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Common.Cache.Serialization
{
    using System.Threading;
    using System.Threading.Tasks;
#if NET462 || NETSTANDARD2_0
    using MessagePack;
#else
    using MemoryPack;
#endif

    public class DefaultCachedItemSerializer : ICachedItemSerializer
    {
#if NET462 || NETSTANDARD2_0
        private readonly MessagePackSerializerOptions serializerOptions;

        public DefaultCachedItemSerializer(MessagePackSerializerOptions? serializerOptions)
        {
            this.serializerOptions = serializerOptions ?? MessagePackSerializerOptions.Standard
                .WithResolver(MessagePack.Resolvers.ContractlessStandardResolver.Instance);
        }
#else
        private readonly MemoryPackSerializerOptions? serializerOptions;

        public DefaultCachedItemSerializer(MemoryPackSerializerOptions? serializerOptions)
        {
            this.serializerOptions = serializerOptions;
        }
#endif

        public byte[]? Serialize<T>(T? item)
        {
            if (item == null)
            {
                return null;
            }

#if NET462 || NETSTANDARD2_0
            return MessagePackSerializer.Serialize(item, this.serializerOptions);
#else
            var buffer = new PoolBufferWriter();
            MemoryPackWriter<PoolBufferWriter> writer = new(ref buffer, MemoryPackWriterOptionalStatePool.Rent(this.serializerOptions));
            try
            {
                MemoryPackSerializer.Serialize(ref writer, item);
                return buffer.ToArray();
            }
            finally
            {
                buffer.Dispose();
            }
#endif
        }

        public T? Deserialize<T>(byte[]? value)
        {
            if (value == null)
            {
                return default;
            }

#if NET462 || NETSTANDARD2_0
            return MessagePackSerializer.Deserialize<T>(value, this.serializerOptions)!;
#else
            return MemoryPackSerializer.Deserialize<T?>(value, this.serializerOptions)!;
#endif
        }

        public ValueTask<byte[]?> SerializeAsync<T>(T? obj, CancellationToken token = default)
        {
            return new ValueTask<byte[]?>(this.Serialize(obj));
        }

        public ValueTask<T?> DeserializeAsync<T>(byte[]? data, CancellationToken token = default)
        {
            return new ValueTask<T?>(this.Deserialize<T>(data));
        }
    }
}