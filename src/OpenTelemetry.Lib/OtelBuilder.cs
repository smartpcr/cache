//-------------------------------------------------------------------------------
// <copyright file="OtelBuilder.cs" company="Microsoft Corp">
// Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
//-------------------------------------------------------------------------------

namespace OpenTelemetry.Lib;

using System.Diagnostics.Metrics;
using System.Globalization;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

/// <summary>
/// Extension methods to build OpenTelemetry components.
/// </summary>
public static class OtelBuilder
{
    /// <summary>
    /// Setup ILoggerFactory with OpenTelemetry logging.
    /// </summary>
    /// <param name="configDict">The config settings.</param>
    /// <param name="serviceName">The service name.</param>
    /// <param name="serviceVersion">The service version.</param>
    /// <returns>An instance of <see cref="ILoggerFactory"/>.</returns>
    public static ILoggerFactory SetupLogger(
        Dictionary<string, string> configDict,
        string serviceName = ApplicationMetadata.ServiceName,
        string serviceVersion = ApplicationMetadata.ServiceVersion)
    {
        var enabled = configDict.TryGetValue(OtelSettings.OtelEnabledParameter, out var enableOtelInput) && bool.Parse(enableOtelInput);
        if (!configDict.TryGetValue(OtelSettings.SinkTypesParameter, out var sinkTypeInput))
        {
            sinkTypeInput = OtelSettings.DefaultSinkTypes.ToString();
        }

        if (!Enum.TryParse(sinkTypeInput, out OtelSinkTypes sinkType))
        {
            sinkType = OtelSettings.DefaultSinkTypes;
        }

        if (!configDict.TryGetValue(OtelSettings.LogLevelParameter, out var logLevelInput))
        {
            logLevelInput = OtelSettings.DefaultLogLevel.ToString();
        }

        if (!Enum.TryParse(logLevelInput, out LogLevel logLevel))
        {
            logLevel = OtelSettings.DefaultLogLevel;
        }

        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.ClearProviders();
            builder.SetMinimumLevel(logLevel);
            builder.AddOpenTelemetry(options =>
            {
                options.IncludeScopes = true;
                options.IncludeFormattedMessage = true;
                options.ParseStateValues = true;
                options.SetResourceBuilder(ResourceBuilder.CreateDefault()
                    .AddService(serviceName, serviceVersion: serviceVersion, serviceInstanceId: Environment.MachineName)
                    .AddAttributes(new Dictionary<string, object> { { "service_name", serviceName }, { "service_version", serviceVersion } }));

                if (enabled)
                {
                    if (!configDict.TryGetValue(OtelSettings.UseBatchParameter, out var useBatchInput))
                    {
                        useBatchInput = OtelSettings.DefaultUseBatch.ToString();
                    }

                    if (!bool.TryParse(useBatchInput, out var useBatch))
                    {
                        useBatch = OtelSettings.DefaultUseBatch;
                    }

                    if (!configDict.TryGetValue(OtelSettings.ExportIntervalParameter, out var exportInterval))
                    {
                        exportInterval = OtelSettings.DefaultExportIntervalMilliseconds.ToString();
                    }

                    if (!int.TryParse(exportInterval, out var exportIntervalMilliseconds))
                    {
                        exportIntervalMilliseconds = OtelSettings.DefaultExportIntervalMilliseconds;
                    }

                    if ((sinkType & OtelSinkTypes.Console) != 0)
                    {
                        options.AddConsoleExporter();
                    }

                    if ((sinkType & OtelSinkTypes.File) != 0)
                    {
                        var fileExporter = new LogFileExporter(new FileSinkSettings("log"), LogLevel.Information);
                        if (useBatch)
                        {
                            options.AddProcessor(new BatchLogRecordExportProcessor(
                                fileExporter,
                                exporterTimeoutMilliseconds: exportIntervalMilliseconds));
                        }
                        else
                        {
                            options.AddProcessor(new SimpleLogRecordExportProcessor(fileExporter));
                        }
                    }

                    if ((sinkType & OtelSinkTypes.OTLP) != 0)
                    {
                        var otlpEndpoint = configDict.GetValueOrDefault(OtelSettings.OtelEndpointParameter, OtelSettings.DefaultOtelEndpoint);

                        options.AddOtlpExporter(otlpOptions =>
                        {
                            otlpOptions.Endpoint = new Uri(otlpEndpoint);
                            otlpOptions.Protocol = OtlpExportProtocol.Grpc; // 4317, or use 4318 for HTTP/Protobuf
                        });
                    }
                }
            });
        });

        return loggerFactory;
    }

    /// <summary>
    /// Setup TracerProvider with OpenTelemetry tracing.
    /// </summary>
    /// <param name="configDict">The config settings.</param>
    /// <param name="serviceName">The service name.</param>
    /// <param name="serviceVersion">The service version.</param>
    /// <returns>An instance of <see cref="TracerProvider"/>.</returns>
    public static TracerProvider SetupTracing(
        Dictionary<string, string> configDict,
        string serviceName = ApplicationMetadata.ServiceName,
        string serviceVersion = ApplicationMetadata.ServiceVersion)
    {
        var enabled = configDict.TryGetValue(OtelSettings.OtelEnabledParameter, out var enableOtelInput) && bool.Parse(enableOtelInput);
        if (!configDict.TryGetValue(OtelSettings.SinkTypesParameter, out var sinkTypeInput))
        {
            sinkTypeInput = OtelSettings.DefaultSinkTypes.ToString();
        }

        if (!Enum.TryParse(sinkTypeInput, out OtelSinkTypes sinkType))
        {
            sinkType = OtelSettings.DefaultSinkTypes;
        }

        if (!configDict.TryGetValue(OtelSettings.SamplerTypesParameter, out var samplerTypesInput))
        {
            samplerTypesInput = OtelSettings.DefaultSamplerTypes.ToString();
        }

        if (!Enum.TryParse(samplerTypesInput, out TraceSamplerTypes samplerType))
        {
            samplerType = OtelSettings.DefaultSamplerTypes;
        }

        if (!configDict.TryGetValue(OtelSettings.SamplerRatioParameter, out var samplerRatioInput))
        {
            samplerRatioInput = OtelSettings.DefaultSamplerRatio.ToString(CultureInfo.InvariantCulture);
        }

        if (!double.TryParse(samplerRatioInput, out var samplerRatio))
        {
            samplerRatio = OtelSettings.DefaultSamplerRatio;
        }

        if (!configDict.TryGetValue(OtelSettings.UseBatchParameter, out var useBatchInput))
        {
            useBatchInput = OtelSettings.DefaultUseBatch.ToString();
        }

        if (!bool.TryParse(useBatchInput, out var useBatch))
        {
            useBatch = OtelSettings.DefaultUseBatch;
        }

        if (!configDict.TryGetValue(OtelSettings.ExportIntervalParameter, out var exportIntervalInput))
        {
            exportIntervalInput = OtelSettings.DefaultExportIntervalMilliseconds.ToString();
        }

        if (!int.TryParse(exportIntervalInput, out var exportIntervalMilliseconds))
        {
            exportIntervalMilliseconds = OtelSettings.DefaultExportIntervalMilliseconds;
        }

        var traceProviderBuilder = Sdk.CreateTracerProviderBuilder()
            .AddSource(serviceName)
            .AddSource($"{serviceName}.*") // this is necessary since we created different spans with this prefix
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(
                serviceName: serviceName,
                serviceVersion: serviceVersion,
                serviceInstanceId: Environment.MachineName))
            .SetSampler(_ => CreateSampler(samplerType, samplerRatio));

        if (enabled)
        {
            if ((sinkType & OtelSinkTypes.Console) != 0)
            {
                traceProviderBuilder.AddConsoleExporter();
            }

            if ((sinkType & OtelSinkTypes.File) != 0)
            {
                traceProviderBuilder.AddProcessor(new TraceFileProcessor(new FileSinkSettings("trace")));
            }

            if ((sinkType & OtelSinkTypes.OTLP) != 0)
            {
                var otlpEndpoint = configDict.GetValueOrDefault(OtelSettings.OtelEndpointParameter, OtelSettings.DefaultOtelEndpoint);

                traceProviderBuilder.AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(otlpEndpoint);
                    options.Protocol = OtlpExportProtocol.Grpc; // 4317, or use 4318 for HTTP/Protobuf
                    options.ExportProcessorType = useBatch
                        ? ExportProcessorType.Batch
                        : ExportProcessorType.Simple;
                    options.TimeoutMilliseconds = exportIntervalMilliseconds;
                });
            }
        }

        var tracerProvider = traceProviderBuilder.Build();

        return tracerProvider;
    }

    /// <summary>
    /// Setup MeterProvider with OpenTelemetry tracing.
    /// </summary>
    /// <param name="configDict">The config settings.</param>
    /// <param name="serviceName">The service name.</param>
    /// <param name="serviceVersion">The service version.</param>
    /// <returns>An instance of <see cref="MeterProvider"/>.</returns>
    public static MeterProvider SetupMetrics(
        Dictionary<string, string> configDict,
        string serviceName = ApplicationMetadata.ServiceName,
        string serviceVersion = ApplicationMetadata.ServiceVersion)
    {
        var enabled = configDict.TryGetValue(OtelSettings.OtelEnabledParameter, out var enableOtelInput) && bool.Parse(enableOtelInput);
        if (!configDict.TryGetValue(OtelSettings.SinkTypesParameter, out var sinkTypeInput))
        {
            sinkTypeInput = OtelSettings.DefaultSinkTypes.ToString();
        }

        if (!Enum.TryParse(sinkTypeInput, out OtelSinkTypes sinkType))
        {
            sinkType = OtelSettings.DefaultSinkTypes;
        }

        var meterProviderBuilder = Sdk.CreateMeterProviderBuilder()
            .ConfigureResource(r => r.AddService(
                serviceName,
                serviceVersion: serviceVersion,
                serviceInstanceId: Environment.MachineName))
            .AddMeter(serviceName);

        if (enabled)
        {
            if (!configDict.TryGetValue(OtelSettings.EnableHttpInstrumentationParameter, out var instrumentHttp))
            {
                instrumentHttp = OtelSettings.DefaultEnableHttpInstrumentation.ToString();
            }

            if (bool.TryParse(instrumentHttp, out var enableHttpInstrumentation) && enableHttpInstrumentation)
            {
                meterProviderBuilder.AddHttpClientInstrumentation();
            }

            if (!configDict.TryGetValue(OtelSettings.EnableRuntimeInstrumentationParameter, out var instrumentRuntime))
            {
                instrumentRuntime = OtelSettings.DefaultEnableRuntimeInstrumentation.ToString();
            }

            if (bool.TryParse(instrumentRuntime, out var enableRunTimeInstrumentation) && enableRunTimeInstrumentation)
            {
                // for .net framework, only "process.runtime.dotnet.gc.collections.count" and
                // "process.runtime.dotnet.gc.objects.size" are supported
                meterProviderBuilder.AddRuntimeInstrumentation();
                var meter = new Meter(serviceName);
                var performanceMetrics = new PerformanceMetrics();
                List<KeyValuePair<string, object?>> tags = new List<KeyValuePair<string, object?>>()
                {
                    new KeyValuePair<string, object?>("service", serviceName),
                    new KeyValuePair<string, object?>("instance", Environment.MachineName),
                    new KeyValuePair<string, object?>("unit", "ms"),
                    new KeyValuePair<string, object?>("type", "gauge"),
                    new KeyValuePair<string, object?>("process", performanceMetrics.GetProcessName()),
                    new KeyValuePair<string, object?>("process_id", performanceMetrics.GetProcessId()),
                };

                meter.CreateObservableGauge(
                    "hci.process.cpu.usage.percent",
                    () => performanceMetrics.GetCpuUsage(),
                    "percent",
                    "CPU usage in milliseconds",
                    tags);

                meter.CreateObservableGauge(
                    "hci.process.memory.usage.mb",
                    () => performanceMetrics.GetUsedMemory(),
                    "MB",
                    "Memory usage in MB",
                    tags);

                meter.CreateObservableGauge(
                    "hci.process.thread.count",
                    () => performanceMetrics.GetThreadCount(),
                    "count",
                    "thread count",
                    tags);

                meter.CreateObservableGauge(
                    "hci.process.handle.count",
                    () => performanceMetrics.GetHandleCount(),
                    "count",
                    "handle count",
                    tags);

                meter.CreateObservableGauge(
                    "hci.process.exception.count",
                    () => performanceMetrics.GetExceptionCount(),
                    "count",
                    "exceptions count",
                    tags);

                meter.CreateObservableCounter(
                    "hci.process.contention.count",
                    () => performanceMetrics.GetContentionCount(),
                    "count",
                    "contention count",
                    tags);


                    meter.CreateObservableGauge(
                        "hci.process.page.faults",
                        () => performanceMetrics.GetPageFaultsRate(),
                        "count",
                        "page faults",
                        tags);

                    meter.CreateObservableGauge(
                        "hci.process.io.bytes",
                        () => performanceMetrics.GetIOBytesRate(),
                        "bytes",
                        "IO data",
                        tags);

                    meter.CreateObservableGauge(
                        "hci.process.memory.stack",
                        () => performanceMetrics.GetStackMemorySize(),
                        "bytes",
                        "stack memory size",
                        tags);

                    meter.CreateObservableGauge(
                        "hci.process.memory.heap",
                        () => performanceMetrics.GetHeapMemorySize(),
                        "bytes",
                        "heap memory size",
                        tags);

                    meter.CreateObservableGauge(
                        "hci.tasks.active",
                        () =>
                        {
                            var diag = DiagnosticsConfig.Instance;
                            return diag.GetActiveTasks();
                        },
                        "count",
                        "total active tasks",
                        tags);

                    meter.CreateObservableGauge(
                        "hci.tasks.hanging",
                        () =>
                        {
                            var diag = DiagnosticsConfig.Instance;
                            return diag.GetLongRunningTasks(TimeSpan.FromHours(1));
                        },
                        "count",
                        "long running tasks over 1hr",
                        tags);

                    meter.CreateObservableGauge(
                        "hci.lock.active",
                        () =>
                        {
                            var diag = DiagnosticsConfig.Instance;
                            return diag.GetActiveLocks();
                        },
                        "count",
                        "total active locks",
                        tags);

                    meter.CreateObservableGauge(
                        "hci.lock.dead",
                        () =>
                        {
                            var diag = DiagnosticsConfig.Instance;
                            return diag.GetDeadLocks(TimeSpan.FromHours(1));
                        },
                        "count",
                        "locks held over 1hr",
                        tags);
            }

            if (!configDict.TryGetValue(OtelSettings.ExportIntervalParameter, out var exportInterval))
            {
                exportInterval = OtelSettings.DefaultExportIntervalMilliseconds.ToString();
            }

            if (!int.TryParse(exportInterval, out var exportIntervalMilliseconds))
            {
                exportIntervalMilliseconds = OtelSettings.DefaultExportIntervalMilliseconds;
            }

            if ((sinkType & OtelSinkTypes.Console) != 0)
            {
                meterProviderBuilder.AddConsoleExporter((_, readerOps) =>
                {
                    readerOps.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds = exportIntervalMilliseconds;
                });
            }

            if ((sinkType & OtelSinkTypes.File) != 0)
            {
                meterProviderBuilder.AddReader(new PeriodicExportingMetricReader(
                    new MetricFileExporter(new FileSinkSettings("metric")),
                    exportIntervalMilliseconds: exportIntervalMilliseconds));
            }

            if ((sinkType & OtelSinkTypes.OTLP) != 0)
            {
                var otlpEndpoint = configDict.GetValueOrDefault(OtelSettings.OtelEndpointParameter, OtelSettings.DefaultOtelEndpoint);

                meterProviderBuilder.AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(otlpEndpoint);
                    options.Protocol = OtlpExportProtocol.Grpc; // 4317, or use 4318 for HTTP/Protobuf
                    options.ExportProcessorType = ExportProcessorType.Simple;
                    options.TimeoutMilliseconds = 1000;
                });
            }
        }

        var meterProvider = meterProviderBuilder.Build();
        return meterProvider;
    }

    private static Sampler CreateSampler(TraceSamplerTypes samplerType, double samplerRatio)
    {
        switch (samplerType)
        {
            case TraceSamplerTypes.AlwaysOff:
                return new AlwaysOffSampler();
            case TraceSamplerTypes.AlwaysOn:
                return new AlwaysOnSampler();
            case TraceSamplerTypes.RatioBased:
                return new TraceIdRatioBasedSampler(samplerRatio);
            default:
                throw new ArgumentOutOfRangeException(nameof(samplerType), samplerType, null);
        }
    }
}
