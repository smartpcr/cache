//-------------------------------------------------------------------------------
// <copyright file="DiagnosticsConfig.cs" company="Microsoft Corp">
// Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
//-------------------------------------------------------------------------------

namespace OpenTelemetry.Lib;

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Net;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

/// <summary>
/// DiagnosticsConfig is a singleton class that provides a set of diagnostics utility functions.
/// It contains the following disposable objects:
/// ILoggerFactory is used to create loggers for different types.
/// Meter is used to create metrics for the application.
/// We do not explicitly dispose these objects, as DiagnosticsConfig is a singleton and
/// will be disposed when the application exits.
/// </summary>
public class DiagnosticsConfig
{
    public const string TraceParentHeader = "traceparent";
    public const string TraceStateHeader = "tracestate";

    // http stack counters
    private readonly Counter<long> httpClientRequestCounter;
    private readonly Counter<long> httpClientRequestLatencyCounter;
    private readonly Counter<long> httpServerRequestCounter;
    private readonly Counter<long> httpServerRequestLatencyCounter;

    // tasks
    private long taskTrackingId = 0;
    private readonly ConcurrentDictionary<long, DateTime> runningTasks = new ConcurrentDictionary<long, DateTime>();
    private readonly Counter<long> scheduledTasksCounter;
    private readonly Counter<long> finishedTasksCounter;
    private readonly Counter<long> failedTasksCounter;

    // synclocks
    private long lockTrackingId = 0;
    private readonly ConcurrentDictionary<long, DateTime> activeLocks = new ConcurrentDictionary<long, DateTime>();
    private readonly Counter<long> enterLockCounter;
    private readonly Counter<long> exitLockCounter;

    // cache
    private readonly Counter<long> cacheUpsertCounter;
    private readonly Counter<long> cacheHitCounter;
    private readonly Counter<long> cacheMissCounter;
    private readonly Counter<long> cacheExpiredCounter;
    private readonly Counter<long> cacheRemovedCounter;
    private readonly Counter<long> cacheErrorCounter;

    public DiagnosticsConfig(
        Dictionary<string, string> configuration,
        string serviceName = ApplicationMetadata.ServiceName,
        string serviceVersion = ApplicationMetadata.ServiceVersion)
    {
        LoggerFactory = OtelBuilder.SetupLogger(configuration, serviceName, serviceVersion);
        MeterProvider = OtelBuilder.SetupMetrics(configuration, serviceName, serviceVersion);
        TracerProvider = OtelBuilder.SetupTracing(configuration, serviceName, serviceVersion);
        Tracer = TracerProvider.GetTracer(serviceName);
        Meter = new Meter(serviceName);

        this.httpClientRequestCounter = Meter.CreateCounter<long>(
            "hci.http.client.request.count",
            "count",
            "The number of HTTP client requests");
        this.httpClientRequestLatencyCounter = Meter.CreateCounter<long>(
            "hci.http.client.request.latency",
            "ms",
            "The duration of HTTP client request in milliseconds");
        this.httpServerRequestCounter = Meter.CreateCounter<long>(
            "hci.http.server.request.count",
            "count",
            "The number of HTTP server requests");
        this.httpServerRequestLatencyCounter = Meter.CreateCounter<long>(
            "hci.http.server.request.latency",
            "ms",
            "The duration of HTTP server request in milliseconds");

        this.scheduledTasksCounter = Meter.CreateCounter<long>(
            "hci.tasks.scheduled",
            "count",
            "total scheduled tasks");
        this.finishedTasksCounter = Meter.CreateCounter<long>(
            "hci.tasks.finished",
            "count",
            "total finished tasks");
        this.failedTasksCounter = Meter.CreateCounter<long>(
            "hci.tasks.failed",
            "count",
            "total failed tasks");

        this.enterLockCounter = Meter.CreateCounter<long>(
            "hci.lock.enter",
            "count",
            "total locks entered");
        this.exitLockCounter = Meter.CreateCounter<long>(
            "hci.lock.exit",
            "count",
            "total locks exited");

        this.cacheUpsertCounter = Meter.CreateCounter<long>(
            "hci.cache.upsert",
            "count",
            "total cache upserts");
        this.cacheHitCounter = Meter.CreateCounter<long>(
            "hci.cache.hit",
            "count",
            "total cache hits");
        this.cacheMissCounter = Meter.CreateCounter<long>(
            "hci.cache.miss",
            "count",
            "total cache misses");
        this.cacheExpiredCounter = Meter.CreateCounter<long>(
            "hci.cache.expired",
            "count",
            "total cache expired");
        this.cacheRemovedCounter = Meter.CreateCounter<long>(
            "hci.cache.removed",
            "count",
            "total cache removed");
        this.cacheErrorCounter = Meter.CreateCounter<long>(
            "hci.cache.error",
            "count",
            "total cache errors");

        DiagnosticsConfig.Instance = this;
    }

