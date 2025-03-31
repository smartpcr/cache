// <copyright file="SpanContextExtension.cs" company="Microsoft Corp">
// Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>

namespace OpenTelemetry.Lib;

using System.Diagnostics;
using OpenTelemetry.Trace;

/// <summary>
/// Extension methods for <see cref="SpanContext"/>.
/// </summary>
public static class SpanContextExtension
{
    private const string DefaultTraceId = "0a000a0a0000a0a0000000a0a0000a00";
    private const string DefaultSpanId = "b9999999b9999bb9";

    /// <summary>
    /// Get default span context.
    /// </summary>
    /// <returns>An instance of <see cref="SpanContext"/>.</returns>
    public static SpanContext GetDefaultContext() =>
        new(
            traceId: ActivityTraceId.CreateFromString(DefaultTraceId.AsSpan()),
            spanId: ActivitySpanId.CreateFromString(DefaultSpanId.AsSpan()),
            traceFlags: ActivityTraceFlags.Recorded);

    /// <summary>
    /// Parse span context from string.
    /// </summary>
    /// <param name="input">The string input.</param>
    /// <param name="spanContext">The span context.</param>
    /// <returns>A flag indicating whether string input is valid.</returns>
    public static bool TryParseSpanContext(string input, out SpanContext spanContext)
    {
        var traceParent = input.Split('-');
        if (traceParent.Length == 4)
        {
            var traceId = traceParent[1];
            var spanId = traceParent[2];
            var traceFlags = traceParent[3];

            if (!traceId.IsEmptyTraceId() && !spanId.IsEmptySpanId())
            {
                spanContext = new SpanContext(
                    traceId: ActivityTraceId.CreateFromString(traceId.AsSpan()),
                    spanId: ActivitySpanId.CreateFromString(spanId.AsSpan()),
                    traceFlags: traceFlags == "01" ? ActivityTraceFlags.Recorded : ActivityTraceFlags.None,
                    isRemote: true);
                return true;
            }
        }

        spanContext = default;
        return false;
    }

    /// <summary>
    /// Generate string representation of span context.
    /// </summary>
    /// <param name="spanContext">The span context.</param>
    /// <returns>String representation of span context.</returns>
    public static string ToString(SpanContext spanContext)
    {
        var traceFlags = spanContext.TraceFlags == ActivityTraceFlags.Recorded ? "01" : "00";
        return $"00-{spanContext.TraceId}-{spanContext.SpanId}-{traceFlags}";
    }
}
