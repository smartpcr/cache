// -----------------------------------------------------------------------
// <copyright file="IHybridCache.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Common.Cache
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IHybridCache<T> where T : class
    {
        Task<(T, Exception)> GetOrAddAsync(
            string key,
            Func<CancellationToken, Task<T>> valueFactory,
            TimeSpan? expiration = null,
            Func<T, bool> skipStoreInCache = null,
            CancellationToken token = default);

        Task<(List<T>, Exception)> GetOrAddListAsync(
            string key,
            Func<CancellationToken, Task<List<T>>> valueFactory,
            TimeSpan? expiration = null,
            CancellationToken token = default);

        Task SetAsync(
            string key,
            T value,
            TimeSpan? expiration = null,
            CancellationToken token = default);

        Task SetListAsync(
            string key,
            List<T> list,
            TimeSpan? expiration = null,
            CancellationToken token = default);

        Task RemoveAsync(string key, CancellationToken token = default);

        Task<bool> ExistsAsync(string key, CancellationToken token = default);

        Task ClearAsync(CancellationToken cancel = default);
    }
}