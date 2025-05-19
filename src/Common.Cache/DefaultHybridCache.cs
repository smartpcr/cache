// -----------------------------------------------------------------------
// <copyright file="DefaultHybridCache.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Common.Cache
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Caching.Hybrid;
    using OpenTelemetry.Lib;

    /// <summary>
    /// Wrapper for HybridCache to provide a consistent interface for caching operations.
    /// </summary>
    public class DefaultHybridCache<T> : IHybridCache<T> where T : class
    {
        private readonly HybridCache hybridCache;
        private readonly DiagnosticsConfig diagnosticsConfig;
        private readonly Func<T, bool> shouldInvalidate;
        private readonly TimeSpan defaultExpirationSpan;
        private readonly List<string> tags;

        public DefaultHybridCache(
            HybridCache cache,
            DiagnosticsConfig diagnosticsConfig,
            TimeSpan defaultExpirationSpan,
            Func<T, bool> shouldInvalidate = null)
        {
            this.hybridCache = cache;
            this.diagnosticsConfig = diagnosticsConfig;
            this.defaultExpirationSpan = defaultExpirationSpan;
            this.shouldInvalidate = shouldInvalidate;
            this.tags = new List<string>()
            {
                typeof(T).Name
            };
        }

        public List<string> Tags => this.tags;

        /// <summary>
        /// Retrieves a value from the cache or adds it if it doesn't exist.
        /// When predicate is not null, it will be used to determine if the value should be cached.
        /// </summary>
        /// <typeparam name="T">The cached item.</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="valueFactory">Function to retrieve value from backend.</param>
        /// <param name="expiration">Optional expiration span.</param>
        /// <param name="shouldInvalidate">
        ///     Function to determine if the cached item should be invalidated,
        ///     if true, it's not stored in cache and always retrieved from backend.
        /// </param>
        /// <param name="token">The cancellation token</param>
        /// <returns>Item retrieved from cache or backend.</returns>
        public async Task<(T, Exception)> GetOrAddAsync(
            string key,
            Func<CancellationToken, Task<T>> valueFactory,
            TimeSpan? expiration = null,
            Func<T, bool> skipStoreInCache = null,
            CancellationToken token = default)
        {
            using var span = this.diagnosticsConfig.StartNewSpan();
            bool cacheHit = true;
            T result;
            Exception valueFactoryException = null;
            Func<T, bool> shouldRemoveFromCache = skipStoreInCache ?? this.shouldInvalidate;
            if (shouldRemoveFromCache == null)
            {
                result = await this.hybridCache.GetOrCreateAsync(
                    key,
                    async ct =>
                    {
                        cacheHit = false;
                        try
                        {
                            var value = await valueFactory(ct);
                            return value;
                        }
                        catch (Exception ex)
                        {
                            valueFactoryException = ex;
                            throw;
                        }
                    },
                    new HybridCacheEntryOptions()
                    {
                        Expiration = expiration ?? this.defaultExpirationSpan,
                    },
                    this.tags,
                    cancellationToken: token);
            }
            else
            {
                result = await this.hybridCache.GetOrCreateAsync(
                    key,
                    async ct =>
                    {
                        cacheHit = false;
                        try
                        {
                            var value = await valueFactory(ct);
                            return value;
                        }
                        catch (Exception ex)
                        {
                            valueFactoryException = ex;
                            return null;
                        }
                    },
                    new HybridCacheEntryOptions()
                    {
                        Expiration = expiration ?? this.defaultExpirationSpan,
                        Flags = HybridCacheEntryFlags.DisableDistributedCacheWrite | HybridCacheEntryFlags.DisableLocalCacheWrite
                    },
                    this.tags,
                    cancellationToken: token);

                if (result != null && !shouldRemoveFromCache(result))
                {
                    await this.hybridCache.SetAsync(key,
                        result,
                        new HybridCacheEntryOptions()
                        {
                            Expiration = expiration ?? this.defaultExpirationSpan,
                        },
                        cancellationToken: token);
                }
            }

            if (cacheHit)
            {
                this.diagnosticsConfig.OnCacheHit(key);
            }
            else
            {
                this.diagnosticsConfig.OnCacheMiss(key);
            }

            return (result, valueFactoryException);
        }

        public async Task<(List<T>, Exception)> GetOrAddListAsync(
            string key,
            Func<CancellationToken, Task<List<T>>> valueFactory,
            TimeSpan? expiration = null,
            CancellationToken token = default)
        {
            using var span = this.diagnosticsConfig.StartNewSpan();
            bool cacheHit = true;
            Exception valueFactoryException = null;
            var list = await this.hybridCache.GetOrCreateAsync(
                key,
                async ct =>
                {
                    cacheHit = false;
                    try
                    {
                        var value = await valueFactory(ct);
                        return value;
                    }
                    catch (Exception ex)
                    {
                        valueFactoryException = ex;
                        return null;
                    }
                },
                new HybridCacheEntryOptions()
                {
                    Expiration = expiration ?? this.defaultExpirationSpan,
                },
                this.tags,
                cancellationToken: token);

            if (cacheHit)
            {
                this.diagnosticsConfig.OnCacheHit(key);
            }
            else
            {
                this.diagnosticsConfig.OnCacheMiss(key);
            }

            return (list, valueFactoryException);
        }

        public async Task SetAsync(string key, T value, TimeSpan? expiration = null, CancellationToken token = default)
        {
            await this.hybridCache.SetAsync(
                key,
                value,
                new HybridCacheEntryOptions()
                {
                    Expiration = expiration ?? this.defaultExpirationSpan,
                },
                this.tags,
                cancellationToken: token);
        }

        public async Task SetListAsync(string key, List<T> list, TimeSpan? expiration = null, CancellationToken token = default)
        {
            await this.hybridCache.SetAsync(
                key,
                list,
                new HybridCacheEntryOptions()
                {
                    Expiration = expiration ?? this.defaultExpirationSpan,
                },
                this.tags,
                cancellationToken: token);
        }

        public async Task RemoveAsync(string key, CancellationToken token = default)
        {
            using var span = this.diagnosticsConfig.StartNewSpan();
            await this.hybridCache.RemoveAsync(key, token);

            const int maxRetries = 10;
            const int delayBetweenRetriesMs = 100;
            int retryCount = 0;
            var cacheExists = await this.ExistsAsync(key, token);
            while (cacheExists && retryCount < maxRetries)
            {
                await this.hybridCache.RemoveAsync(key, token);
                retryCount++;
                cacheExists = await this.ExistsAsync(key, token);
                if (!cacheExists)
                {
                    break;
                }

                await Task.Delay(delayBetweenRetriesMs, token);
            }

            if (cacheExists)
            {
                throw new InvalidOperationException($"Failed to remove key '{key}' after multiple attempts.");
            }

            this.diagnosticsConfig.OnCacheRemoved(key);
        }

        public async Task<bool> ExistsAsync(string key, CancellationToken token = default)
        {
            using var span = this.diagnosticsConfig.StartNewSpan();
            var result = await this.hybridCache.GetOrCreateAsync<T>(
                key,
                _ => new ValueTask<T>((T)null),
                new HybridCacheEntryOptions()
                {
                    Flags = HybridCacheEntryFlags.DisableDistributedCacheWrite | HybridCacheEntryFlags.DisableLocalCacheWrite,
                },
                cancellationToken: token);
            return result != null;
        }

        public async Task ClearAsync(CancellationToken cancel = default)
        {
            using var span = this.diagnosticsConfig.StartNewSpan();
            await this.hybridCache.RemoveByTagAsync(this.tags, cancel);
        }
    }
}