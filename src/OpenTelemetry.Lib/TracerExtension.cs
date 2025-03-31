// <copyright file="TracerExtension.cs" company="Microsoft Corp">
// Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>

namespace OpenTelemetry.Lib;

using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using OpenTelemetry.Trace;

/// <summary>
/// Extension methods for <see cref="Tracer"/>.
/// </summary>
public static class TracerExtension
{
    /// <summary>
    /// Extract trace state from incoming request headers and start a new span as its child.
    /// </summary>
    /// <param name="tracer">The <see cref="Tracer"/>.</param>
    /// <param name="headers">The http headers.</param>
    /// <param name="filePath">Caller file path.</param>
    /// <param name="memberName">Caller method name.</param>
    /// <param name="lineNumber">Caller line number.</param>
    /// <returns>An instance of <see cref="TelemetrySpan"/>.</returns>
    public static TelemetrySpan StartControllerSpan(
        this Tracer tracer,
        HttpHeaders? headers,
        [CallerFilePath] string filePath = "",
        [CallerMemberName] string memberName = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        bool TryGetHeaderValue(string key, out string? value)
        {
            if (headers != null && headers.TryGetValues(key, out var headerValues))
            {
                value = headerValues.FirstOrDefault();
                return !string.IsNullOrEmpty(value);
            }

            value = null;
            return false;
        }

        return StartControllerSpanInternal(tracer, TryGetHeaderValue, filePath, memberName, lineNumber);
    }

    /// <summary>
    /// Extract trace state from incoming request headers and start a new span as its child.
    /// </summary>
    /// <param name="tracer">The <see cref="Tracer"/>.</param>
    /// <param name="listenerContext">The http listener context.</param>
    /// <param name="filePath">Caller file path.</param>
    /// <param name="memberName">Caller method name.</param>
    /// <param name="lineNumber">Caller line number.</param>
    /// <returns>An instance of <see cref="TelemetrySpan"/>.</returns>
    public static TelemetrySpan StartControllerSpan(
        this Tracer tracer,
        HttpListenerContext listenerContext,
        [CallerFilePath] string filePath = "",
        [CallerMemberName] string memberName = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        bool TryGetHeaderValue(string key, out string? value)
        {
            if (listenerContext.Request.Headers.HasKeys() && listenerContext.Request.Headers[key] != null)
            {
                value = listenerContext.Request.Headers[key];
                return !string.IsNullOrEmpty(value);
            }

            value = null;
            return false;
        }

        return StartControllerSpanInternal(tracer, TryGetHeaderValue, filePath, memberName, lineNumber);
    }

    /// <summary>
    /// Add current trace context to outgoing request headers.
    /// </summary>
    /// <param name="span">The telemetry span.</param>
    /// <param name="headers">Thr request headers.</param>
    public static void InjectSpanContext(this TelemetrySpan span, HttpRequestHeaders headers)
    {
        var traceParent = $"00-{span.Context.TraceId}-{span.Context.SpanId}-01";
        headers.Add(DiagnosticsConfig.TraceParentHeader, traceParent);

        if (span.Context.IsValid)
        {
            var traceStateValue = string.Join(",", span.Context.TraceState.Select(entry => $"{entry.Key}={entry.Value}"));
            headers.Add(DiagnosticsConfig.TraceStateHeader, traceStateValue);
        }
    }

    private static TelemetrySpan StartControllerSpanInternal(
        Tracer tracer,
        DiagnosticsConfig.TryGetHeaderValueDelegate tryGetHeaderValue,
        string filePath,
        string memberName,
        int lineNumber)
    {
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        var span = tracer.StartActiveSpan($"{fileName}.{memberName}", SpanKind.Server);

        if (tryGetHeaderValue(DiagnosticsConfig.TraceParentHeader, out var traceParentHeader))
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
                            traceFlags == "01" ? ActivityTraceFlags.Recorded : ActivityTraceFlags.None);
                        var parentSpanContext = new SpanContext(
                            traceId: parentActivityContext.TraceId,
                            spanId: parentActivityContext.SpanId,
                            traceFlags: parentActivityContext.TraceFlags,
                            isRemote: true);

                        if (tryGetHeaderValue(DiagnosticsConfig.TraceStateHeader, out var tracestateHeaderValue))
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

                        span = tracer.StartActiveSpan($"{fileName}.{memberName}", SpanKind.Server, parentSpanContext);
                    }
                }
            }
        }

        span.PopulateCallerInfo(fileName, memberName, lineNumber);
        return span;
    }

    private static void PopulateCallerInfo(
        this TelemetrySpan span,
        string fileName,
        string memberName,
        int lineNumber)
    {
        span.SetAttribute("file", fileName);
        span.SetAttribute("line", lineNumber);
        span.SetAttribute("method", memberName);
    }
}
