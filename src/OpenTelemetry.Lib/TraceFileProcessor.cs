// <copyright file="TraceFileProcessor.cs" company="Microsoft Corp">
// Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>

namespace OpenTelemetry.Lib;

using System.Diagnostics;
using Newtonsoft.Json;

/// <summary>
/// Persist trace to file.
/// </summary>
public class TraceFileProcessor : BaseProcessor<Activity>
{
    private readonly RollingFileLogger fileLogger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TraceFileProcessor"/> class.
    /// </summary>
    /// <param name="fileSink">The file sink settings.</param>
    public TraceFileProcessor(FileSinkSettings fileSink)
    {
        this.fileLogger = new RollingFileLogger(fileSink, "trace");
    }

    /// <summary>
    /// Flush activity data to file.
    /// </summary>
    /// <param name="data">The activity.</param>
    public override void OnEnd(Activity data)
    {
        var traceData = $"{DateTime.UtcNow:o} Id: {data.Id}, Trace: \n\t{JsonConvert.SerializeObject(data)}\n";
        this.fileLogger.Log(traceData);
    }
}
