// -----------------------------------------------------------------------
// <copyright file="ResourceResolver.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.AzureStack.Services.Fabric.Common.Resource
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using global::Common.Cache;
    using global::Common.Models.Contract;
    using global::Common.Models.Core;
    using global::Common.Models.Resources;
    using Microsoft.Extensions.DependencyInjection;
    using OpenTelemetry.Lib;
    using OpenTelemetry.Trace;

    public class ResourceResolver<T> where T : class, IResourceProperties
    {
        private readonly TimeSpan resolverTimeout = TimeSpan.FromMinutes(5);
        private readonly IHybridCache<T> cache;
        private readonly DiagnosticsConfig diagnosticsConfig;
        private readonly IResourceAggregator<T> aggregator;

        public ResourceResolver(IServiceProvider serviceProvider)
        {
            this.cache = serviceProvider.GetRequiredService<IHybridCache<T>>();
            this.diagnosticsConfig = serviceProvider.GetRequiredService<DiagnosticsConfig>();
            this.aggregator = serviceProvider.GetRequiredService<IResourceAggregator<T>>();
        }

        public async Task<T> GetResource(RpId key, ResolverContext resolverContext, CancellationToken cancel)
        {
            using var span = this.diagnosticsConfig.StartNewSpan();
            ArgumentValidator.NotNull(key, nameof(key));
            var cacheKey = DefaultCacheKey.GetItemCacheKey<T>(key.IdString);
            span.SetAttribute("cacheKey", cacheKey);
            var readFromBackend = false;

            try
            {
                T cacheReturn = null;
                using (var timeoutCts = new CancellationTokenSource(this.resolverTimeout))
                using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancel, timeoutCts.Token))
                {
                    try
                    {
                        (cacheReturn, var valueEx) = await this.cache.GetOrAddAsync(
                            cacheKey,
                            async (ct) =>
                            {
                                await Task.Delay(1000, ct);
                                var cachedItem = await this.aggregator.GetResult(key, resolverContext, ct);
                                readFromBackend = true;
                                return cachedItem;
                            },
                            token: linkedCts.Token);

                        if (valueEx != null)
                        {
                            throw valueEx;
                        }
                    }
                    catch (OperationCanceledException ex) when (timeoutCts.Token.IsCancellationRequested)
                    {
                        span.SetAttribute("timeout", true);
                        throw new TimeoutException($"The operation timed out after {this.resolverTimeout.TotalSeconds} seconds.", ex);
                    }
                    catch (OperationCanceledException) when (cancel.IsCancellationRequested)
                    {
                        span.SetAttribute("canceledByCaller", true);
                        throw;
                    }
                }

                if (cacheReturn == null || !readFromBackend)
                {
                    this.diagnosticsConfig.OnCacheMiss(cacheKey);
                }
                else
                {
                    this.diagnosticsConfig.OnCacheHit(cacheKey);
                }

                return cacheReturn;
            }
            catch (Exception ex)
            {
                this.diagnosticsConfig.OnCacheError(cacheKey, ex.Message);
                span.SetStatus(Status.Error);
                span.RecordException(ex);
                throw;
            }
        }

        public async Task<IEnumerable<T>> GetResourceList(ResolverContext resolverContext, CancellationToken cancel = default)
        {
            using var span = this.diagnosticsConfig.StartNewSpan();
            var cacheReturn = new List<T>();
            var cacheKey = DefaultCacheKey.GetListCacheKey<T>();

            try
            {
                var readFromBackend = false;
                using (var timeoutCts = new CancellationTokenSource(this.resolverTimeout))
                using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancel, timeoutCts.Token))
                {
                    try
                    {
                        (cacheReturn, var valueEx) = await this.cache.GetOrAddListAsync(
                            cacheKey,
                            async (ct) =>
                            {
                                var list = await this.aggregator.GetAllResults(resolverContext, ct);
                                readFromBackend = true;
                                return list.ToList();
                            },
                            token: linkedCts.Token);

                        if (valueEx != null)
                        {
                            throw valueEx;
                        }
                    }
                    catch (OperationCanceledException ex) when (timeoutCts.Token.IsCancellationRequested)
                    {
                        span.SetAttribute("timeout", true);
                        throw new TimeoutException($"The operation timed out after {this.resolverTimeout.TotalSeconds} seconds.", ex);
                    }
                    catch (OperationCanceledException) when (cancel.IsCancellationRequested)
                    {
                        span.SetAttribute("canceledByCaller", true);
                        throw;
                    }
                }

                if (cacheReturn == null || !readFromBackend)
                {
                    this.diagnosticsConfig.OnCacheMiss(cacheKey);
                }
                else
                {
                    this.diagnosticsConfig.OnCacheHit(cacheKey);
                }

                return cacheReturn;
            }
            catch (Exception ex)
            {
                this.diagnosticsConfig.OnCacheError(cacheKey, ex.Message);
                span.SetStatus(Status.Error);
                span.RecordException(ex);
                throw;
            }
        }
    }
}