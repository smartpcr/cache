// -----------------------------------------------------------------------
// <copyright file="CsvCache.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Common.Cache
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Caching.Distributed;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Internal;
    using OpenTelemetry.Lib;
    using OpenTelemetry.Trace;

    /// <summary>
    /// L2 cache implementation using CSV files.
    /// </summary>
    public class CsvCache : IDistributedCache
    {
        private readonly CsvCacheSettings cacheSettings;
        private readonly DiagnosticsConfig diagnosticsConfig;
        private readonly ISystemClock clock;

        public CsvCache(IServiceProvider serviceProvider)
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            this.cacheSettings = configuration.GetConfiguredSettings<CsvCacheSettings>();
            this.diagnosticsConfig = serviceProvider.GetRequiredService<DiagnosticsConfig>();
            this.clock = serviceProvider.GetRequiredService<ISystemClock>();
        }

        public byte[]? Get(string key)
        {
            return this.GetAsync(key).GetAwaiter().GetResult();
        }

        public async Task<byte[]?> GetAsync(string key, CancellationToken token = new CancellationToken())
        {
            using var span = this.diagnosticsConfig.StartNewSpan();
            span.SetAttribute("key", key);
            try
            {
                var cacheFile = Path.Combine(this.cacheSettings.CacheFolder, key);
                if (File.Exists(cacheFile))
                {
                    if (File.GetCreationTimeUtc(cacheFile).Add(this.cacheSettings.TimeToLive) < this.clock.UtcNow)
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
            this.SetAsync(key, value, options).GetAwaiter().GetResult();
        }

        public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = new CancellationToken())
        {
            using var span = this.diagnosticsConfig.StartNewSpan();
            span.SetAttribute("key", key);

            try
            {
                var size = (int)Math.Ceiling((double)value.Length / 1_000_000); // MB
                span.SetAttribute("size", size);
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
                this.diagnosticsConfig.OnCacheUpsert(key, this.cacheSettings.TimeToLive);
            }
            catch (Exception ex)
            {
                span.SetStatus(Status.Error);
                span.RecordException(ex);
            }
        }

        public void Refresh(string key)
        {
            this.RefreshAsync(key).GetAwaiter().GetResult();
        }

        public Task RefreshAsync(string key, CancellationToken token = new CancellationToken())
        {
            using var span = this.diagnosticsConfig.StartNewSpan();
            span.SetAttribute("key", key);

            try
            {
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
            catch (Exception ex)
            {
                span.SetStatus(Status.Error);
                span.RecordException(ex);
            }

            return Task.CompletedTask;
        }

        public void Remove(string key)
        {
            this.RemoveAsync(key).GetAwaiter().GetResult();
        }

        public Task RemoveAsync(string key, CancellationToken token = new CancellationToken())
        {
            using var span = this.diagnosticsConfig.StartNewSpan();
            span.SetAttribute("key", key);

            try
            {
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
    }
}