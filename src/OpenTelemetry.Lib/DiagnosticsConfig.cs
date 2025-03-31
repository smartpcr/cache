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
/// This is singleton instance registered in the application. It is responsible for setting up
/// tracing, metrics and logging.
/// </summary>
public class DiagnosticsConfig : IDisposable
{
    /// <summary>
    /// Http header name for trace parent.
    /// </summary>
    public const string TraceParentHeader = "traceparent";

    /// <summary>
    /// Http header name for trace state.
    /// </summary>
    public const string TraceStateHeader = "tracestate";

    // http stack counters
    private readonly Counter<long> httpClientRequestCounter;
    private readonly Counter<long> httpClientRequestLatencyCounter;
    private readonly Counter<long> httpServerRequestCounter;
    private readonly Counter<long> httpServerRequestLatencyCounter;

    // tasks
    private long taskTrackingId;
    private readonly ConcurrentDictionary<long, DateTime> runningTasks = new ConcurrentDictionary<long, DateTime>();
    private readonly Counter<long> scheduledTasksCounter;
    private readonly Counter<long> finishedTasksCounter;
    private readonly Counter<long> failedTasksCounter;

    // synclocks
    private long lockTrackingId;
    private readonly ConcurrentDictionary<long, DateTime> activeLocks = new ConcurrentDictionary<long, DateTime>();
    private readonly Counter<long> enterLockCounter;
    private readonly Counter<long> exitLockCounter;

    // cache
    private readonly Counter<long> cacheUpsertCounter;
    private readonly Counter<long> cacheHitCounter;
    private readonly Counter<long> cacheMissCounter;
    private readonly Counter<long> cacheExpiredCounter;

    /// <summary>
    /// Initializes a new instance of the <see cref="DiagnosticsConfig"/> class.
    /// </summary>
    /// <param name="configuration">Configuration settings.</param>
    /// <param name="serviceName">The service name.</param>
    /// <param name="serviceVersion">The service version.</param>
    public DiagnosticsConfig(
        Dictionary<string, string> configuration,
        string serviceName = ApplicationMetadata.ServiceName,
        string serviceVersion = ApplicationMetadata.ServiceVersion)
    {
        this.LoggerFactory = OtelBuilder.SetupLogger(configuration, serviceName, serviceVersion);
        this.MeterProvider = OtelBuilder.SetupMetrics(configuration, serviceName, serviceVersion);
        this.TracerProvider = OtelBuilder.SetupTracing(configuration, serviceName, serviceVersion);
        this.Tracer = this.TracerProvider.GetTracer(serviceName);
        this.Meter = new Meter(serviceName);

        // http stack counters
        this.httpClientRequestCounter = this.Meter.CreateCounter<long>(
            "hci.http.client.request.count",
            "count",
            "The number of HTTP client requests");
        this.httpClientRequestLatencyCounter = this.Meter.CreateCounter<long>(
            "hci.http.client.request.latency",
            "ms",
            "The duration of HTTP client request in milliseconds");
        this.httpServerRequestCounter = this.Meter.CreateCounter<long>(
            "hci.http.server.request.count",
            "count",
            "The number of HTTP server requests");
        this.httpServerRequestLatencyCounter = this.Meter.CreateCounter<long>(
            "hci.http.server.request.latency",
            "ms",
            "The duration of HTTP server request in milliseconds");

        // tasks
        this.scheduledTasksCounter = this.Meter.CreateCounter<long>(
            "hci.tasks.scheduled",
            "count",
            "total scheduled tasks");
        this.finishedTasksCounter = this.Meter.CreateCounter<long>(
            "hci.tasks.finished",
            "count",
            "total finished tasks");
        this.failedTasksCounter = this.Meter.CreateCounter<long>(
            "hci.tasks.failed",
            "count",
            "total failed tasks");

        // locks
        this.enterLockCounter = this.Meter.CreateCounter<long>(
            "hci.lock.enter",
            "count",
            "total locks entered");
        this.exitLockCounter = this.Meter.CreateCounter<long>(
            "hci.lock.exit",
            "count",
            "total locks exited");

        // cache
        this.cacheUpsertCounter = this.Meter.CreateCounter<long>(
            "hci.cache.upsert",
            "count",
            "total cache upserts");
        this.cacheHitCounter = this.Meter.CreateCounter<long>(
            "hci.cache.hit",
            "count",
            "total cache hits");
        this.cacheMissCounter = this.Meter.CreateCounter<long>(
            "hci.cache.miss",
            "count",
            "total cache misses");
        this.cacheExpiredCounter = this.Meter.CreateCounter<long>(
            "hci.cache.expired",
            "count",
            "total cache expired");

        DiagnosticsConfig.Instance = this;
    }