    /// <summary>
    /// this is used for BVTs
    /// </summary>
    /// <param name="otelEndpoint">Local OLTP receiver endpoint.</param>
    /// <returns></returns>
    public static DiagnosticsConfig GetEnabledInstance(string otelEndpoint) =>
        new DiagnosticsConfig(new Dictionary<string, string>()
        {
            { OtelSettings.OtelEnabledParameter, "true" },
            { OtelSettings.SinkTypesParameter, OtelSinkTypes.OTLP.ToString() },
            { OtelSettings.OtelEndpointParameter, otelEndpoint }
        });

    /// <summary>
    /// Singleton instance of DiagnosticsConfig.
    /// </summary>
    public static DiagnosticsConfig Instance { get; private set; } = new DiagnosticsConfig(new Dictionary<string, string>()); // default instance

    /// <summary>
    /// Always return new instance for test isolation and prevent being disposed prematurely.
    /// </summary>
    public static DiagnosticsConfig NewTestInstance => new DiagnosticsConfig(new Dictionary<string, string>());

    public delegate bool TryGetHeaderValueDelegate(string key, out string value);

    public ILoggerFactory LoggerFactory { get; private set; }

    public MeterProvider MeterProvider { get; private set; }

    public TracerProvider TracerProvider { get; private set; }

    public Tracer Tracer { get; private set; }

    public Meter Meter { get; private set; }

    public ILogger<T> GetLogger<T>()
    {
        return this.LoggerFactory.CreateLogger<T>();
    }

    public ILogger GetLogger(string typeName)
    {
        return this.LoggerFactory.CreateLogger(typeName);
    }

    public Counter<long> GetCounter(string name, string description, List<KeyValuePair<string, object>> tags)
    {
        return this.Meter.CreateCounter<long>(name, null, description, tags);
    }

    public Histogram<double> GetHistogram(string name, string unit, string description, List<KeyValuePair<string, object>> tags)
    {
        return this.Meter.CreateHistogram<double>(name, unit, description, tags);
    }

    public TelemetrySpan StartActiveSpan(
        string methodName,
        SpanKind spanKind = SpanKind.Internal,
        [CallerFilePath] string filePath = "",
        [CallerMemberName] string memberName = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        var span = this.Tracer.StartActiveSpan($"{fileName}.{methodName}", spanKind);
        span.SetAttribute("file", fileName);
        span.SetAttribute("line", lineNumber);
        span.SetAttribute("method", memberName);
        return span;
    }

    public TelemetrySpan StartNewSpan(
        [CallerFilePath] string filePath = "",
        [CallerMemberName] string memberName = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        var span = this.Tracer.StartActiveSpan($"{fileName}.{memberName}");
        span.SetAttribute("file", fileName);
        span.SetAttribute("line", lineNumber);
        span.SetAttribute("method", memberName);
        return span;
    }

    public TelemetrySpan StartNewRootSpan(
        [CallerFilePath] string filePath = "",
        [CallerMemberName] string memberName = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        var span = this.Tracer.StartRootSpan($"{fileName}.{memberName}");
        span.SetAttribute("file", fileName);
        span.SetAttribute("line", lineNumber);
        span.SetAttribute("method", memberName);
        return span;
    }

    public TelemetrySpan StartWithParent(
        SpanContext parentSpanContext,
        SpanContext linkedSpanContext = default,
        [CallerFilePath] string filePath = "",
        [CallerMemberName] string memberName = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        var links = new List<Link>();
        if (linkedSpanContext.IsValid)
        {
            links.Add(new Link(linkedSpanContext));
        }

        var span = this.Tracer.StartActiveSpan($"{fileName}.{memberName}", SpanKind.Internal, parentSpanContext, links: links);
        span.SetAttribute("file", fileName);
        span.SetAttribute("line", lineNumber);
        span.SetAttribute("method", memberName);
        return span;
    }

