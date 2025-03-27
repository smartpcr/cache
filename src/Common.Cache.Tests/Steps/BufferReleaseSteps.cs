// -----------------------------------------------------------------------
// <copyright file="BufferReleaseSteps.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Common.Cache.Tests.Steps
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Common.Cache.Tests.Hooks;
    using FluentAssertions;
    using Microsoft.Extensions.Caching.Hybrid;
    using Microsoft.Extensions.DependencyInjection;
    using Reqnroll;

    [Binding]
    public class BufferReleaseSteps
    {
        private readonly ScenarioContext context;
        private readonly IReqnrollOutputHelper outputHelper;

        public BufferReleaseSteps(ScenarioContext context, IReqnrollOutputHelper outputHelper)
        {
            this.context = context;
            this.outputHelper = outputHelper;
        }

        [Given(@"I have a hybrid cache provider")]
        public void GivenIHaveAHybridCacheProvider()
        {
            var services = TestHook.GetServiceCollection();
            services.Should().NotBeNull();
            services.AddHybridCache();
            var serviceProvider = services.BuildServiceProvider();
            var cache = serviceProvider.GetService<HybridCache>();
            this.context.Set(cache);
        }

        [Given(@"a cached item")]
        public void GivenACachedItem(Table table)
        {
            var key = table.Rows[0]["Key"];
            var value = table.Rows[0]["Value"];
            this.context.Set(new KeyValuePair<string, string>(key, value), "cachedItem");
        }

        [When(@"I store cached item")]
        public async Task WhenIStoreCachedItem()
        {
            var kvp = this.context.Get<KeyValuePair<string, string>>("cachedItem");
            var cache = this.context.Get<HybridCache>();
            var first = await cache.GetOrCreateAsync<string>(kvp.Key, _ => GetAsync(kvp.Value));
            first.Should().NotBeNull();
            first.Should().Be(kvp.Value);
        }

        [Then(@"cached item should be added to cache")]
        public void ThenCachedItemShouldBeAddedToCache()
        {
            var kvp = this.context.Get<KeyValuePair<string, string>>("cachedItem");
            var cache = this.context.Get<HybridCache>();

        }

        static ValueTask<string> GetAsync(string value) => new ValueTask<string>(value);
    }
}