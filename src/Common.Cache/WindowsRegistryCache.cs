// -----------------------------------------------------------------------
// <copyright file="WindowsRegistryCache.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Common.Cache
{
    using System;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Caching.Distributed;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Internal;
    using Microsoft.Win32;
    using OpenTelemetry.Lib;

    public class WindowsRegistryCache : IDistributedCache
    {
        private readonly WindowsRegistryCacheSettings cacheSettings;
        private readonly DiagnosticsConfig diagnosticsConfig;
        private readonly ISystemClock clock;

        public WindowsRegistryCache(IServiceProvider serviceProvider)
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            this.cacheSettings = configuration.GetConfiguredSettings<WindowsRegistryCacheSettings>();
            this.diagnosticsConfig = serviceProvider.GetRequiredService<DiagnosticsConfig>();
            this.clock = serviceProvider.GetRequiredService<ISystemClock>();
        }

        public byte[]? Get(string key)
        {
            using var span = this.diagnosticsConfig.StartNewSpan();
            span.SetAttribute("key", key);

            var data = this.ReadRegistryValue(key);
            if (data == null || data.Length < 16)
            {
                // we store sliding and absolute expiration in the first 16 bytes
                this.diagnosticsConfig.OnCacheMiss(key);
                this.DeleteRegistryValue(key);
                return null;
            }

            this.diagnosticsConfig.OnCacheHit(key);

            // Extract header values.
            var absExpirationTicks = BitConverter.ToInt64(data, 0);
            var absoluteExpiration = absExpirationTicks > 0
                ? new DateTime(absExpirationTicks, DateTimeKind.Utc)
                : (DateTime?)null;
            // Check if the entry has expired.
            if (absoluteExpiration.HasValue && this.clock.UtcNow > absoluteExpiration.Value)
            {
                span.SetAttribute("absoluteExpiration", absoluteExpiration.Value.ToString(CultureInfo.InvariantCulture));
                this.diagnosticsConfig.OnCacheExpired(key);
                this.DeleteRegistryValue(key);
                return null;
            }

            var actualData = new byte[data.Length - 16];
            Buffer.BlockCopy(data, 16, actualData, 0, actualData.Length);

            var slidingTicks = BitConverter.ToInt64(data, 8);
            // If sliding expiration is set, update the absolute expiration.
            if (slidingTicks > 0)
            {
                var slidingCandidate = this.clock.UtcNow.AddTicks(slidingTicks);
                var newExpiration = absoluteExpiration.HasValue
                    ? (slidingCandidate < absoluteExpiration.Value ? slidingCandidate : absoluteExpiration.Value)
                    : slidingCandidate;
                span.SetAttribute("slidingExpiration", newExpiration.ToString(CultureInfo.InvariantCulture));

                // Write the updated header back to the registry.
                var newHeader = new byte[16];
                Array.Copy(BitConverter.GetBytes(newExpiration.Ticks), 0, newHeader, 0, 8);
                Array.Copy(BitConverter.GetBytes(slidingTicks), 0, newHeader, 8, 8);

                var newData = new byte[16 + actualData.Length];
                Buffer.BlockCopy(newHeader, 0, newData, 0, 16);
                Buffer.BlockCopy(actualData, 0, newData, 16, actualData.Length);
                this.WriteRegistryValue(key, newData);
            }

            return actualData;
        }

        public async Task<byte[]?> GetAsync(string key, CancellationToken token = new CancellationToken())
        {
            return await Task.FromResult(this.Get(key));
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            // Calculate absolute expiration.
            DateTime? absoluteExpiration = null;
            if (options.AbsoluteExpirationRelativeToNow.HasValue)
            {
                absoluteExpiration = this.clock.UtcNow.DateTime.Add(options.AbsoluteExpirationRelativeToNow.Value);
            }
            else if (options.AbsoluteExpiration.HasValue)
            {
                absoluteExpiration = options.AbsoluteExpiration.Value!.ToUniversalTime().DateTime;
            }

            var absExpirationTicks = absoluteExpiration?.Ticks ?? 0;
            var slidingTicks = options.SlidingExpiration?.Ticks ?? 0;
            var header = new byte[16];
            Array.Copy(BitConverter.GetBytes(absExpirationTicks), 0, header, 0, 8);
            Array.Copy(BitConverter.GetBytes(slidingTicks), 0, header, 8, 8);

            var data = new byte[16 + value.Length];
            Buffer.BlockCopy(header, 0, data, 0, 16);
            Buffer.BlockCopy(value, 0, data, 16, value.Length);

            this.WriteRegistryValue(key, data);
        }

        public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = new CancellationToken())
        {
            this.Set(key, value, options);
            await Task.CompletedTask;
        }

        public void Refresh(string key)
        {
            // Call Get to update the sliding expiration.
            this.Get(key);
        }

        public async Task RefreshAsync(string key, CancellationToken token = new CancellationToken())
        {
            this.Refresh(key);
            await Task.CompletedTask;
        }

        public void Remove(string key)
        {
            this.DeleteRegistryValue(key);
        }

        public async Task RemoveAsync(string key, CancellationToken token = new CancellationToken())
        {
            this.Remove(key);
            await Task.CompletedTask;
        }

        // Helper to optionally sanitize/transform the cache key for registry storage.
        private string GetRegistryValueName(string key) => $"{this.cacheSettings.RegistryPath}\\{key}";

        private byte[]? ReadRegistryValue(string key)
        {
            using var baseKey = Registry.LocalMachine.OpenSubKey(this.cacheSettings.RegistryPath, writable: false);
            if (baseKey == null)
                return null;

            return baseKey.GetValue(this.GetRegistryValueName(key)) as byte[];
        }

        private void WriteRegistryValue(string key, byte[] value)
        {
            using var baseKey = Registry.LocalMachine.CreateSubKey(this.cacheSettings.RegistryPath);
            baseKey!.SetValue(this.GetRegistryValueName(key), value, RegistryValueKind.Binary);
        }

        private void DeleteRegistryValue(string key)
        {
            using var baseKey = Registry.LocalMachine.OpenSubKey(this.cacheSettings.RegistryPath, writable: true);
            if (baseKey != null)
            {
                baseKey.DeleteValue(this.GetRegistryValueName(key), throwOnMissingValue: false);
            }
        }
    }
}