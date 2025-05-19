// -----------------------------------------------------------------------
// <copyright file="CacheWrapper.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Common.Cache.Tests.Steps
{
    using System;
    using System.Buffers;
    using System.Threading.Tasks;
    using Common.Cache.Serialization;
    using FluentAssertions;
    using Microsoft.Extensions.Caching.Distributed;
    using Microsoft.Extensions.Caching.Hybrid;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.DependencyInjection;
    using Reqnroll;

    public class CacheWrapper
    {
        private readonly CacheProviderType cacheProviderType;
        private readonly IServiceProvider serviceProvider;
        private readonly byte[]? nullValue = null;

        public CacheWrapper(ScenarioContext context, CacheProviderType cacheProviderType)
        {
            this.cacheProviderType = cacheProviderType;
            this.serviceProvider = context.Get<IServiceCollection>().BuildServiceProvider();
        }

        public async Task<byte[]?> GetAsync(string key)
        {
            byte[]? result = null;
            switch (this.cacheProviderType)
            {
                case CacheProviderType.Memory:
                    var memoryCache = this.serviceProvider.GetRequiredService<IMemoryCache>();
                    result = memoryCache.Get<byte[]>(key);
                    break;
                case CacheProviderType.Csv:
                case CacheProviderType.WindowsRegistry:
                    var distributedCache = this.serviceProvider.GetRequiredService<IDistributedCache>();
                    result = await distributedCache.GetAsync(key);
                    break;
                case CacheProviderType.Hybrid:
                    var hybridCache = this.serviceProvider.GetRequiredService<HybridCache>();
                    result = await hybridCache.GetOrCreateAsync<byte[]>(key, null);
                    break;
            }

            return result;
        }

        public async Task<T?> GetAsync<T>(string key) where T : class, new()
        {
            T? result = null;
            var serializerFactory = new SerializerFactory(SerializerType.Binary);
            var created = serializerFactory.TryCreateSerializer<T>(out var serializer);
            created.Should().BeTrue();
            switch (this.cacheProviderType)
            {
                case CacheProviderType.Memory:
                    var memoryCache = this.serviceProvider.GetRequiredService<IMemoryCache>();
                    result = memoryCache.Get<T>(key);
                    break;
                case CacheProviderType.Csv:
                case CacheProviderType.WindowsRegistry:
                    var distributedCache = this.serviceProvider.GetRequiredService<IDistributedCache>();
                    var stored = await distributedCache.GetAsync(key);
                    result = serializer.Deserialize(new ReadOnlySequence<byte>(stored));
                    break;
                case CacheProviderType.Hybrid:
                    var hybridCache = this.serviceProvider.GetRequiredService<HybridCache>();
                    result = await hybridCache.GetOrCreateAsync<T>(key, _ => new ValueTask<T>(default(T)));
                    break;
            }

            return result;
        }

        public async Task SetAsync(string key, byte[] value, TimeSpan ttl)
        {
            switch (this.cacheProviderType)
            {
                case CacheProviderType.Memory:
                    var memoryCache = this.serviceProvider.GetRequiredService<IMemoryCache>();
                    memoryCache.Set(key, value, new MemoryCacheEntryOptions()
                    {
                        AbsoluteExpirationRelativeToNow = ttl
                    });
                    break;
                case CacheProviderType.Csv:
                case CacheProviderType.WindowsRegistry:
                    var distributedCache = this.serviceProvider.GetRequiredService<IDistributedCache>();
                    await distributedCache.SetAsync(key, value, new DistributedCacheEntryOptions()
                    {
                        AbsoluteExpirationRelativeToNow = ttl
                    });
                    break;
                case CacheProviderType.Hybrid:
                    var hybridCache = this.serviceProvider.GetRequiredService<HybridCache>();
                    await hybridCache.SetAsync(key, value, new HybridCacheEntryOptions()
                    {
                        Expiration = ttl
                    });
                    break;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan ttl)
        {
            var serializerFactory = new SerializerFactory(SerializerType.Binary);
            var created = serializerFactory.TryCreateSerializer<T>(out var serializer);
            created.Should().BeTrue();
            using var writeBuffer = new PoolBufferWriter();

            switch (this.cacheProviderType)
            {
                case CacheProviderType.Memory:
                    var memoryCache = this.serviceProvider.GetRequiredService<IMemoryCache>();
                    memoryCache.Set(key, value, new MemoryCacheEntryOptions()
                    {
                        AbsoluteExpirationRelativeToNow = ttl
                    });
                    break;
                case CacheProviderType.Csv:
                case CacheProviderType.WindowsRegistry:
                    var distributedCache = this.serviceProvider.GetRequiredService<IDistributedCache>();
                    serializer.Serialize(value, writeBuffer);
                    var bytes = writeBuffer.ToArray();
                    await distributedCache.SetAsync(key, bytes, new DistributedCacheEntryOptions()
                    {
                        AbsoluteExpirationRelativeToNow = ttl
                    });
                    break;
                case CacheProviderType.Hybrid:
                    var hybridCache = this.serviceProvider.GetRequiredService<HybridCache>();
                    await hybridCache.SetAsync(key, value, new HybridCacheEntryOptions()
                    {
                        Expiration = ttl
                    });
                    break;
            }
        }

        public async Task RemoveAsync(string key)
        {
            switch (this.cacheProviderType)
            {
                case CacheProviderType.Memory:
                    var memoryCache = this.serviceProvider.GetRequiredService<IMemoryCache>();
                    memoryCache.Remove(key);
                    break;
                case CacheProviderType.Csv:
                case CacheProviderType.WindowsRegistry:
                    var distributedCache = this.serviceProvider.GetRequiredService<IDistributedCache>();
                    await distributedCache.RemoveAsync(key);
                    break;
                case CacheProviderType.Hybrid:
                    var hybridCache = this.serviceProvider.GetRequiredService<HybridCache>();
                    await hybridCache.RemoveAsync(key);
                    break;
            }
        }
    }
}