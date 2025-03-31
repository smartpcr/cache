// -----------------------------------------------------------------------
// <copyright file="ScenarioHook.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Common.Cache.Tests.Hooks
{
    using System;
    using System.Linq;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Internal;
    using OpenTelemetry.Trace;
    using Reqnroll;

    [Reqnroll.Binding]
    public class ScenarioHook
    {
        [BeforeScenario(Order = 1)]
        public void SetupOtel(ScenarioContext scenarioContext, IReqnrollOutputHelper outputHelper)
        {
            var services = TestHook.GetServiceCollection();
            services.AddSingleton(TestHook.DiagConfig);
            var tracerProvider = TestHook.DiagConfig.TracerProvider;
            var tracer = tracerProvider.GetTracer($"{TestHook.ServiceName}");
            scenarioContext.Set(tracer);

            var span = tracer.StartActiveSpan(scenarioContext.ScenarioInfo.Title, SpanKind.Client);
            if (scenarioContext.ScenarioInfo.Tags.Any())
            {
                foreach (var tag in scenarioContext.ScenarioInfo.Tags)
                {
                    span.SetAttribute(tag, true);
                }
            }

            scenarioContext.Set(span);
        }

        [Reqnroll.BeforeScenario("dev", Order = 1)]
        public void SetupDevEnv(ScenarioContext scenarioContext, IReqnrollOutputHelper outputHelper)
        {
            this.SetupEnv(scenarioContext, outputHelper, "Development");
        }

        [BeforeScenario("prod", Order = 1)]
        public void SetupProdEnv(ScenarioContext scenarioContext, IReqnrollOutputHelper outputHelper)
        {
            this.SetupEnv(scenarioContext, outputHelper, "Production");
        }

        [BeforeScenario(Order = 1)]
        public void SetupDI(ScenarioContext scenarioContext, IReqnrollOutputHelper outputHelper)
        {
            outputHelper.WriteLine($"Starting scenario: {scenarioContext.ScenarioInfo.Title}");
            outputHelper.WriteLine($"Tags: {string.Join(", ", scenarioContext.ScenarioInfo.Tags)}");

            var services = TestHook.GetServiceCollection();
            services.Should().NotBeNull();

            var clock = new FakeTime();
            clock.Reset();
            scenarioContext.Set(clock);

            services.AddSingleton<TimeProvider>(clock);
            services.AddSingleton<ISystemClock>(clock);

            scenarioContext.Set(services);
        }

        [Reqnroll.AfterScenario]
        public void AfterScenario(ScenarioContext scenarioContext, IReqnrollOutputHelper outputHelper)
        {
            if (scenarioContext.TryGetValue(out TelemetrySpan span))
            {
                span.SetStatus(scenarioContext.TestError != null
                    ? Status.Error.WithDescription(scenarioContext.TestError.Message)
                    : Status.Ok);
                span.End();
            }

            foreach (var item in scenarioContext)
            {
                if (item.Value is IDisposable disposableItem)
                {
                    outputHelper.WriteLine($"Disposing {item.Key}...");
                    disposableItem.Dispose();
                }
            }
        }

        private void SetupEnv(ScenarioContext scenarioContext, IReqnrollOutputHelper outputHelper, string envName)
        {
            outputHelper.WriteLine($"Use {envName} environment");
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", envName, EnvironmentVariableTarget.Process);
            var services = TestHook.GetServiceCollection();
            var configuration = services.AddConfiguration();
            scenarioContext.Set(configuration);
            scenarioContext.Set(envName, "envName");
        }

    }
}