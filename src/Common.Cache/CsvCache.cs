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
    /// L2 cache implementation using files under cluster shared volume.
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
                span.SetAttribute("cacheFile", cacheFile);
                if (File.Exists(cacheFile))
                {
                    if (File.GetCreationTimeUtc(cacheFile).Add(this.cacheSettings.TimeToLive) < this.clock.UtcNow)
                    {
                        span.SetAttribute("expired", true);
                        this.diagnosticsConfig.OnCacheExpired(key);
                        return null;
                    }
                }
                else
                {
                    span.SetAttribute("found", false);
                    this.diagnosticsConfig.OnCacheMiss(key);
                    return null;
                }

#if NET462 || NETSTANDARD2_0 || NETSTANDARD2_1
                var fileContent = await FileExtension.ReadAllBytesAsync(cacheFile, token);
#else
                var fileContent = await File.ReadAllBytesAsync(cacheFile, token);
#endif

                // File must be at least 16 bytes long (header + data).
                if (fileContent.Length < 16)
                {
                    span.SetAttribute("invalid", true);
                    this.diagnosticsConfig.OnCacheMiss(key);
                    return null;
                }

                // Extract header: first 8 bytes for absolute expiration ticks,
                // next 8 bytes for sliding expiration ticks.
                var absExpirationTicks = BitConverter.ToInt64(fileContent, 0);
                var slidingTicks = BitConverter.ToInt64(fileContent, 8);
                var absoluteExpiration = absExpirationTicks > 0 ? new DateTime(absExpirationTicks, DateTimeKind.Utc) : (DateTime?)null;

                // Check if the cache entry has expired.
                if (absoluteExpiration.HasValue && this.clock.UtcNow > absoluteExpiration.Value)
                {
                    span.SetAttribute("expired", true);
                    this.diagnosticsConfig.OnCacheExpired(key);
                    File.Delete(cacheFile);
                    return null;
                }

                // If sliding expiration is set, update the header with a new expiration.
                if (slidingTicks > 0)
                {
                    var slidingCandidate = this.clock.UtcNow.AddTicks(slidingTicks);
                    var newExpiration = absoluteExpiration.HasValue
                        ? (slidingCandidate < absoluteExpiration.Value ? slidingCandidate : absoluteExpiration.Value)
                        : slidingCandidate;

                    // Prepare new header.
                    var newHeader = new byte[16];
                    Buffer.BlockCopy(BitConverter.GetBytes(newExpiration.Ticks), 0, newHeader, 0, 8);
                    Buffer.BlockCopy(BitConverter.GetBytes(slidingTicks), 0, newHeader, 8, 8);

                    // Open the file for writing and update only the header (first 16 bytes).
                    using var stream = new FileStream(cacheFile, FileMode.Open, FileAccess.Write, FileShare.None);
                    await stream.WriteAsync(newHeader, 0, newHeader.Length, token);
                }

                // Extract and return the actual data (after the 16-byte header).
                var actualData = new byte[fileContent.Length - 16];
                Buffer.BlockCopy(fileContent, 16, actualData, 0, actualData.Length);

                span.SetAttribute("found", true);
                span.SetAttribute("size", actualData.Length);
                this.diagnosticsConfig.OnCacheHit(key);
                return actualData;
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
                span.SetAttribute("cacheFile", cacheFile);
                if (File.Exists(cacheFile))
                {
                    span.SetAttribute("update", true);
                    File.Delete(cacheFile);
                }

                // Determine the absolute expiration.
                DateTime? absoluteExpiration = null;
                if (options.AbsoluteExpirationRelativeToNow.HasValue)
                {
                    absoluteExpiration = this.clock.UtcNow.DateTime.Add(options.AbsoluteExpirationRelativeToNow.Value);
                }
                else if (options.AbsoluteExpiration.HasValue)
                {
                    absoluteExpiration = options.AbsoluteExpiration.Value.ToUniversalTime().DateTime;
                }
                else
                {
                    // Fallback to using sliding expiration or a default TTL.
                    absoluteExpiration = this.clock.UtcNow.DateTime.Add(options.SlidingExpiration ?? this.cacheSettings.TimeToLive);
                }

                var absExpirationTicks = absoluteExpiration?.Ticks ?? 0;
                var slidingTicks = options.SlidingExpiration?.Ticks ?? 0;

                // Build the 16-byte header.
                var header = new byte[16];
                Buffer.BlockCopy(BitConverter.GetBytes(absExpirationTicks), 0, header, 0, 8);
                Buffer.BlockCopy(BitConverter.GetBytes(slidingTicks), 0, header, 8, 8);

                // Create a new array containing the header and then the cache value.
                var data = new byte[16 + value.Length];
                Buffer.BlockCopy(header, 0, data, 0, 16);
                Buffer.BlockCopy(value, 0, data, 16, value.Length);

#if NET462 || NETSTANDARD2_0 || NETSTANDARD2_1
                await FileExtension.WriteAllBytesAsync(cacheFile, data, token);
#else
                await File.WriteAllBytesAsync(cacheFile, data, token);
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

        public async Task RefreshAsync(string key, CancellationToken token = new CancellationToken())
        {
            using var span = this.diagnosticsConfig.StartNewSpan();
            span.SetAttribute("key", key);

            try
            {
                var cacheFile = Path.Combine(this.cacheSettings.CacheFolder, key);
                span.SetAttribute("cacheFile", cacheFile);
                if (File.Exists(cacheFile))
                {
                    span.SetAttribute("found", true);
                    // For a sliding expiration, GetAsync will update the header.
                    await this.GetAsync(key, token);
                }
                else
                {
                    span.SetAttribute("found", false);
                    this.diagnosticsConfig.OnCacheMiss(key);
                }
            }
            catch (Exception ex)
            {
                span.SetStatus(Status.Error);
                span.RecordException(ex);
            }
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
                span.SetAttribute("cacheFile", cacheFile);
                if (File.Exists(cacheFile))
                {
                    span.SetAttribute("found", true);
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