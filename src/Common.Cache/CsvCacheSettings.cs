// -----------------------------------------------------------------------
// <copyright file="CsvCacheOptions.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Common.Cache
{
    using System;

    public class CsvCacheSettings
    {
        /// <summary>
        /// Fallback cache folder for local cache.
        /// </summary>
        [FolderExistsValidation]
        public string CacheFolder { get; set; }

        /// <summary>
        /// Gets or sets the amount the cache is compacted by when the maximum size is exceeded.
        /// </summary>
        public double CompactionPercentage { get; set; } = 0.05;

        /// <summary>
        /// Gets or sets the maximum size of the cache.
        /// </summary>
        public long? SizeLimit { get; set; } = 100 * 1024 * 1024; // 100 MB

        /// <summary>
        /// Gets or sets the minimum length of time between successive scans for expired items.
        /// </summary>
        public TimeSpan TimeToLive { get; set; } = TimeSpan.FromMinutes(15);
    }
}