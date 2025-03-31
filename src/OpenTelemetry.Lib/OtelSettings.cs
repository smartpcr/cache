// <copyright file="OtelSettings.cs" company="Microsoft Corp">
// Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>

namespace OpenTelemetry.Lib;

using Microsoft.Extensions.Logging;

/// <summary>
/// Open Telemetry settings.
/// </summary>
public static class OtelSettings
{
    /// <summary>
    /// The empty trace id.
    /// </summary>
    public const string EmptyTraceId = "00000000000000000000000000000000";

    /// <summary>
    /// The empty span id.
    /// </summary>
    public const string EmptySpanId = "0000000000000000";

    /// <summary>
    /// Parameter name in setting file indicating if OpenTelemetry is enabled.
    /// By default, it is disabled if not specified.
    /// </summary>
    public const string OtelEnabledParameter = "OtelEnabled";

    /// <summary>
    /// Parameter name in setting file indicating the sink types.
    /// </summary>
    public const string SinkTypesParameter = "SinkTypes";

    /// <summary>
    /// Parameter name in setting file indicating the sampler types.
    /// </summary>
    public const string SamplerTypesParameter = "SamplerTypes";

    /// <summary>
    /// Parameter name in setting file indicating the sampler ratio.
    /// </summary>
    public const string SamplerRatioParameter = "SamplerRatio";

    /// <summary>
    /// Parameter name in setting file indicating the OTLP endpoint.
    /// This is required when OpenTelemetry is enabled.
    /// </summary>
    public const string OtelEndpointParameter = "OtelEndpoint";

    /// <summary>
    /// Parameter name in setting file indicating if batch should be used.
    /// </summary>
    public const string UseBatchParameter = "UseBatch";

    /// <summary>
    /// Parameter name in setting file indicating if HTTP instrumentation should be enabled.
    /// </summary>
    public const string EnableHttpInstrumentationParameter = "EnableHttpInstrumentation";

    /// <summary>
    /// Parameter name in setting file indicating if runtime instrumentation should be enabled.
    /// Note that runtime instrumentation is only supported in .NET Core 3.1 and later.
    /// </summary>
    public const string EnableRuntimeInstrumentationParameter = "EnableRuntimeInstrumentation";

    /// <summary>
    /// Parameter name in setting file indicating the export interval in milliseconds.
    /// </summary>
    public const string ExportIntervalParameter = "ExportInterval";

    /// <summary>
    /// Parameter name in setting file indicating the Jaeger agent host.
    /// </summary>
    public const string JaegerAgentHostParameter = "JaegerAgentHost";

    /// <summary>
    /// Parameter name in setting file indicating the Jaeger agent port.
    /// </summary>
    public const string JaegerAgentPortParameter = "JaegerAgentPort";

    /// <summary>
    /// The default sink type.
    /// </summary>
    public const OtelSinkTypes DefaultSinkTypes = OtelSinkTypes.OTLP;

    /// <summary>
    /// The default sampler type.
    /// </summary>
    public const TraceSamplerTypes DefaultSamplerTypes = TraceSamplerTypes.RatioBased;

    /// <summary>
    /// The default sampler ratio.
    /// </summary>
    public const double DefaultSamplerRatio = 0.1;

    /// <summary>
    /// The default flag indicating whether to use batch.
    /// </summary>
    public const bool DefaultUseBatch = true;

    /// <summary>
    /// The default export interval in milliseconds.
    /// </summary>
    public const int DefaultExportIntervalMilliseconds = 60000;

    /// <summary>
    /// The default otel endpoint.
    /// </summary>
    public const string DefaultOtelEndpoint = "http://dvmendpoint:4320"; // otel collector grpc/OTLP endpoint

    /// <summary>
    /// The default flag indicating whether to enable HTTP instrumentation.
    /// </summary>
    public const bool DefaultEnableHttpInstrumentation = true;

    /// <summary>
    /// The default flag indicating whether to enable runtime instrumentation.
    /// </summary>
    public const bool DefaultEnableRuntimeInstrumentation = true;

    /// <summary>
    /// The parameter name for log level.
    /// </summary>
    public const string LogLevelParameter = "LogLevel";

    /// <summary>
    /// The default minimal lob level.
    /// </summary>
    public const LogLevel DefaultLogLevel = LogLevel.Information;

    /// <summary>
    /// Get OpenTelemetry configuration settings.
    /// </summary>
    /// <param name="enableOtel">A flag indicating whether open telemetry is enabled.</param>
    /// <param name="otelEndpoint">Otel endpoint.</param>
    /// <param name="logLevel">Minimal log level.</param>
    /// <returns>OpenTelemetry settings.</returns>
    public static Dictionary<string, string> GetOtelConfigSettings(bool enableOtel, string otelEndpoint, LogLevel logLevel = DefaultLogLevel)
    {
        var dict = new Dictionary<string, string>();
        dict.Add(OtelSettings.OtelEnabledParameter, enableOtel.ToString());
        dict.Add(OtelSettings.SinkTypesParameter, "OTLP");
        dict.Add(OtelSettings.SamplerTypesParameter, "AlwaysOn");
        dict.Add(OtelSettings.ExportIntervalParameter, "1000");
        dict.Add(OtelSettings.OtelEndpointParameter, otelEndpoint);
        dict.Add(OtelSettings.EnableHttpInstrumentationParameter, "true");
        dict.Add(OtelSettings.EnableRuntimeInstrumentationParameter, "true");
        dict.Add(OtelSettings.UseBatchParameter, "false");
        dict.Add(OtelSettings.LogLevelParameter, logLevel.ToString());

        return dict;
    }

    /// <summary>
    /// Returns true if the trace id is empty.
    /// </summary>
    /// <param name="traceId">Trace id.</param>
    /// <returns>A flag indicating if trace id is empty.</returns>
    public static bool IsEmptyTraceId(this string traceId) =>
        string.IsNullOrEmpty(traceId) || traceId.Equals(EmptyTraceId);

    /// <summary>
    /// Returns true if the span id is empty.
    /// </summary>
    /// <param name="spanId">Span id.</param>
    /// <returns>A flag indicating if span id is empty.</returns>
    public static bool IsEmptySpanId(this string spanId) =>
        string.IsNullOrEmpty(spanId) || spanId.Equals(EmptySpanId);
}

/// <summary>
/// OpenTelemetry sink types.
/// </summary>
[System.Flags]
public enum OtelSinkTypes
{
    /// <summary>
    /// Not recorded.
    /// </summary>
    None = 0,

    /// <summary>
    /// Write to console.
    /// </summary>
    Console = 1 << 1,

    /// <summary>
    /// Write to file.
    /// </summary>
    File = 1 << 2,

    /// <summary>
    /// Send to OTLP endpoint.
    /// </summary>
    OTLP = 1 << 3,

    /// <summary>
    /// Send to Jaeger.
    /// </summary>
    Jaeger = 1 << 4,

    /// <summary>
    /// The default exporter type.
    /// </summary>
    Default = OTLP | Console
}

/// <summary>
/// Trace sampler type.
/// </summary>
public enum TraceSamplerTypes
{
    /// <summary>
    /// Not sampled.
    /// </summary>
    AlwaysOff,

    /// <summary>
    /// All sampled.
    /// </summary>
    AlwaysOn,

    /// <summary>
    /// Sampled based on ratio.
    /// </summary>
    RatioBased,
}
