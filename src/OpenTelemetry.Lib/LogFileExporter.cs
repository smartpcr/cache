// <copyright file="LogFileExporter.cs" company="Microsoft Corp">
// Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>

namespace OpenTelemetry.Lib;

using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;

/// <summary>
/// Export logs to file.
/// </summary>
public class LogFileExporter : BaseExporter<LogRecord>
{
    private readonly LogLevel logLevel;
    private readonly RollingFileLogger fileLogger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogFileExporter"/> class.
    /// </summary>
    /// <param name="fileSink">The file sink settings.</param>
    /// <param name="logLevel">The log level.</param>
    public LogFileExporter(FileSinkSettings fileSink, LogLevel logLevel)
    {
        this.logLevel = logLevel;
        this.fileLogger = new RollingFileLogger(fileSink, "log");
    }

    /// <summary>
    /// Export log record in batch.
    /// </summary>
    /// <param name="batch">Batch of log record.</param>
    /// <returns>Export result.</returns>
    public override ExportResult Export(in Batch<LogRecord> batch)
    {
        var fileLogEntries = new List<string>();
        foreach (var record in batch)
        {
            if (record.LogLevel < this.logLevel)
            {
                continue;
            }

            var logMessage = $"{record.Timestamp:o}: {record.CategoryName} [{record.LogLevel}] {record.FormattedMessage}\n";
            fileLogEntries.Add(logMessage);
        }

        if (fileLogEntries.Any())
        {
            this.fileLogger.Log(fileLogEntries);
        }

        return ExportResult.Success;
    }
}