    /// <summary>
    /// Gets a singleton instance of <see cref="DiagnosticsConfig"/>.
    /// </summary>
    public static DiagnosticsConfig Instance { get; private set; } = new DiagnosticsConfig(new Dictionary<string, string>()); // default instance

    /// <summary>
    /// this is used for BVTs.
    /// </summary>
    /// <param name="otelEndpoint">Local OLTP receiver endpoint.</param>
    /// <returns>An instance of <see cref="DiagnosticsConfig"/>.</returns>
    public static DiagnosticsConfig GetEnabledInstance(string otelEndpoint) =>
        new DiagnosticsConfig(new Dictionary<string, string>()
        {
            { OtelSettings.OtelEnabledParameter, "true" },
            { OtelSettings.SinkTypesParameter, OtelSinkTypes.OTLP.ToString() },
            { OtelSettings.OtelEndpointParameter, otelEndpoint },
        });

    /// <summary>
    /// Delegate to get header value from incoming request.
    /// </summary>
    /// <param name="key">The header key used to lookup header value.</param>
    /// <param name="value">Retrieved value if found in headers.</param>
    /// <returns>A flag indicating if header exists by key.</returns>
    public delegate bool TryGetHeaderValueDelegate(string key, out string? value);

    /// <summary>
    /// Gets the injected singleton logger factory.
    /// </summary>
    public ILoggerFactory LoggerFactory { get; private set; }

    /// <summary>
    /// Gets the injected singleton meter provider.
    /// </summary>
    public MeterProvider MeterProvider { get; private set; }

    /// <summary>
    /// Gets the injected singleton tracer provider.
    /// </summary>
    public TracerProvider TracerProvider { get; private set; }

    /// <summary>
    /// Gets the injected singleton tracer.
    /// </summary>
    public Tracer Tracer { get; private set; }

    /// <summary>
    /// Gets the injected singleton meter.
    /// </summary>
    public Meter Meter { get; private set; }

    /// <summary>
    /// Gets the logger for the specified type.
    /// </summary>
    /// <typeparam name="T">Type.</typeparam>
    /// <returns>An instance of logger of specified type.</returns>
    public ILogger<T> GetLogger<T>() => this.LoggerFactory.CreateLogger<T>();

    /// <summary>
    /// Gets the logger for the specified type name.
    /// </summary>
    /// <param name="typeName">Type name.</param>
    /// <returns>An instance of logger of specified type name.</returns>
    public ILogger GetLogger(string typeName) => this.LoggerFactory.CreateLogger(typeName);

    /// <summary>
    /// Get the counter instance.
    /// </summary>
    /// <param name="name">The counter name.</param>
    /// <param name="description">Counter description.</param>
    /// <param name="tags">Counter tags.</param>
    /// <returns>An instance of <see cref="Counter{T}"/>.</returns>
    public Counter<long> GetCounter(string name, string description, List<KeyValuePair<string, object?>>? tags) =>
        this.Meter.CreateCounter<long>(name, null, description, tags);

    /// <summary>
    /// Get the histogram instance.
    /// </summary>
    /// <param name="name">Histogram name.</param>
    /// <param name="unit">Histogram unit.</param>
    /// <param name="description">Histogram description.</param>
    /// <param name="tags">Histogram tags.</param>
    /// <returns>An instance of <see cref="Histogram{T}"/>.</returns>
    public Histogram<double> GetHistogram(string name, string unit, string description, List<KeyValuePair<string, object?>>? tags) =>
        this.Meter.CreateHistogram<double>(name, unit, description, tags);

    /// <summary>
    /// Start new span with the specified method name.
    /// </summary>
    /// <param name="methodName">The method name to be included as span name.</param>
    /// <param name="spanKind">The span kind.</param>
    /// <param name="filePath">Caller file path.</param>
    /// <param name="memberName">Caller method name.</param>
    /// <param name="lineNumber">Caller line number.</param>
    /// <returns>An instance of <see cref="TelemetrySpan"/>.</returns>
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

    /// <summary>
    /// Start new span from caller.
    /// </summary>
    /// <param name="filePath">Caller file path.</param>
    /// <param name="memberName">Caller method name.</param>
    /// <param name="lineNumber">Caller line number.</param>
    /// <returns>An instance of <see cref="TelemetrySpan"/>.</returns>
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

