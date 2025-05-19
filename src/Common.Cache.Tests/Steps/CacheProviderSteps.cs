// -----------------------------------------------------------------------
// <copyright file="CacheSteps.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Common.Cache.Tests.Steps
{
    using System;
    using System.Threading.Tasks;
    using Common.Cache.Serialization;
    using Common.Cache.Tests.Hooks;
    using FluentAssertions;
    using Microsoft.Extensions.Caching.Distributed;
    using Microsoft.Extensions.Caching.Hybrid;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.DependencyInjection;
    using Reqnroll;

    [Binding]
    public class CacheProviderSteps
    {
        private readonly ScenarioContext context;
        private readonly IReqnrollOutputHelper outputHelper;

        public CacheProviderSteps(ScenarioContext scenarioContext, IReqnrollOutputHelper outputHelper)
        {
            this.context = scenarioContext;
            this.outputHelper = outputHelper;
        }

        [Given(@"cache provider {string} is registered")]

        public void GivenCacheOfTypeIsRegistered(string cacheProvider)
        {
            Enum.TryParse(cacheProvider, true, out CacheProviderType cacheProviderType).Should().BeTrue();

            var services = this.context.Get<IServiceCollection>();
            var clock = this.context.Get<FakeTime>();

            switch (cacheProviderType)
            {
                case CacheProviderType.Memory:
                    var l1 = new MemoryCache(new MemoryCacheOptions { Clock = clock });
                    services.AddSingleton<IMemoryCache>(l1);
                    break;
                case CacheProviderType.Csv:
                    var l2 = new CsvFileCache(services.BuildServiceProvider());
                    services.AddSingleton<IDistributedCache>(l2);
                    break;
                case CacheProviderType.WindowsRegistry:
                    var l3 = new WindowsRegistryCache(services.BuildServiceProvider());
                    services.AddSingleton<IDistributedCache>(l3);
                    break;
                case CacheProviderType.Hybrid:
                    var memCache = new MemoryCache(new MemoryCacheOptions { Clock = clock });
                    services.AddSingleton<IMemoryCache>(memCache);
                    var csvCache = new CsvFileCache(services.BuildServiceProvider());
                    services.AddSingleton<IDistributedCache>(csvCache);
                    var factory = new SerializerFactory(SerializerType.Binary);
                    services.AddSingleton<IHybridCacheSerializerFactory>(factory);
                    services.AddHybridCache();
                    break;
            }

            var cacheWrapper = new CacheWrapper(this.context, cacheProviderType);
            this.context.Set(cacheWrapper);
        }

        [Given(@"store a cached item with ttl of {int} minutes")]
        public async Task GivenACachedItem(int ttlMin, Table table)
        {
            var cacheWrapper = this.context.Get<CacheWrapper>();
            var key = table.Rows[0]["Key"];
            var size = int.Parse(table.Rows[0]["Size"]);
            var expected = new byte[size];
            new Random().NextBytes(expected);
            this.context.Set(expected, "expected");

            await cacheWrapper.RemoveAsync(key);
            var found = await cacheWrapper.GetAsync(key);
            found.Should().BeNull();

            await cacheWrapper.SetAsync(key, expected, TimeSpan.FromMinutes(ttlMin));
        }

        [Given("store a customer with ttl of {int} minutes")]
        public async Task GivenStoreACustomerWithTtlOfMinutes(int ttl, DataTable dataTable)
        {
            var key = dataTable.Rows[0]["Key"];
            var customer = dataTable.CreateInstance<Customer>();

            var cacheWrapper = this.context.Get<CacheWrapper>();
            await cacheWrapper.RemoveAsync(key);
            var found = await cacheWrapper.GetAsync<Customer>(key);
            found.Should().BeNull();

            await cacheWrapper.SetAsync(key, customer, TimeSpan.FromMinutes(ttl));
        }

        [Then(@"I can validate the cached item")]
        public async Task ThenCachedItemShouldBeAddedToCache(Table table)
        {
            var cacheWrapper = this.context.Get<CacheWrapper>();
            var key = table.Rows[0]["Key"];
            var size = int.Parse(table.Rows[0]["Size"]);
            var expected = this.context.Get<byte[]>("expected");

            var actual = await cacheWrapper.GetAsync(key);
            actual.Should().NotBeNull();
            actual.Length.Should().Be(size);
            actual.Should().BeEquivalentTo(expected);
        }

        [Then("cached item should still be valid after {int} minutes")]
        public async Task ThenCachedItemShouldStillBeValidAfterMinutes(int waitMinutes, DataTable table)
        {
            var clock = this.context.Get<FakeTime>();
            clock.Add(TimeSpan.FromMinutes(waitMinutes));

            var key = table.Rows[0]["Key"];
            var size = int.Parse(table.Rows[0]["Size"]);
            var expected = this.context.Get<byte[]>("expected");

            var cacheWrapper = this.context.Get<CacheWrapper>();
            var actual = await cacheWrapper.GetAsync(key);
            actual.Should().NotBeNull();
            actual.Length.Should().Be(size);
            actual.Should().BeEquivalentTo(expected);
            this.outputHelper.WriteLine("cached item not yet expired");
        }

        [Then("cached item should be expired after {int} minutes")]
        public async Task ThenCachedItemShouldBeExpiredAfterMinutes(int waitMinutes, DataTable table)
        {
            var clock = this.context.Get<FakeTime>();
            clock.Add(TimeSpan.FromMinutes(waitMinutes));

            var cacheWrapper = this.context.Get<CacheWrapper>();
            var key = table.Rows[0]["Key"];
            var actual = await cacheWrapper.GetAsync(key);
            actual.Should().BeNull();
            this.outputHelper.WriteLine("expiration validated");
        }

        [Then("I can validate customer")]
        public async Task ThenICanValidateCustomer(DataTable dataTable)
        {
            var key = dataTable.Rows[0]["Key"];
            var expectedCustomer = dataTable.CreateInstance<Customer>();

            var cacheWrapper = this.context.Get<CacheWrapper>();

            var actualCustomer = await cacheWrapper.GetAsync<Customer>(key);
            actualCustomer.Should().NotBeNull();
            actualCustomer.Should().BeEquivalentTo(expectedCustomer);
        }

        [Then("cached customer should still be valid after {int} minutes")]
        public async Task ThenCachedCustomerShouldStillBeValidAfterMinutes(int waitMinutes, DataTable dataTable)
        {
            var clock = this.context.Get<FakeTime>();
            clock.Add(TimeSpan.FromMinutes(waitMinutes));

            var key = dataTable.Rows[0]["Key"];
            var cacheWrapper = this.context.Get<CacheWrapper>();
            var actualCustomer = await cacheWrapper.GetAsync<Customer>(key);
            actualCustomer.Should().NotBeNull();
        }

        [Then("cached customer should be expired after {int} minutes")]
        public async Task ThenCachedCustomerShouldBeExpiredAfterMinutes(int waitMinutes, DataTable dataTable)
        {
            var clock = this.context.Get<FakeTime>();
            clock.Add(TimeSpan.FromMinutes(waitMinutes));

            var key = dataTable.Rows[0]["Key"];
            var cacheWrapper = this.context.Get<CacheWrapper>();
            var actualCustomer = await cacheWrapper.GetAsync<Customer>(key);
            actualCustomer.Should().BeNull();
        }
    }
}