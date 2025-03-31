// <copyright file="FileSinkSettings.cs" company="Microsoft Corp">
// Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>

namespace OpenTelemetry.Lib;

/// <summary>
/// When opentelemetry sink type is configured to file, this class is used to configure the file sink settings
/// for logging, metrics and traces.
/// </summary>
public class FileSinkSettings
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FileSinkSettings"/> class.
    /// </summary>
    /// <param name="filePrefix">The file prefix.</param>
    public FileSinkSettings(string filePrefix)
    {
        this.FilePrefix = filePrefix;
    }

    /// <summary>
    /// Gets or sets log file name prefix, in the format of {prefix}_{date}_001.log.
    /// </summary>
    public string? FilePrefix { get; set; }

    /// <summary>
    /// Gets or sets log file extension, default is log.
    /// </summary>
    public string? FileExtension { get; set; } = "log";

    /// <summary>
    /// Gets or sets max file size in MB, when exceeded, a new file is created.
    /// </summary>
    public int MaxFileSizeMb { get; set; } = 10;

    /// <summary>
    /// Gets or sets max entries in a file, when exceeded, a new file is created.
    /// </summary>
    public int MaxEntriesInFile { get; set; } = 5000;

    /// <summary>
    /// Gets or sets max file retention in days, when exceeded, files are deleted.
    /// </summary>
    public int MaxFileRetentionInDays { get; set; } = 7;

    /// <summary>
    /// Gets or sets the maximum number of files to retain in the log folder.
    /// </summary>
    /// <remarks>
    /// This property determines the maximum number of log files that will be retained in the log folder.
    /// If the number of log files exceeds the maximum file count, the oldest log files will be deleted to ensure that the total number of files does not exceed the limit.
    /// The default value is 10.
    /// </remarks>
    /// <value>The maximum number of log files to retain.</value>
    public int MaxFileCount { get; set; } = 10;

    /// <summary>
    /// Gets or sets the monitoring folder, otherwise, files are written to base directory of the running application, which can be messy
    /// otherwise, files are written to the specified folder.
    /// </summary>
    public string MonitoringFolder { get; set; } = "logs";
}
