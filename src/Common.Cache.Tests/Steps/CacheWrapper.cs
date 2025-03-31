// -----------------------------------------------------------------------
// <copyright file="CacheWrapper.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Common.Cache.Tests.Steps
{
    using System;
    using System.Threading.Tasks;
    using Common.Cache.Serialization;
    using Microsoft.Extensions.Caching.Distributed;
    using Microsoft.Extensions.Caching.Hybrid;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.DependencyInjection;
    using Reqnroll;

    public class CacheWrapper
    {
        private readonly CacheProviderType cacheProviderType;
        private readonly ICachedItemSerializer serializer;
        private readonly IServiceProvider serviceProvider;
        private readonly byte[] nullValue = Array.Empty<byte>();

        public CacheWrapper(ScenarioContext context, CacheProviderType cacheProviderType)
        {
            this.cacheProviderType = cacheProviderType;
            this.serializer = new DefaultCachedItemSerializer(null);
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
                    var distributedCache = this.serviceProvider.GetRequiredService<IDistributedCache>();
                    result = await distributedCache.GetAsync(key);
                    break;
                case CacheProviderType.Hybrid:
                    var hybridCache = this.serviceProvider.GetRequiredService<HybridCache>();
                    result = await hybridCache.GetOrCreateAsync<byte[]>(key, _ => new ValueTask<byte[]>(this.nullValue));
                    break;
            }

            return result;
        }

        public async Task<T?> GetAsync<T>(string key) where T : class, new()
        {
            T? result = null;
            switch (this.cacheProviderType)
            {
                case CacheProviderType.Memory:
                    var memoryCache = this.serviceProvider.GetRequiredService<IMemoryCache>();
                    result = memoryCache.Get<T>(key);
                    break;
                case CacheProviderType.Csv:
                    var distributedCache = this.serviceProvider.GetRequiredService<IDistributedCache>();
                    var stored = await distributedCache.GetAsync(key);
                    result = this.serializer.Deserialize<T>(stored!);
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
                        SlidingExpiration = ttl
                    });
                    break;
                case CacheProviderType.Csv:
                    var distributedCache = this.serviceProvider.GetRequiredService<IDistributedCache>();
                    await distributedCache.SetAsync(key, value, new DistributedCacheEntryOptions()
                    {
                        SlidingExpiration = ttl
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

        public async Task SetAsync<T>(string key, T value) where T : class, new()
        {
            switch (this.cacheProviderType)
            {
                case CacheProviderType.Memory:
                    var memoryCache = this.serviceProvider.GetRequiredService<IMemoryCache>();
                    memoryCache.Set(key, value);
                    break;
                case CacheProviderType.Csv:
                    var distributedCache = this.serviceProvider.GetRequiredService<IDistributedCache>();
                    var bytes = await this.serializer.SerializeAsync(value);
                    await distributedCache.SetAsync(key, bytes);
                    break;
                case CacheProviderType.Hybrid:
                    var hybridCache = this.serviceProvider.GetRequiredService<HybridCache>();
                    await hybridCache.SetAsync(key, value);
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