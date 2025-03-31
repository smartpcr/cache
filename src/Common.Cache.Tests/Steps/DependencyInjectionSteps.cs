// -----------------------------------------------------------------------
// <copyright file="DependencyInjectionSteps.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Common.Cache.Tests.Steps
{
    using Common.Cache.Tests.Hooks;
    using FluentAssertions;
    using Microsoft.Extensions.Caching.Hybrid;
    using Microsoft.Extensions.DependencyInjection;
    using Reqnroll;
    using Unity;
    using Unity.Lifetime;

    [Binding]
    public class DependencyInjectionSteps
    {
        private readonly ScenarioContext scenarioContext;
        private readonly IReqnrollOutputHelper outputHelper;

        public DependencyInjectionSteps(ScenarioContext scenarioContext, IReqnrollOutputHelper outputHelper)
        {
            this.scenarioContext = scenarioContext;
            this.outputHelper = outputHelper;
        }

        [Given("I have a service collection")]
        public void GivenIHaveAServiceCollection()
        {
            var services = TestHook.GetServiceCollection();
            services.Should().NotBeNull();
            this.scenarioContext.Set(services);
        }

        [Given(@"I have a unity container")]
        public void GivenIHaveAUnityContainer()
        {
            var services = TestHook.GetServiceCollection();
            services.Should().NotBeNull();
            this.scenarioContext.Set(services);

            var container = TestHook.GetUnityContainer();
            container.Should().NotBeNull();
            this.scenarioContext.Set(container);
        }

        [When(@"I register hybrid cache provider with service collection")]
        public void WhenIRegisterHybridCacheProvider()
        {
            var services = this.scenarioContext.Get<IServiceCollection>();
            services.Should().NotBeNull();
            services.AddHybridCache();
            var serviceProvider = services.BuildServiceProvider();
            var cache = serviceProvider.GetService<HybridCache>();
            cache.Should().NotBeNull();
            this.scenarioContext.Set(cache, "cache");
        }

        [When(@"I register hybrid cache provider with unity container")]
        public void WhenIRegisterHybridCacheProviderWithUnityContainer()
        {
            var services = this.scenarioContext.Get<IServiceCollection>();
            services.Should().NotBeNull();
            services.AddHybridCache();
            var serviceProvider = services.BuildServiceProvider();
            var cache = serviceProvider.GetService<HybridCache>();
            cache.Should().NotBeNull();
            cache.GetType().Name.Should().Be("DefaultHybridCache");

            var container = this.scenarioContext.Get<UnityContainer>();
            container.Should().NotBeNull();
            cache.Should().NotBeNull();
            container.RegisterInstance(typeof(HybridCache), cache, new ContainerControlledLifetimeManager());
            this.scenarioContext.Set(cache, "cache");
        }

        [Then(@"I should be able to resolve the hybrid cache provider")]
        public void ThenIShouldBeAbleToResolveTheHybridCacheProvider()
        {
            var cache = this.scenarioContext.Get<HybridCache>("cache");
            cache.Should().NotBeNull();
            this.outputHelper.WriteLine("Hybrid cache provider resolved successfully.");

            if (this.scenarioContext.TryGetValue(out UnityContainer container))
            {
                var cacheFromUnity = container.Resolve<HybridCache>();
                cacheFromUnity.Should().NotBeNull();
            }
        }
    }
}