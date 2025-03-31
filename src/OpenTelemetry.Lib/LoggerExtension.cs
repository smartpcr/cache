// <copyright file="LoggerExtension.cs" company="Microsoft Corp">
// Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>

namespace OpenTelemetry.Lib;

using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;

/// <summary>
/// Extension methods for <see cref="ILogger"/>.
/// </summary>
public static class LoggerExtension
{
    /// <summary>
    /// Log message with span context and level info.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="spanContext">Current span context.</param>
    /// <param name="message">The log message.</param>
    /// <param name="tags">The log tags.</param>
    /// <param name="filePath">Caller file path.</param>
    /// <param name="memberName">Caller method name.</param>
    /// <param name="lineNumber">Caller line number.</param>
    public static void Info(
        this ILogger logger,
        SpanContext spanContext,
        string message,
        List<KeyValuePair<string, object>>? tags = null,
        [CallerFilePath] string filePath = "",
        [CallerMemberName] string memberName = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        if (tags == null)
        {
            tags = new List<KeyValuePair<string, object>>();
        }

        tags.Add(new KeyValuePair<string, object>("TraceId", spanContext.TraceId.ToString()));
        tags.Add(new KeyValuePair<string, object>("SpanId", spanContext.SpanId.ToString()));
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        var logMessage = $"[{fileName}.{memberName}:{lineNumber}] {message}";
        using (logger.BeginScope(tags))
        {
            logger.LogInformation(logMessage);
        }
    }

    /// <summary>
    /// Log message with span context and level debug.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="spanContext">Current span context.</param>
    /// <param name="message">The log message.</param>
    /// <param name="tags">The log tags.</param>
    /// <param name="filePath">Caller file path.</param>
    /// <param name="memberName">Caller method name.</param>
    /// <param name="lineNumber">Caller line number.</param>
    public static void Debug(
        this ILogger logger,
        SpanContext spanContext,
        string message,
        List<KeyValuePair<string, object>>? tags = null,
        [CallerFilePath] string filePath = "",
        [CallerMemberName] string memberName = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        if (tags == null)
        {
            tags = new List<KeyValuePair<string, object>>();
        }

        tags.Add(new KeyValuePair<string, object>("TraceId", spanContext.TraceId.ToString()));
        tags.Add(new KeyValuePair<string, object>("SpanId", spanContext.SpanId.ToString()));
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        var logMessage = $"[{fileName}.{memberName}:{lineNumber}] {message}";
        using (logger.BeginScope(tags))
        {
            logger.LogDebug(logMessage);
        }
    }

    /// <summary>
    /// Log message with span context and level warning.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="spanContext">Current span context.</param>
    /// <param name="message">The log message.</param>
    /// <param name="tags">The log tags.</param>
    /// <param name="filePath">Caller file path.</param>
    /// <param name="memberName">Caller method name.</param>
    /// <param name="lineNumber">Caller line number.</param>
    public static void Warning(
        this ILogger logger,
        SpanContext spanContext,
        string message,
        List<KeyValuePair<string, object>>? tags = null,
        [CallerFilePath] string filePath = "",
        [CallerMemberName] string memberName = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        var logMessage = $"[{fileName}.{memberName}:{lineNumber}] {message}";
        if (tags == null)
        {
            tags = new List<KeyValuePair<string, object>>();
        }

        tags.Add(new KeyValuePair<string, object>("TraceId", spanContext.TraceId.ToString()));
        tags.Add(new KeyValuePair<string, object>("SpanId", spanContext.SpanId.ToString()));
        using (logger.BeginScope(tags))
        {
            logger.LogWarning(logMessage);
        }
    }

    /// <summary>
    /// Log message with span context and level error.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="spanContext">Current span context.</param>
    /// <param name="message">The log message.</param>
    /// <param name="exception">The exception.</param>
    /// <param name="tags">The log tags.</param>
    /// <param name="filePath">Caller file path.</param>
    /// <param name="memberName">Caller method name.</param>
    /// <param name="lineNumber">Caller line number.</param>
    public static void Error(
        this ILogger logger,
        SpanContext spanContext,
        string message,
        Exception exception,
        List<KeyValuePair<string, object>>? tags = null,
        [CallerFilePath] string filePath = "",
        [CallerMemberName] string memberName = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        var logMessage = $"[{fileName}.{memberName}:{lineNumber}] {message}";
        if (tags == null)
        {
            tags = new List<KeyValuePair<string, object>>();
        }

        tags.Add(new KeyValuePair<string, object>("TraceId", spanContext.TraceId.ToString()));
        tags.Add(new KeyValuePair<string, object>("SpanId", spanContext.SpanId.ToString()));
        tags.Add(new KeyValuePair<string, object>("ExceptionMessage", exception.Message));
        tags.Add(new KeyValuePair<string, object>("ExceptionStackTrace", exception.StackTrace ?? "No stack trace"));
        using (logger.BeginScope(tags))
        {
            logger.LogError(logMessage, exception);
        }
    }

    /// <summary>
    /// Log message with span context and level critical.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="spanContext">Current span context.</param>
    /// <param name="message">The log message.</param>
    /// <param name="exception">The exception.</param>
    /// <param name="tags">The log tags.</param>
    /// <param name="filePath">Caller file path.</param>
    /// <param name="memberName">Caller method name.</param>
    /// <param name="lineNumber">Caller line number.</param>
    public static void Critical(
        this ILogger logger,
        SpanContext spanContext,
        string message,
        Exception exception,
        List<KeyValuePair<string, object>>? tags = null,
        [CallerFilePath] string filePath = "",
        [CallerMemberName] string memberName = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        var logMessage = $"[{fileName}.{memberName}:{lineNumber}] {message}";
        if (tags == null)
        {
            tags = new List<KeyValuePair<string, object>>();
        }

        tags.Add(new KeyValuePair<string, object>("TraceId", spanContext.TraceId.ToString()));
        tags.Add(new KeyValuePair<string, object>("SpanId", spanContext.SpanId.ToString()));
        using (logger.BeginScope(tags))
        {
            logger.LogCritical(logMessage, exception);
        }
    }
}
