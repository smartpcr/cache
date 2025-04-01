// -----------------------------------------------------------------------
// <copyright file="CacheProviderBenchmark.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Common.Cache.Benchmarks
{
    using System;
    using System.Collections.Generic;
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
        private CsvCache csvCache;
        private WindowsRegistryCache windowsRegistryCache;
        private IServiceProvider serviceProvider;
        private ICachedItemSerializer serializer;

        [Params(16, 16348, 5242880)]
        public int PayloadSize { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            var services = new ServiceCollection();
            services.AddConfiguration();
            services.AddSingleton(DiagConfig);

            ISystemClock clock = new SystemClock();
            services.AddSingleton<ISystemClock>(clock);

            this.memoryCache = new MemoryCache(new MemoryCacheOptions { Clock = new SystemClock() });
            services.AddSingleton(this.memoryCache);

            this.csvCache = new CsvCache(services.BuildServiceProvider());
            services.AddSingleton(this.csvCache);

            this.windowsRegistryCache = new WindowsRegistryCache(services.BuildServiceProvider());
            services.AddSingleton(this.windowsRegistryCache);

            services.AddSingleton<IMemoryCache>(this.memoryCache);
            services.AddSingleton<IDistributedCache>(this.csvCache);
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
            var payload = this.GetPayload();
            await this.csvCache.SetAsync(CacheProviderBenchmark.Key, payload, new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
            });
            var stored = await this.csvCache.GetAsync(CacheProviderBenchmark.Key);
            stored.Should().NotBeNull();
            stored.Should().BeEquivalentTo(payload);
        }

        [Benchmark]
        public async Task WinRegistryBenchmark()
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

        [Benchmark]
        public async Task HybridBenchmark()
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

        private byte[] GetPayload()
        {
            var expected = new byte[this.PayloadSize];
            new Random().NextBytes(expected);
            return expected;
        }
    }
}