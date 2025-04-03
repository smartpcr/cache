// -----------------------------------------------------------------------
// <copyright file="CacheProviderBenchmark.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Common.Cache.Benchmarks
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using BenchmarkDotNet.Attributes;
    using Common.Cache.Serialization;
    using FluentAssertions;
    using Microsoft.Extensions.Caching.Distributed;
    using Microsoft.Extensions.Caching.Hybrid;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Internal;
    using Microsoft.Extensions.Logging;
    using Microsoft.Win32;
    using OpenTelemetry.Lib;

    [MemoryDiagnoser]
    public class CacheProviderBenchmark
    {
        private const string Key = "k1";

        internal const string ServiceName = "bvt";
        internal static readonly string OtlpEndpoint = "http://127.0.0.1:4320";
        internal static readonly Dictionary<string, string> OtelConfig = OtelSettings.GetOtelConfigSettings(true, OtlpEndpoint, LogLevel.Debug);
        internal static readonly DiagnosticsConfig DiagConfig = new DiagnosticsConfig(OtelConfig, ServiceName);

        private MemoryCache memoryCache;
        private CsvFileCache csvFileCache;
        private WindowsRegistryCache windowsRegistryCache;
        private IServiceProvider serviceProvider;
        private ICachedItemSerializer serializer;

        [Params(16, 16348, 5242880)]
        public int PayloadSize { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            var services = new ServiceCollection();
            var config = services.AddConfiguration();
            var cacheSettings = config.GetConfiguredSettings<CacheSettings>();
            if (!Directory.Exists(cacheSettings.CacheFolder))
            {
                Directory.CreateDirectory(cacheSettings.CacheFolder);
            }
            var csvCacheSettings = config.GetConfiguredSettings<CsvCacheSettings>();
            if (!Directory.Exists(csvCacheSettings.CacheFolder))
            {
                Directory.CreateDirectory(csvCacheSettings.CacheFolder);
            }
            var winRegCacheSettings = config.GetConfiguredSettings<WindowsRegistryCacheSettings>();
            using var baseKey = Registry.LocalMachine.OpenSubKey(winRegCacheSettings.RegistryPath, writable: true);
            if (baseKey == null)
            {
                // Create the registry key if it does not exist
                Console.WriteLine($"creating win registry key at {winRegCacheSettings.RegistryPath}");
                Registry.LocalMachine.CreateSubKey(winRegCacheSettings.RegistryPath);
            }

            services.AddSingleton(DiagConfig);

            ISystemClock clock = new SystemClock();
            services.AddSingleton<ISystemClock>(clock);

            this.memoryCache = new MemoryCache(new MemoryCacheOptions { Clock = new SystemClock() });
            services.AddSingleton(this.memoryCache);

            this.csvFileCache = new CsvFileCache(services.BuildServiceProvider());
            services.AddSingleton(this.csvFileCache);

            this.windowsRegistryCache = new WindowsRegistryCache(services.BuildServiceProvider());
            services.AddSingleton(this.windowsRegistryCache);

            services.AddSingleton<IMemoryCache>(this.memoryCache);
            services.AddSingleton<IDistributedCache>(this.csvFileCache);
            services.AddHybridCache();

            this.serviceProvider = services.BuildServiceProvider();

            this.serializer = new DefaultCachedItemSerializer(null);
        }

        [Benchmark]
        public Task MemoryCacheBenchmark()
        {
            var payload = this.GetPayload();
            this.memoryCache.Set(CacheProviderBenchmark.Key, payload, TimeSpan.FromMinutes(5));
            var value = this.memoryCache.Get(CacheProviderBenchmark.Key);
            value.Should().BeEquivalentTo(payload);
            return Task.CompletedTask;
        }

        [Benchmark]
        public async Task FileCacheBenchmark()
        {
            try
            {
                var payload = this.GetPayload();
                await this.csvFileCache.SetAsync(CacheProviderBenchmark.Key,
                    payload,
                    new DistributedCacheEntryOptions()
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
                    });
                var stored = await this.csvFileCache.GetAsync(CacheProviderBenchmark.Key);
                stored.Should().NotBeNull();
                stored.Should().BeEquivalentTo(payload);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        [Benchmark]
        public async Task WinRegistryBenchmark()
        {
            try
            {
                var payload = this.GetPayload();
                await this.windowsRegistryCache.SetAsync(CacheProviderBenchmark.Key, payload, new DistributedCacheEntryOptions()
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
                });
                var stored = await this.windowsRegistryCache.GetAsync(CacheProviderBenchmark.Key);
                stored.Should().NotBeNull();
                stored.Should().BeEquivalentTo(payload);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        [Benchmark]
        public async Task HybridBenchmark()
        {
            try
            {
                var payload = this.GetPayload();
                var hybridCache = this.serviceProvider.GetRequiredService<HybridCache>();
                await hybridCache.SetAsync(CacheProviderBenchmark.Key, payload, new HybridCacheEntryOptions()
                {
                    Expiration = TimeSpan.FromMinutes(5),
                });
                var stored = await hybridCache.GetOrCreateAsync(CacheProviderBenchmark.Key, _ => new ValueTask<object>(payload));
                stored.Should().NotBeNull();
                stored.Should().BeEquivalentTo(payload);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private byte[] GetPayload()
        {
            var expected = new byte[this.PayloadSize];
            new Random().NextBytes(expected);
            return expected;
        }
    }
}