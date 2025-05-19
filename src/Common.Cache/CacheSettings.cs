// -----------------------------------------------------------------------
// <copyright file="CacheSettings.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Common.Cache
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.IO;

    public class CacheSettings
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
        /// Gets or sets global expiration span of cached item. Default is null and only use individual item expiration.
        /// </summary>
        public TimeSpan? TimeToLive { get; set; } = null;

        /// <summary>
        /// Gets or sets the expiration period for purging old cache files.
        /// Default is null and never purge.
        /// </summary>
        public int? PurgeExpirationInDays { get; set; } = null;
    }

    public class FolderExistsValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(value?.ToString()))
            {
                return new ValidationResult("The folder path cannot be null or empty.");
            }

            if (value is string folderPath && !Directory.Exists(folderPath))
            {
                return new ValidationResult($"The folder specified in '{folderPath}' does not exist.");
            }

            return ValidationResult.Success!;
        }
    }
}