    public TelemetrySpan StartControllerSpan(
        HttpListenerContext listenerContext,
        [CallerFilePath] string filePath = "",
        [CallerMemberName] string memberName = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        bool TryGetHeaderValue(string key, out string value)
        {
            if (listenerContext.Request?.Headers.HasKeys() == true && listenerContext.Request.Headers[key] != null)
            {
                value = listenerContext.Request.Headers[key];
                return !string.IsNullOrEmpty(value);
            }

            value = null;
            return false;
        }

        var httpMethod = listenerContext.Request?.HttpMethod;
        var requestUri = listenerContext.Request?.RawUrl;
        return StartControllerSpanInternal(this.Tracer, TryGetHeaderValue, httpMethod ?? "unknown", requestUri ?? "unknown", filePath, memberName, lineNumber);
    }

    public TelemetrySpan StartHttpClientSpan(
        HttpRequestHeaders headers,
        Uri baseUri,
        [CallerFilePath] string filePath = "",
        [CallerMemberName] string memberName = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        var span = this.Tracer.StartActiveSpan($"{fileName}.{memberName}", SpanKind.Client);
        span.SetAttribute("file", fileName);
        span.SetAttribute("line", lineNumber);
        span.SetAttribute("method", memberName);
        span.SetAttribute("http.url", baseUri.AbsoluteUri);
        InjectSpanContext(span, headers);
        return span;
    }

    #region http stack

    public void HttpClientRequestFinished(string method, string requestUri, int statusCode, long durationMs)
    {
        // trim query parameters
        if (requestUri.Contains('?'))
        {
            requestUri = requestUri.Substring(0, requestUri.IndexOf('?'));
        }

        this.httpClientRequestCounter.Add(
            1,
            new KeyValuePair<string, object>("http.method", method),
            new KeyValuePair<string, object>("http.request.uri", requestUri),
            new KeyValuePair<string, object>("http.response.status_code", statusCode));

        this.httpClientRequestLatencyCounter.Add(
            durationMs,
            new KeyValuePair<string, object>("http.method", method),
            new KeyValuePair<string, object>("http.request.uri", requestUri),
            new KeyValuePair<string, object>("http.response.status_code", statusCode));
    }

    public void HttpServerRequestFinished(string method, string requestUri, int statusCode, long durationMs)
    {
        // trim query parameters
        if (requestUri?.Contains('?') == true)
        {
            requestUri = requestUri.Substring(0, requestUri.IndexOf('?'));
        }

        this.httpServerRequestCounter.Add(
            1,
            new KeyValuePair<string, object>("http.method", method),
            new KeyValuePair<string, object>("http.request.uri", requestUri),
            new KeyValuePair<string, object>("http.response.status_code", statusCode));

        this.httpServerRequestLatencyCounter.Add(
            durationMs,
            new KeyValuePair<string, object>("http.method", method),
            new KeyValuePair<string, object>("http.request.uri", requestUri),
            new KeyValuePair<string, object>("http.response.status_code", statusCode));
    }

    #endregion

    #region tasks

    /// <summary>
    /// register a task in memory.
    /// </summary>
    /// <param name="name">The name of task.</param>
    /// <param name="filePath">The caller file path.</param>
    /// <param name="memberName">Caller method name.</param>
    /// <param name="lineNumber">Caller line number.</param>
    /// <returns>An automic sequence number for tracking.</returns>
    public long OnTaskCreated(
        string name,
        [CallerFilePath] string filePath = "",
        [CallerMemberName] string memberName = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        var trackingId = Interlocked.Increment(ref this.taskTrackingId);
        if (this.runningTasks.TryAdd(trackingId, DateTime.UtcNow))
        {
            this.scheduledTasksCounter.Add(
                1,
                new KeyValuePair<string, object>("name", name),
                new KeyValuePair<string, object>("filePath", filePath),
                new KeyValuePair<string, object>("methodName", memberName),
                new KeyValuePair<string, object>("lineNumber", lineNumber));
            return trackingId;
        }

        return 0;
    }

    public void OnTaskFinished(
        long trackingId,
        string name,
        [CallerFilePath] string filePath = "",
        [CallerMemberName] string memberName = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        if (this.runningTasks.TryRemove(trackingId, out var creationTime))
        {
            var durationInMs = (long)(DateTime.UtcNow - creationTime).TotalMilliseconds;
            this.finishedTasksCounter.Add(
                1,
                new KeyValuePair<string, object>("name", name),
                new KeyValuePair<string, object>("durationMS", durationInMs),
                new KeyValuePair<string, object>("filePath", filePath),
                new KeyValuePair<string, object>("methodName", memberName),
                new KeyValuePair<string, object>("lineNumber", lineNumber));
        }
    }

