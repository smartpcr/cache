// -----------------------------------------------------------------------
// <copyright file="CsvCache.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Common.Cache
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Caching.Distributed;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Internal;
    using OpenTelemetry.Lib;
    using OpenTelemetry.Trace;

    /// <summary>
    /// L2 cache implementation using files under cluster shared volume.
    /// </summary>
    public class CsvFileCache : IDistributedCache
    {
        private readonly CacheSettings cacheSettings;
        private readonly DiagnosticsConfig diagnosticsConfig;
        private readonly ISystemClock clock;

        public CsvFileCache(CacheSettings cacheSettings, ISystemClock clock, DiagnosticsConfig diagnosticsConfig)
        {
            this.cacheSettings = cacheSettings;
            this.clock = clock;
            this.diagnosticsConfig = diagnosticsConfig;
        }

        public byte[] Get(string key)
        {
            return this.GetAsync(key).GetAwaiter().GetResult();
        }

        public async Task<byte[]> GetAsync(string key, CancellationToken token = new CancellationToken())
        {
            using var span = this.diagnosticsConfig.StartNewSpan();
            span.SetAttribute("key", key);
            try
            {
                var cacheFile = CacheKeyToFilePath(key, this.cacheSettings.CacheFolder);
                span.SetAttribute("cacheFile", cacheFile);
                if (File.Exists(cacheFile))
                {
                    var fileCreationTime = File.GetCreationTimeUtc(cacheFile);
                    bool fileExpired = (this.cacheSettings.TimeToLive.HasValue && fileCreationTime.Add(this.cacheSettings.TimeToLive.Value) < this.clock.UtcNow) || (this.cacheSettings.PurgeExpirationInDays.HasValue &&
                        fileCreationTime.AddDays(this.cacheSettings.PurgeExpirationInDays.Value) < this.clock.UtcNow);

                    if (fileExpired)
                    {
                        span.SetAttribute("expired", true);
                        this.diagnosticsConfig.OnCacheExpired(key);
                        try
                        {
                            File.Delete(cacheFile);
                        }
                        catch (Exception ex)
                        {
                            this.diagnosticsConfig.OnCacheError(key, $"Failed to delete expired file {cacheFile}: {ex.Message}");
                        }
                        return null;
                    }
                }
                else
                {
                    span.SetAttribute("found", false);
                    this.diagnosticsConfig.OnCacheMiss(key);
                    return null;
                }

                var fileContent = await FileExtension.ReadAllBytesAsync(cacheFile, token);
                if (fileContent.Length < 16)
                {
                    this.diagnosticsConfig.OnCacheMiss(key);
                    return null;
                }

                span.SetAttribute("found", true);
                this.diagnosticsConfig.OnCacheHit(key);

                // Extract header: first 8 bytes for absolute expiration ticks,
                // next 8 bytes for sliding expiration ticks.
                var absExpirationTicks = BitConverter.ToInt64(fileContent, 0);
                var slidingTicks = BitConverter.ToInt64(fileContent, 8);
                var absoluteExpiration = absExpirationTicks > 0 ? new DateTime(absExpirationTicks, DateTimeKind.Utc) : (DateTime?)null;

                // Check if the cache entry has expired.
                if (absoluteExpiration.HasValue && this.clock.UtcNow > absoluteExpiration.Value)
                {
                    this.diagnosticsConfig.OnCacheExpired(key);
                    File.Delete(cacheFile);
                    return null;
                }

                // If sliding expiration is set, update the header with a new expiration.
                if (slidingTicks > 0)
                {
                    var slidingCandidate = this.clock.UtcNow.AddTicks(slidingTicks);
                    var newExpiration = absoluteExpiration ?? slidingCandidate;

                    // Prepare new header.
                    var newHeader = new byte[16];
                    Buffer.BlockCopy(BitConverter.GetBytes(newExpiration.Ticks), 0, newHeader, 0, 8);
                    Buffer.BlockCopy(BitConverter.GetBytes(slidingTicks), 0, newHeader, 8, 8);

                    // Open the file for writing and update only the header (first 16 bytes).
                    using var stream = new FileStream(cacheFile, FileMode.Open, FileAccess.Write, FileShare.None);
                    await stream.WriteAsync(newHeader, 0, newHeader.Length, token);
                }

                // Extract and return the actual data (after the 16-byte header).
                var actualData = new byte[fileContent.Length - 16];
                Buffer.BlockCopy(fileContent, 16, actualData, 0, actualData.Length);
                span.SetAttribute("size", actualData.Length);

                return actualData;
            }
            catch (Exception ex)
            {
                span.SetStatus(Status.Error);
                span.RecordException(ex);
                return null;
            }
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            this.SetAsync(key, value, options).GetAwaiter().GetResult();
        }

        public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = new CancellationToken())
        {
            using var span = this.diagnosticsConfig.StartNewSpan();
            span.SetAttribute("key", key);
            span.SetAttribute("size", value.Length);

            try
            {
                var cacheFile = CacheKeyToFilePath(key, this.cacheSettings.CacheFolder);
                var cacheFileDirectory = Path.GetDirectoryName(cacheFile);
                if (!string.IsNullOrEmpty(cacheFileDirectory) && !Directory.Exists(cacheFileDirectory))
                {
                    Directory.CreateDirectory(cacheFileDirectory!);
                }
                span.SetAttribute("cacheFile", cacheFile);
                if (File.Exists(cacheFile))
                {
                    span.SetAttribute("mode", "update");
                    await EnsureFileDeletedAsync(cacheFile, token);
                }
                else
                {
                    span.SetAttribute("mode", "create");
                }

                // Determine the absolute expiration.
                DateTime? absoluteExpiration = null;
                if (options.AbsoluteExpirationRelativeToNow.HasValue)
                {
                    absoluteExpiration = this.clock.UtcNow.DateTime.Add(options.AbsoluteExpirationRelativeToNow.Value);
                }
                else if (options.AbsoluteExpiration.HasValue)
                {
                    absoluteExpiration = options.AbsoluteExpiration.Value.ToUniversalTime().DateTime;
                }

                var absExpirationTicks = absoluteExpiration?.Ticks ?? 0;
                var slidingExpiration = (options.SlidingExpiration ?? this.cacheSettings.TimeToLive) ?? TimeSpan.Zero;
                var slidingTicks = slidingExpiration.Ticks;

                // Build the 16-byte header.
                var header = new byte[16];
                Buffer.BlockCopy(BitConverter.GetBytes(absExpirationTicks), 0, header, 0, 8);
                Buffer.BlockCopy(BitConverter.GetBytes(slidingTicks), 0, header, 8, 8);

                // Create a new array containing the header and then the cache value.
                var data = new byte[16 + value.Length];
                Buffer.BlockCopy(header, 0, data, 0, 16);
                Buffer.BlockCopy(value, 0, data, 16, value.Length);

                await FileExtension.WriteAllBytesAsync(cacheFile, data, token);
                this.diagnosticsConfig.OnCacheUpsert(key, slidingExpiration);

                if (this.cacheSettings.PurgeExpirationInDays.HasValue)
                {
                    this.PurgeOldFiles();
                }
            }
            catch (Exception ex)
            {
                span.SetStatus(Status.Error);
                span.RecordException(ex);
            }
        }

        public void Refresh(string key)
        {
            this.RefreshAsync(key).GetAwaiter().GetResult();
        }

        public async Task RefreshAsync(string key, CancellationToken token = new CancellationToken())
        {
            using var span = this.diagnosticsConfig.StartNewSpan();
            span.SetAttribute("key", key);
            await this.GetAsync(key, token);
        }

        public void Remove(string key)
        {
            this.RemoveAsync(key).GetAwaiter().GetResult();
        }

        public async Task RemoveAsync(string key, CancellationToken token = new CancellationToken())
        {
            using var span = this.diagnosticsConfig.StartNewSpan();
            span.SetAttribute("key", key);

            try
            {
                var cacheFile = CacheKeyToFilePath(key, this.cacheSettings.CacheFolder);
                span.SetAttribute("cacheFile", cacheFile);
                if (File.Exists(cacheFile))
                {
                    span.SetAttribute("found", true);
                    await EnsureFileDeletedAsync(cacheFile, token);
                }
            }
            catch (Exception ex)
            {
                span.SetStatus(Status.Error);
                span.RecordException(ex);
            }
        }


        /// <summary>
        /// Removes files from the cache whose creation time is older than 30 days.
        /// </summary>
        public void PurgeOldFiles()
        {
            if (this.cacheSettings.PurgeExpirationInDays == null)
            {
                return;
            }

            using var span = this.diagnosticsConfig.StartNewSpan();
            int totalFilesRemoved = 0;
            var cacheFolder = this.cacheSettings.CacheFolder;
            if (Directory.Exists(cacheFolder))
            {
                var files = Directory.GetFiles(cacheFolder, "*.*", SearchOption.AllDirectories);
                var now = DateTime.UtcNow;

                foreach (var file in files)
                {
                    var cacheKey = FilePathToCacheKey(file, this.cacheSettings.CacheFolder);
                    try
                    {
                        var creationTime = File.GetCreationTimeUtc(file);
                        if ((now - creationTime).TotalDays > this.cacheSettings.PurgeExpirationInDays)
                        {
                            File.Delete(file);
                            this.diagnosticsConfig.OnCacheRemoved(cacheKey);
                            totalFilesRemoved++;
                        }
                    }
                    catch (Exception ex)
                    {
                        this.diagnosticsConfig.OnCacheError(cacheKey, $"Failed to delete file {file}: {ex.Message}");
                    }
                }
            }

            span.SetAttribute("totalFilesRemoved", totalFilesRemoved);
        }

        /// <summary>
        /// Deletes a file and ensures it is physically removed from the operating system.
        /// </summary>
        /// <param name="filePath">The path of the file to delete.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private static async Task EnsureFileDeletedAsync(string filePath, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
            }

            if (!File.Exists(filePath))
            {
                return; // File already deleted
            }

            try
            {
                // Attempt to delete the file
                File.Delete(filePath);

                // Poll to ensure the file is removed
                const int maxRetries = 10;
                const int delayBetweenRetriesMs = 100;

                for (int i = 0; i < maxRetries; i++)
                {
                    if (!File.Exists(filePath))
                    {
                        return; // File successfully deleted
                    }

                    // Wait before retrying
                    await Task.Delay(delayBetweenRetriesMs, cancellationToken);
                }

                // If the file still exists after retries, throw an exception
                if (File.Exists(filePath))
                {
                    throw new IOException($"Failed to delete file '{filePath}' after multiple attempts.");
                }
            }
            catch (Exception ex)
            {
                throw new IOException($"An error occurred while deleting the file '{filePath}': {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Converts a cache key to a valid file path.
        /// </summary>
        /// <param name="cacheKey">The cache key to convert.</param>
        /// <param name="cacheFolder">The base cache folder.</param>
        /// <returns>The full file path for the cache key.</returns>
        private static string CacheKeyToFilePath(string cacheKey, string cacheFolder)
        {
            if (string.IsNullOrWhiteSpace(cacheKey))
            {
                throw new ArgumentException("Cache key cannot be null or empty.", nameof(cacheKey));
            }

            // Replace invalid file path characters with underscores
            var sanitizedKey = cacheKey.Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar);

            return Path.Combine(cacheFolder, sanitizedKey);
        }

        /// <summary>
        /// Converts a file path back to a cache key.
        /// </summary>
        /// <param name="filePath">The file path to convert.</param>
        /// <param name="cacheFolder">The base cache folder.</param>
        /// <returns>The cache key corresponding to the file path.</returns>
        private static string FilePathToCacheKey(string filePath, string cacheFolder)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
            }

            if (!filePath.StartsWith(cacheFolder, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("File path does not belong to the cache folder.", nameof(filePath));
            }

            // Remove the cache folder prefix and normalize to use '/' as the separator
            var relativePath = filePath.Substring(cacheFolder.Length).TrimStart(Path.DirectorySeparatorChar);
            return relativePath.Replace(Path.DirectorySeparatorChar, '/');
        }
    }
}