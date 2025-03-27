// -----------------------------------------------------------------------
// <copyright file="TestHook.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Common.Cache.Tests.Hooks
{
    using Microsoft.Extensions.DependencyInjection;
    using Reqnroll;
    using Reqnroll.Infrastructure;
    using Reqnroll.Microsoft.Extensions.DependencyInjection;
    using Unity;

    [Binding]
    public class TestHook
    {
        [ScenarioDependencies]
        public static IServiceCollection GetServiceCollection()
        {
            var services = new ServiceCollection();
            services.AddScoped<IReqnrollOutputHelper, ReqnrollOutputHelper>();
            return services;
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