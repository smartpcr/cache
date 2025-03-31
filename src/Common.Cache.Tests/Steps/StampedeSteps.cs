// -----------------------------------------------------------------------
// <copyright file="StampedeSteps.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Common.Cache.Tests.Steps
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.Extensions.Caching.Hybrid;
    using Microsoft.Extensions.DependencyInjection;
    using Reqnroll;

    [Binding]
    public class StampedeSteps
    {
        private readonly ScenarioContext context;
        private readonly IReqnrollOutputHelper outputHelper;
        private readonly ConcurrentDictionary<string, Customer> customers = new ConcurrentDictionary<string, Customer>();

        public StampedeSteps(ScenarioContext context, IReqnrollOutputHelper outputHelper)
        {
            this.context = context;
            this.outputHelper = outputHelper;
        }

        [Given("a customer stored in backend api")]
        public void GivenACustomerStoredInBackendApi(DataTable dataTable)
        {
            var key = dataTable.Rows[0]["Key"];
            var customer = dataTable.CreateInstance<Customer>();
            this.customers[key] = customer;
            this.context.Set(key, "Key");
        }

        [When("I try to fetch customer from cache with fetch from api upon cache miss")]
        public async Task WhenITryToFetchCustomerFromCacheWithFetchFromApiUponCacheMiss(DataTable dataTable)
        {
            var callCount = int.Parse(dataTable.Rows[0]["CallCount"]);
            var canBeCanceled = bool.Parse(dataTable.Rows[0]["CanBeCanceled"]);
            var key = this.context.Get<string>("Key");
            var services = this.context.Get<IServiceCollection>();
            var serviceProvider = services.BuildServiceProvider();
            var cache = serviceProvider.GetRequiredService<HybridCache>();

            using var semaphore = new SemaphoreSlim(0);
            using var cts = canBeCanceled ? new CancellationTokenSource() : null;
            var token = cts?.Token ?? CancellationToken.None;
            var executeCount = 0;
            var cancelCount = 0;
            var results = new Task<Customer>[callCount];
            for (var i = 0; i < callCount; i++)
            {
                results[i] = cache.GetOrCreateAsync(key, async ct =>
                {
                    this.outputHelper.WriteLine($"Fetching customer from API for {key}");
                    using var reg = ct.Register(() => Interlocked.Increment(ref cancelCount));
                    if (!await semaphore.WaitAsync(5_000, CancellationToken.None))
                    {
                        throw new TimeoutException("Failed to activate");
                    }

                    Interlocked.Increment(ref executeCount);
                    ct.ThrowIfCancellationRequested(); // assert not cancelled
                    return this.customers[key];
                }, cancellationToken: token).AsTask();
            }

            Volatile.Read(ref executeCount).Should().Be(0);
            Volatile.Read(ref cancelCount).Should().Be(0);
            semaphore.Release();
            this.outputHelper.WriteLine($"ExecuteCount: {executeCount}");
            this.outputHelper.WriteLine($"CancelCount: {cancelCount}");

            results.Should().NotBeNullOrEmpty();
            var customers = new List<Customer>();
            var firstResult = await results[0];
            customers.Add(firstResult);
            Volatile.Read(ref executeCount).Should().Be(1);
            Volatile.Read(ref cancelCount).Should().Be(0);
            for (var i = 1; i < callCount; i++)
            {
                var result = await results[i];
                customers.Add(result);
                result.Should().NotBeNull();
                result.Should().BeEquivalentTo(firstResult);
            }
            Volatile.Read(ref executeCount).Should().Be(1);
            Volatile.Read(ref cancelCount).Should().Be(0);

            this.context.Set(customers, "Customers");
            this.context.Set(executeCount, "ExecuteCount");
            this.context.Set(cancelCount, "CancelCount");
        }

        [Then("expected customer should be returned")]
        public void ThenExpectedCustomerShouldBeReturned(DataTable dataTable)
        {
            var expectedCustomer = dataTable.CreateInstance<Customer>();
            var results = this.context.Get<List<Customer>>("Customers");
            results.Count.Should().BeGreaterThanOrEqualTo(1);
            var actualCustomer = results[0];
            actualCustomer.Should().NotBeNull();
            actualCustomer.Should().BeEquivalentTo(expectedCustomer);
        }

        [Then("backend call count should be")]
        public void ThenBackendCallCountShouldBe(DataTable dataTable)
        {
            var expectedExecutionCount = int.Parse(dataTable.Rows[0]["ExecutionCount"]);
            var actualExecutionCount = this.context.Get<int>("ExecuteCount");
            actualExecutionCount.Should().Be(expectedExecutionCount);
        }
    }
}