    /// <summary>
    /// Start a new root span.
    /// </summary>
    /// <param name="filePath">Caller file path.</param>
    /// <param name="memberName">Caller method name.</param>
    /// <param name="lineNumber">Caller line number.</param>
    /// <returns>An instance of <see cref="TelemetrySpan"/>.</returns>
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

    /// <summary>
    /// Start a new span from given parent.
    /// </summary>
    /// <param name="parentSpanContext">The parent call context.</param>
    /// <param name="linkedSpanContext">The linked call context.</param>
    /// <param name="filePath">Caller file path.</param>
    /// <param name="memberName">Caller method name.</param>
    /// <param name="lineNumber">Caller line number.</param>
    /// <returns>An instance of <see cref="TelemetrySpan"/>.</returns>
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

    /// <summary>
    /// Extracts trace context from incoming request headers and starts a new span.
    /// </summary>
    /// <param name="headers">The http headers.</param>
    /// <param name="httpMethod">The http method.</param>
    /// <param name="requestUri">The http request uri.</param>
    /// <param name="filePath">Caller file path.</param>
    /// <param name="memberName">Caller method name.</param>
    /// <param name="lineNumber">Caller line number.</param>
    /// <returns>An instance of <see cref="TelemetrySpan"/>.</returns>
    public TelemetrySpan StartControllerSpan(
        HttpHeaders headers,
        string httpMethod,
        string requestUri,
        [CallerFilePath] string filePath = "",
        [CallerMemberName] string memberName = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        bool TryGetHeaderValue(string key, out string? value)
        {
            if (headers.TryGetValues(key, out var headerValues))
            {
                value = headerValues.FirstOrDefault();
                return !string.IsNullOrEmpty(value);
            }

            value = null;
            return false;
        }

        return this.StartControllerSpanInternal(this.Tracer, TryGetHeaderValue, httpMethod, requestUri, filePath, memberName, lineNumber);
    }

    /// <summary>
    /// Extracts trace context from incoming request headers and starts a new span.
    /// </summary>
    /// <param name="listenerContext">The http listener context.</param>
    /// <param name="filePath">Caller file path.</param>
    /// <param name="memberName">Caller method name.</param>
    /// <param name="lineNumber">Caller line number.</param>
    /// <returns>An instance of <see cref="TelemetrySpan"/>.</returns>
    public TelemetrySpan StartControllerSpan(
        HttpListenerContext listenerContext,
        [CallerFilePath] string filePath = "",
        [CallerMemberName] string memberName = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        bool TryGetHeaderValue(string key, out string? value)
        {
            if (listenerContext.Request?.Headers.HasKeys() == true && listenerContext.Request.Headers[key] != null)
            {
                value = listenerContext.Request.Headers[key];
                return !string.IsNullOrEmpty(value);
            }

            value = null;
            return false;
        }

        // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        var httpMethod = listenerContext.Request?.HttpMethod;
        var requestUri = listenerContext.Request?.RawUrl;
        return this.StartControllerSpanInternal(this.Tracer, TryGetHeaderValue, httpMethod ?? "unknown", requestUri ?? "unknown", filePath, memberName, lineNumber);
    }

    /// <summary>
    /// Start new span for HttpClient request and inject trace context into request headers.
    /// </summary>
    /// <param name="requestHeaders">Http request headers.</param>
    /// <param name="httpMethod">Http method.</param>
    /// <param name="requestUri">Http request uri.</param>
    /// <param name="filePath">Caller file path.</param>
    /// <param name="memberName">Caller method name.</param>
    /// <param name="lineNumber">Caller line number.</param>
    /// <returns>An instance of <see cref="TelemetrySpan"/>.</returns>
    public TelemetrySpan StartHttpClientSpan(
        HttpRequestHeaders requestHeaders,
        string httpMethod,
        string requestUri,
        [CallerFilePath] string filePath = "",
        [CallerMemberName] string memberName = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        var span = this.Tracer.StartActiveSpan($"{fileName}.{memberName}", SpanKind.Client);
        span.SetAttribute("file", fileName);
        span.SetAttribute("line", lineNumber);
        span.SetAttribute("method", memberName);
        span.SetAttribute("http.method", httpMethod);
        span.SetAttribute("http.url", requestUri);
        this.InjectSpanContext(span, requestHeaders);
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
            new KeyValuePair<string, object?>("http.method", method),
            new KeyValuePair<string, object?>("http.request.uri", requestUri),
            new KeyValuePair<string, object?>("http.response.status_code", statusCode));

