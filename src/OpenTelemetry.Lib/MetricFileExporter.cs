// <copyright file="MetricFileExporter.cs" company="Microsoft Corp">
// Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>

namespace OpenTelemetry.Lib;

using System.Text;
using OpenTelemetry.Metrics;

/// <summary>
/// Persist metrics to a file.
/// </summary>
public class MetricFileExporter : BaseExporter<Metric>
{
    private readonly RollingFileLogger fileLogger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MetricFileExporter"/> class.
    /// </summary>
    /// <param name="fileSink">The file sink settings.</param>
    public MetricFileExporter(FileSinkSettings fileSink)
    {
        this.fileLogger = new RollingFileLogger(fileSink, "metric");
    }

    /// <summary>
    /// Export batches of metrics to a file.
    /// </summary>
    /// <param name="batch">Batch of metrics.</param>
    /// <returns>The export result.</returns>
    public override ExportResult Export(in Batch<Metric> batch)
    {
        if (batch.Count == 0)
        {
            return ExportResult.Success;
        }

        using (SuppressInstrumentationScope.Begin())
        {
            var sb = new StringBuilder();
            foreach (var metric in batch)
            {
                sb.AppendLine($"{metric.Name}[{metric.MetricType}]:");

                foreach (ref readonly var metricPoint in metric.GetMetricPoints())
                {
                    sb.AppendLine($"\tStart Time: {metricPoint.StartTime.ToString("yyyy-MM-dd hh:mm:ss.fff")}");

                    foreach (var tag in metricPoint.Tags)
                    {
                        sb.AppendLine($"\t\tTag: {tag.Key} = {tag.Value}");
                    }

                    // Add metric values based on metric type
                    switch (metric.MetricType)
                    {
                        case MetricType.LongSum:
                            sb.AppendLine($"\tValue: {metricPoint.GetSumLong()}");
                            break;
                        case MetricType.DoubleSum:
                            sb.AppendLine($"\tValue: {metricPoint.GetSumDouble()}");
                            break;
                        case MetricType.LongGauge:
                            sb.AppendLine($"\tValue: {metricPoint.GetGaugeLastValueLong()}");
                            break;
                        case MetricType.DoubleGauge:
                            sb.AppendLine($"\tValue: {metricPoint.GetGaugeLastValueDouble()}");
                            break;
                        case MetricType.Histogram:
                            sb.AppendLine($"\tHistogram sum: {metricPoint.GetHistogramSum()}");
                            sb.AppendLine($"\tHistogram count: {metricPoint.GetHistogramCount()}");
                            break;
                        case MetricType.LongSumNonMonotonic:
                            sb.AppendLine($"\tValue: {metricPoint.GetSumLong()} (Non-Monotonic)");
                            break;
                        default:
                            sb.AppendLine($"\tUnsupported metric type: {metric.MetricType}");
                            break;
                    }
                }
            }

            Console.WriteLine($"Metrics: \n{sb}");
            this.fileLogger.Log(sb.ToString());
            return ExportResult.Success;
        }
    }
}
