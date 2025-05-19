// -----------------------------------------------------------------------
// <copyright file="SpillableMemoryCache.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Common.Cache
{
    using System;
    using System.Buffers;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Caching.Distributed;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Internal;
    using Microsoft.Extensions.Options;
    using OpenTelemetry.Lib;
    using OpenTelemetry.Trace;

    public class SpillableMemoryCache : IBufferDistributedCache
    {
        private readonly CacheSettings cacheSettings;
        private readonly DiagnosticsConfig diagnosticsConfig;
        private readonly MemoryCache memoryCache;
        private readonly ISystemClock clock;

        public SpillableMemoryCache(IServiceProvider serviceProvider)
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            this.cacheSettings = configuration.GetConfiguredSettings<CacheSettings>();
            this.diagnosticsConfig = serviceProvider.GetRequiredService<DiagnosticsConfig>();
            this.clock = serviceProvider.GetRequiredService<ISystemClock>();

            var cacheOptions = new MemoryCacheOptions
            {
                CompactionPercentage = this.cacheSettings.CompactionPercentage,
                SizeLimit = this.cacheSettings.SizeLimit,
                ExpirationScanFrequency = this.cacheSettings.TimeToLive.Value,
                Clock = this.clock
            };
            this.memoryCache = new MemoryCache(new OptionsWrapper<MemoryCacheOptions>(cacheOptions));
        }

        public byte[]? Get(string key)
        {
            return this.GetAsync(key).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public async Task<byte[]?> GetAsync(string key, CancellationToken token = new CancellationToken())
        {
            using var span = this.diagnosticsConfig.StartNewSpan();
            span.SetAttribute("key", key);
            try
            {
                if (this.memoryCache.TryGetValue(key, out var value) && value is byte[] cachedValue)
                {
                    this.diagnosticsConfig.OnCacheHit(key);
                    return cachedValue;
                }

                var cacheFile = Path.Combine(this.cacheSettings.CacheFolder, key);
                if (File.Exists(cacheFile))
                {
                    if (File.GetCreationTimeUtc(cacheFile).Add(this.cacheSettings.TimeToLive.Value) < this.clock.UtcNow)
                    {
                        this.diagnosticsConfig.OnCacheExpired(key);
                        return null;
                    }
                }
                else
                {
                    this.diagnosticsConfig.OnCacheMiss(key);
                    return null;
                }

#if NET462 || NETSTANDARD2_0 || NETSTANDARD2_1
                var fileContent = await FileExtension.ReadAllBytesAsync(cacheFile, token);
#else
                var fileContent = await File.ReadAllBytesAsync(cacheFile, token);
#endif

                this.diagnosticsConfig.OnCacheHit(key);

                // fill back on memory cache and extend sliding expiration
                var size = (int)Math.Ceiling((double)fileContent.Length / 1_000_000); // MB
                var entryOptions = new MemoryCacheEntryOptions()
                    .SetSize(size)
                    .SetSlidingExpiration(this.cacheSettings.TimeToLive.Value);
                this.memoryCache.Set(key, fileContent, entryOptions);
                this.diagnosticsConfig.OnCacheUpsert(key, this.cacheSettings.TimeToLive.Value);

                return fileContent;
            }
            catch (Exception ex)
            {
                span.SetStatus(Status.Error);
                span.RecordException(ex);
                return null;
            }
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            this.SetAsync(key, value, options).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = new CancellationToken())
        {
            using var span = this.diagnosticsConfig.StartNewSpan();
            span.SetAttribute("key", key);

            try
            {
                var size = (int)Math.Ceiling((double)value.Length / 1_000_000); // MB
                span.SetAttribute("size", size);
                var entryOptions = new MemoryCacheEntryOptions()
                    .SetSize(size)
                    .SetSlidingExpiration(this.cacheSettings.TimeToLive.Value);
                this.memoryCache.Set(key, value, entryOptions);
                var cacheFile = Path.Combine(this.cacheSettings.CacheFolder, key);
                if (File.Exists(cacheFile))
                {
                    File.Delete(cacheFile);
                }

#if NET462 || NETSTANDARD2_0 || NETSTANDARD2_1
                await FileExtension.WriteAllBytesAsync(cacheFile, value, token);
#else
                await File.WriteAllBytesAsync(cacheFile, value, token);
#endif
                this.diagnosticsConfig.OnCacheUpsert(key, this.cacheSettings.TimeToLive.Value);
            }
            catch (Exception ex)
            {
                span.SetStatus(Status.Error);
                span.RecordException(ex);
            }
        }

        public void Refresh(string key)
        {
            this.RefreshAsync(key).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <summary>
        /// refresh and update the expiration time only if the key exists
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="token">The cancellation token.</param>
        public async Task RefreshAsync(string key, CancellationToken token = new CancellationToken())
        {
            using var span = this.diagnosticsConfig.StartNewSpan();
            span.SetAttribute("key", key);

            var cachedValue = await this.GetAsync(key, token);
            if (cachedValue != null)
            {
                this.diagnosticsConfig.OnCacheHit(key);
                var size = (int)Math.Ceiling((double)cachedValue.Length / 1_000_000); // MB
                var entryOptions = new MemoryCacheEntryOptions()
                    .SetSize(size)
                    .SetSlidingExpiration(this.cacheSettings.TimeToLive.Value);
                this.memoryCache.Set(key, cachedValue, entryOptions);
            }
            else
            {
                this.diagnosticsConfig.OnCacheMiss(key);
            }

            var cacheFile = Path.Combine(this.cacheSettings.CacheFolder, key);
            if (File.Exists(cacheFile))
            {
                File.SetCreationTimeUtc(cacheFile, this.clock.UtcNow.UtcDateTime);
            }
            else
            {
                this.diagnosticsConfig.OnCacheMiss(key);
            }
        }

        public void Remove(string key)
        {
            this.RemoveAsync(key).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public Task RemoveAsync(string key, CancellationToken token = new CancellationToken())
        {
            using var span = this.diagnosticsConfig.StartNewSpan();
            span.SetAttribute("key", key);

            try
            {
                this.memoryCache.Remove(key);
                var cacheFile = Path.Combine(this.cacheSettings.CacheFolder, key);
                if (File.Exists(cacheFile))
                {
                    File.Delete(cacheFile);
                }
            }
            catch (Exception ex)
            {
                span.SetStatus(Status.Error);
                span.RecordException(ex);
            }

            return Task.CompletedTask;
        }

        public bool TryGet(string key, IBufferWriter<byte> destination)
        {
            return this.TryGetAsync(key, destination, CancellationToken.None).GetAwaiter().GetResult();
        }

        public async ValueTask<bool> TryGetAsync(string key, IBufferWriter<byte> destination, CancellationToken token = new CancellationToken())
        {
            using var span = this.diagnosticsConfig.StartNewSpan();
            span.SetAttribute("key", key);
            try
            {
                var cachedValue = await this.GetAsync(key, token);
                if (cachedValue != null)
                {
                    destination.Write(cachedValue);
                    this.diagnosticsConfig.OnCacheHit(key);
                    return true;
                }

                this.diagnosticsConfig.OnCacheMiss(key);
                return false;
            }
            catch (Exception ex)
            {
                span.SetStatus(Status.Error);
                span.RecordException(ex);
                return false;
            }
        }

        public void Set(string key, ReadOnlySequence<byte> value, DistributedCacheEntryOptions options)
        {
            this.SetAsync(key, value, options).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public async ValueTask SetAsync(string key, ReadOnlySequence<byte> value, DistributedCacheEntryOptions options, CancellationToken token = new CancellationToken())
        {
            using var span = this.diagnosticsConfig.StartNewSpan();
            span.SetAttribute("key", key);

            try
            {
                var size = (int)Math.Ceiling((double)value.Length / 1_000_000); // MB
                span.SetAttribute("size", size);
                var entryOptions = new MemoryCacheEntryOptions()
                    .SetSize(size)
                    .SetSlidingExpiration(this.cacheSettings.TimeToLive.Value);
                this.memoryCache.Set(key, value.ToArray(), entryOptions);
                var cacheFile = Path.Combine(this.cacheSettings.CacheFolder, key);
                if (File.Exists(cacheFile))
                {
                    File.Delete(cacheFile);
                }

#if NET462 || NETSTANDARD2_0 || NETSTANDARD2_1
                await FileExtension.WriteAllBytesAsync(cacheFile, value.ToArray(), token);
#else
                await File.WriteAllBytesAsync(cacheFile, value.ToArray(), token);
#endif
            }
            catch (Exception ex)
            {
                span.SetStatus(Status.Error);
                span.RecordException(ex);
            }
        }
    }
}