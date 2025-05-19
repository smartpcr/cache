// -----------------------------------------------------------------------
// <copyright file="ResourceStatus.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Common.Models.Resources
{
    using System;

    /// <summary>
    /// Represents the status of a get operation.
    /// </summary>
    [Flags]
    public enum ResourceStatus
    {
        /// <summary>
        /// The default status.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// The resource was obtained from the aggregator.
        /// </summary>
        FromAggregator = 1,

        /// <summary>
        /// The resource was obtained from the cache.
        /// </summary>
        FromCache = 2,

        /// <summary>
        /// The cached resource was expired.
        /// </summary>
        ItemExpired = 4,

        /// <summary>
        /// The cached resource list was expired.
        /// </summary>
        ListExpired = 8,

        /// <summary>
        /// The resource was not in the cache.
        /// </summary>
        NotInCache = 16,

        /// <summary>
        /// The aggregator timed out when obtaining the resource.
        /// </summary>
        AggregatorTimeout = 32,

        /// <summary>
        /// The resource was not found.
        /// </summary>
        NotFound = 64
    }
}