// -----------------------------------------------------------------------
// <copyright file="TestHook.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Common.Cache.Tests.Hooks
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using OpenTelemetry.Lib;
    using Reqnroll;
    using Reqnroll.Infrastructure;
    using Reqnroll.Microsoft.Extensions.DependencyInjection;
    using Unity;

    [Binding]
    public class TestHook
    {
        internal const string ServiceName = "bvt";
        internal static readonly string OtlpEndpoint = "http://127.0.0.1:4320";
        internal static readonly Dictionary<string, string> OtelConfig = OtelSettings.GetOtelConfigSettings(true, OtlpEndpoint, LogLevel.Debug);
        internal static readonly DiagnosticsConfig DiagConfig = new DiagnosticsConfig(OtelConfig, ServiceName);

        private static readonly Lazy<ServiceCollection> services = new(() =>
        {
            var serviceCollection = new ServiceCollection();
            return serviceCollection;
        });

        [ScenarioDependencies]
        public static IServiceCollection GetServiceCollection()
        {
            services.Value.AddScoped<IReqnrollOutputHelper, ReqnrollOutputHelper>();
            return services.Value;
        }

        [ScenarioDependencies]
        public static UnityContainer GetUnityContainer()
        {
            var container = new UnityContainer();
            container.RegisterType<IReqnrollOutputHelper, ReqnrollOutputHelper>();
            return container;
        }
    }
}