    public void OnTaskFailed(
        long trackingId,
        string name,
        string error,
        [CallerFilePath] string filePath = "",
        [CallerMemberName] string memberName = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        if (this.runningTasks.TryRemove(trackingId, out var creationTime))
        {
            var durationInMs = (long)(DateTime.UtcNow - creationTime).TotalMilliseconds;
            this.failedTasksCounter.Add(
                1,
                new KeyValuePair<string, object>("name", name),
                new KeyValuePair<string, object>("durationMS", durationInMs),
                new KeyValuePair<string, object>("error",
                    string.IsNullOrWhiteSpace(error)
                        ? "none"
                        : error),
                new KeyValuePair<string, object>("filePath", filePath),
                new KeyValuePair<string, object>("methodName", memberName),
                new KeyValuePair<string, object>("lineNumber", lineNumber));
        }
    }

    public int GetActiveTasks()
    {
        return this.runningTasks.Count;
    }

    public int GetLongRunningTasks(TimeSpan span)
    {
        var now = DateTime.UtcNow;
        return this.runningTasks.Count(t => now - t.Value >= span);
    }

    #endregion

    #region locks

    public long OnEnterLock(
        string name,
        [CallerFilePath] string filePath = "",
        [CallerMemberName] string memberName = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        var trackingId = Interlocked.Increment(ref this.lockTrackingId);
        if (this.activeLocks.TryAdd(trackingId, DateTime.UtcNow))
        {
            this.enterLockCounter.Add(
                1,
                new KeyValuePair<string, object>("name", name),
                new KeyValuePair<string, object>("filePath", filePath),
                new KeyValuePair<string, object>("methodName", memberName),
                new KeyValuePair<string, object>("lineNumber", lineNumber));
            return trackingId;
        }

        return 0;
    }

    public void OnExitLock(
        long trackingId,
        string name,
        [CallerFilePath] string filePath = "",
        [CallerMemberName] string memberName = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        if (this.activeLocks.TryRemove(trackingId, out var creationTime))
        {
            var durationInMs = (long)(DateTime.UtcNow - creationTime).TotalMilliseconds;
            this.exitLockCounter.Add(
                1,
                new KeyValuePair<string, object>("name", name),
                new KeyValuePair<string, object>("durationMS", durationInMs),
                new KeyValuePair<string, object>("filePath", filePath),
                new KeyValuePair<string, object>("methodName", memberName),
                new KeyValuePair<string, object>("lineNumber", lineNumber));
        }
    }

    public int GetActiveLocks()
    {
        return this.activeLocks.Count;
    }

    public int GetDeadLocks(TimeSpan span)
    {
        var now = DateTime.UtcNow;
        return this.activeLocks.Count(l => now - l.Value > span);
    }

    #endregion

    #region cache

    public void OnCacheUpsert(
        string cacheKey,
        TimeSpan expirationSpan,
        [CallerFilePath] string filePath = "",
        [CallerMemberName] string memberName = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        this.cacheUpsertCounter.Add(
            1,
            new KeyValuePair<string, object>("key", cacheKey),
            new KeyValuePair<string, object>("expirationSpan", expirationSpan.ToString()),
            new KeyValuePair<string, object>("filePath", filePath),
            new KeyValuePair<string, object>("methodName", memberName),
            new KeyValuePair<string, object>("lineNumber", lineNumber));
    }

    public void OnCacheHit(
        string cacheKey,
        [CallerFilePath] string filePath = "",
        [CallerMemberName] string memberName = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        this.cacheHitCounter.Add(
            1,
            new KeyValuePair<string, object>("key", cacheKey),
            new KeyValuePair<string, object>("filePath", filePath),
            new KeyValuePair<string, object>("methodName", memberName),
            new KeyValuePair<string, object>("lineNumber", lineNumber));
    }

    public void OnCacheMiss(
        string cacheKey,
        [CallerFilePath] string filePath = "",
        [CallerMemberName] string memberName = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        this.cacheMissCounter.Add(
            1,
            new KeyValuePair<string, object>("key", cacheKey),
            new KeyValuePair<string, object>("filePath", filePath),
            new KeyValuePair<string, object>("methodName", memberName),
            new KeyValuePair<string, object>("lineNumber", lineNumber));
    }

    public void OnCacheExpired(
        string cacheKey,
        [CallerFilePath] string filePath = "",
        [CallerMemberName] string memberName = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        this.cacheExpiredCounter.Add(
            1,
            new KeyValuePair<string, object>("key", cacheKey),
            new KeyValuePair<string, object>("filePath", filePath),
            new KeyValuePair<string, object>("methodName", memberName),
            new KeyValuePair<string, object>("lineNumber", lineNumber));
    }