        this.httpClientRequestLatencyCounter.Add(
            durationMs,
            new KeyValuePair<string, object?>("http.method", method),
            new KeyValuePair<string, object?>("http.request.uri", requestUri),
            new KeyValuePair<string, object?>("http.response.status_code", statusCode));
    }

    public void HttpServerRequestFinished(string method, string? requestUri, int statusCode, long durationMs)
    {
        // trim query parameters
        if (requestUri?.Contains('?') == true)
        {
            requestUri = requestUri.Substring(0, requestUri.IndexOf('?'));
        }

        this.httpServerRequestCounter.Add(
            1,
            new KeyValuePair<string, object?>("http.method", method),
            new KeyValuePair<string, object?>("http.request.uri", requestUri),
            new KeyValuePair<string, object?>("http.response.status_code", statusCode));

        this.httpServerRequestLatencyCounter.Add(
            durationMs,
            new KeyValuePair<string, object?>("http.method", method),
            new KeyValuePair<string, object?>("http.request.uri", requestUri),
            new KeyValuePair<string, object?>("http.response.status_code", statusCode));
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
                new KeyValuePair<string, object?>("name", name),
                new KeyValuePair<string, object?>("filePath", filePath),
                new KeyValuePair<string, object?>("methodName", memberName),
                new KeyValuePair<string, object?>("lineNumber", lineNumber));
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
                new KeyValuePair<string, object?>("name", name),
                new KeyValuePair<string, object?>("durationMS", durationInMs),
                new KeyValuePair<string, object?>("filePath", filePath),
                new KeyValuePair<string, object?>("methodName", memberName),
                new KeyValuePair<string, object?>("lineNumber", lineNumber));
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
                new KeyValuePair<string, object?>("name", name),
                new KeyValuePair<string, object?>("durationMS", durationInMs),
                new KeyValuePair<string, object?>("error",
                    string.IsNullOrWhiteSpace(error)
                        ? "none"
                        : error),
                new KeyValuePair<string, object?>("filePath", filePath),
                new KeyValuePair<string, object?>("methodName", memberName),
                new KeyValuePair<string, object?>("lineNumber", lineNumber));
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
                new KeyValuePair<string, object?>("name", name),
                new KeyValuePair<string, object?>("filePath", filePath),
                new KeyValuePair<string, object?>("methodName", memberName),
                new KeyValuePair<string, object?>("lineNumber", lineNumber));
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
                new KeyValuePair<string, object?>("name", name),
                new KeyValuePair<string, object?>("durationMS", durationInMs),
                new KeyValuePair<string, object?>("filePath", filePath),
                new KeyValuePair<string, object?>("methodName", memberName),
                new KeyValuePair<string, object?>("lineNumber", lineNumber));
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
            new KeyValuePair<string, object?>("key", cacheKey),
            new KeyValuePair<string, object?>("expirationSpan", expirationSpan.ToString()),
            new KeyValuePair<string, object?>("filePath", filePath),
            new KeyValuePair<string, object?>("methodName", memberName),
            new KeyValuePair<string, object?>("lineNumber", lineNumber));
    }

    public void OnCacheHit(
        string cacheKey,
        [CallerFilePath] string filePath = "",
        [CallerMemberName] string memberName = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        this.cacheHitCounter.Add(
            1,
            new KeyValuePair<string, object?>("key", cacheKey),
            new KeyValuePair<string, object?>("filePath", filePath),
            new KeyValuePair<string, object?>("methodName", memberName),
            new KeyValuePair<string, object?>("lineNumber", lineNumber));
    }

    public void OnCacheMiss(
        string cacheKey,
        [CallerFilePath] string filePath = "",
        [CallerMemberName] string memberName = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        this.cacheMissCounter.Add(
            1,
            new KeyValuePair<string, object?>("key", cacheKey),
            new KeyValuePair<string, object?>("filePath", filePath),
            new KeyValuePair<string, object?>("methodName", memberName),
            new KeyValuePair<string, object?>("lineNumber", lineNumber));
    }

    public void OnCacheExpired(
        string cacheKey,
        [CallerFilePath] string filePath = "",
        [CallerMemberName] string memberName = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        this.cacheExpiredCounter.Add(
            1,
            new KeyValuePair<string, object?>("key", cacheKey),
            new KeyValuePair<string, object?>("filePath", filePath),
            new KeyValuePair<string, object?>("methodName", memberName),
            new KeyValuePair<string, object?>("lineNumber", lineNumber));
    }

    #endregion


    /// <summary>
    /// Dispose resources embedded in this instance.
    /// </summary>
    public void Dispose()
    {
        this.LoggerFactory.Dispose();
        this.MeterProvider.Dispose();
        this.TracerProvider.Dispose();
        this.Meter.Dispose();
    }

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