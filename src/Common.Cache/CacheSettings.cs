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
        /// Gets or sets the minimum length of time between successive scans for expired items.
        /// </summary>
        public TimeSpan TimeToLive { get; set; } = TimeSpan.FromMinutes(15);
    }

    public class FolderExistsValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
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