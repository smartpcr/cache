// <copyright file="RollingFileLogger.cs" company="Microsoft Corp">
// Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>

namespace OpenTelemetry.Lib;

/// <summary>
/// Keep log entries in a rolling file.
/// </summary>
public class RollingFileLogger
{
    private static readonly SemaphoreSlim FileLock = new SemaphoreSlim(1, 1);
    private readonly string filePrefix;
    private readonly string fileExtension;
    private readonly string currentDirectory;
    private readonly long maxFileSize;
    private readonly int maxEntriesInFile;
    private readonly int maxFileCount;
    private readonly int maxRetentionInDays;
    private string currentLogFile;
    private int fileIndex;
    private DateTime currentDate;
    private int totalLogEntries;

    /// <summary>
    /// Initializes a new instance of the <see cref="RollingFileLogger"/> class.
    /// </summary>
    /// <param name="fileSink">The file sink settings.</param>
    /// <param name="defaultFilePrefix">File prefix.</param>
    public RollingFileLogger(FileSinkSettings fileSink, string defaultFilePrefix)
    {
        this.filePrefix = fileSink.FilePrefix ?? defaultFilePrefix;
        this.fileExtension = fileSink.FileExtension ?? "log";
        this.maxFileCount = fileSink.MaxFileCount;
        this.maxRetentionInDays = fileSink.MaxFileRetentionInDays;
        this.fileIndex = 0;
        this.totalLogEntries = 0;
        this.currentDate = DateTime.UtcNow.Date;

        this.maxFileSize = fileSink.MaxFileSizeMb * 1024 * 1024;
        this.maxEntriesInFile = fileSink.MaxEntriesInFile;
        this.currentDirectory = string.IsNullOrEmpty(fileSink.MonitoringFolder)
            ? Directory.GetCurrentDirectory()
            : fileSink.MonitoringFolder;
        if (!Directory.Exists(this.currentDirectory))
        {
            Directory.CreateDirectory(this.currentDirectory);
        }

        this.currentLogFile = this.CreateWriter();
    }

    /// <summary>
    /// Log the message to file.
    /// </summary>
    /// <param name="message">The log message.</param>
    public void Log(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return;
        }

        FileLock.Wait();

        try
        {
            if (DateTime.UtcNow.Date != this.currentDate)
            {
                this.currentDate = DateTime.UtcNow.Date;
                this.fileIndex = 0;
                this.PurgeOldFiles();
                this.currentLogFile = this.CreateWriter();
                this.totalLogEntries = 0;
            }

            // retry with max of 3 attempts, if file is locked by another process
            // wait for 100ms before retrying
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    File.AppendAllLines(this.currentLogFile, new[] { message });
                    break;
                }
                catch (IOException)
                {
                    Thread.Sleep(100);
                }
            }

            this.totalLogEntries++;

            if (this.totalLogEntries > this.maxEntriesInFile && new FileInfo(this.currentLogFile).Length > this.maxFileSize)
            {
                this.currentLogFile = this.CreateWriter();
            }
        }
        finally
        {
            FileLock.Release();
        }
    }

    /// <summary>
    /// Log the messages to file.
    /// </summary>
    /// <param name="messages">List of messages.</param>
    public void Log(List<string>? messages)
    {
        if (messages == null || !messages.Any())
        {
            return;
        }

        FileLock.Wait();
        try
        {
            foreach (var message in messages)
            {
                if (DateTime.UtcNow.Date != this.currentDate)
                {
                    this.currentDate = DateTime.UtcNow.Date;
                    this.fileIndex = 0;
                    this.PurgeOldFiles();
                    this.currentLogFile = this.CreateWriter();
                    this.totalLogEntries = 0;
                }

                File.AppendAllLines(this.currentLogFile, new[] { message });
                this.totalLogEntries++;

                if (this.totalLogEntries > this.maxEntriesInFile && new FileInfo(this.currentLogFile).Length > this.maxFileSize)
                {
                    this.currentLogFile = this.CreateWriter();
                }
            }
        }
        finally
        {
            FileLock.Release();
        }
    }

    private string CreateWriter()
    {
        var logFilePath = Path.Combine(this.currentDirectory, $"{this.filePrefix}_{this.currentDate:yyyy_MM_dd}_{this.fileIndex:000}.{this.fileExtension}");
        var fileInfo = new FileInfo(logFilePath);
        if (!fileInfo.Exists)
        {
            // create empty file
            File.WriteAllText(logFilePath, string.Empty);
            return logFilePath;
        }

        if (fileInfo.Length < this.maxFileSize)
        {
            return logFilePath;
        }

        while (fileInfo.Exists && fileInfo.Length >= this.maxFileSize)
        {
            this.fileIndex++;
            logFilePath = Path.Combine(this.currentDirectory, $"{this.filePrefix}_{this.currentDate:yyyy_MM_dd}_{this.fileIndex:000}.{this.fileExtension}");
            fileInfo = new FileInfo(logFilePath);
        }

        File.WriteAllText(logFilePath, string.Empty);
        return logFilePath;
    }

    private void PurgeOldFiles()
    {
        var files = Directory.GetFiles(this.currentDirectory, $"*.{this.fileExtension}");
        if (files.Length > this.maxFileCount)
        {
            // Sort by creation time, remove the oldest file until files count is less than max file count
            files.OrderBy(File.GetCreationTimeUtc)
                .Take(files.Length - this.maxFileCount)
                .ToList()
                .ForEach(File.Delete);
            files = Directory.GetFiles(this.currentDirectory, $"*.{this.fileExtension}");
        }

        foreach (var file in files)
        {
            var creationTime = File.GetCreationTimeUtc(file);
            if (creationTime.AddDays(this.maxRetentionInDays) < DateTime.UtcNow)
            {
                File.Delete(file);
            }
        }
    }
}