    public void OnCacheRemoved(
        string cacheKey,
        [CallerFilePath] string filePath = "",
        [CallerMemberName] string memberName = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        this.cacheRemovedCounter.Add(
            1,
            new KeyValuePair<string, object>("key", cacheKey),
            new KeyValuePair<string, object>("filePath", filePath),
            new KeyValuePair<string, object>("methodName", memberName),
            new KeyValuePair<string, object>("lineNumber", lineNumber));
    }

    public void OnCacheError(
        string cacheKey,
        string error,
        [CallerFilePath] string filePath = "",
        [CallerMemberName] string memberName = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        this.cacheErrorCounter.Add(
            1,
            new KeyValuePair<string, object>("key", cacheKey),
            new KeyValuePair<string, object>("error", error),
            new KeyValuePair<string, object>("filePath", filePath),
            new KeyValuePair<string, object>("methodName", memberName),
            new KeyValuePair<string, object>("lineNumber", lineNumber));
    }

    #endregion

    private void InjectSpanContext(TelemetrySpan span, HttpRequestHeaders headers)
    {
        var traceParent = $"00-{span.Context.TraceId}-{span.Context.SpanId}-01";
        headers.Add(TraceParentHeader, traceParent);

        if (span.Context.IsValid)
        {
            var traceStateValue = string.Join(",", span.Context.TraceState.Select(entry => $"{entry.Key}={entry.Value}"));
            headers.Add(TraceStateHeader, traceStateValue);
        }
    }

    private TelemetrySpan StartControllerSpanInternal(
        Tracer tracer,
        TryGetHeaderValueDelegate tryGetHeaderValue,
        string httpMethod,
        string requestUri,
        string filePath,
        string memberName,
        int lineNumber)
    {
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        SpanContext? parentSpanContext = null;

        if (tryGetHeaderValue(TraceParentHeader, out var traceParentHeader))
        {
            if (!string.IsNullOrEmpty(traceParentHeader))
            {
                var traceParent = traceParentHeader.Split('-');
                if (traceParent.Length == 4)
                {
                    var traceId = traceParent[1];
                    var spanId = traceParent[2];
                    var traceFlags = traceParent[3];

                    if (!traceId.IsEmptyTraceId() && !spanId.IsEmptySpanId())
                    {
                        var parentActivityContext = new ActivityContext(
                            ActivityTraceId.CreateFromString(traceId.AsSpan()),
                            ActivitySpanId.CreateFromString(spanId.AsSpan()),
                            traceFlags == "01"
                                ? ActivityTraceFlags.Recorded
                                : ActivityTraceFlags.None);
                        parentSpanContext = new SpanContext(
                            traceId: parentActivityContext.TraceId,
                            spanId: parentActivityContext.SpanId,
                            traceFlags: parentActivityContext.TraceFlags,
                            isRemote: true);

                        if (tryGetHeaderValue(TraceStateHeader, out var tracestateHeaderValue))
                        {
                            if (!string.IsNullOrEmpty(tracestateHeaderValue))
                            {
                                var kvpList = new List<KeyValuePair<string, string>>();
                                foreach (var kvp in tracestateHeaderValue.Split(','))
                                {
                                    var keyValuePair = kvp.Split('=');
                                    if (keyValuePair.Length == 2)
                                    {
                                        kvpList.Add(new KeyValuePair<string, string>(keyValuePair[0], keyValuePair[1]));
                                    }
                                }

                                parentSpanContext = new SpanContext(
                                    traceId: parentActivityContext.TraceId,
                                    spanId: parentActivityContext.SpanId,
                                    traceFlags: parentActivityContext.TraceFlags,
                                    traceState: kvpList,
                                    isRemote: true);
                            }
                        }
                    }
                }
            }
        }

        var span = parentSpanContext == null
            ? tracer.StartRootSpan($"{fileName}.{memberName}", SpanKind.Server)
            : tracer.StartActiveSpan($"{fileName}.{memberName}", SpanKind.Server, (SpanContext)parentSpanContext);
        span.SetAttribute("http.method", httpMethod);
        span.SetAttribute("http.url", requestUri);
        span.SetAttribute("file", fileName);
        span.SetAttribute("line", lineNumber);
        span.SetAttribute("method", memberName);
        return span;
    }
}