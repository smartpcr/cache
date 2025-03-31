// <copyright file="PerformanceMetrics.cs" company="Microsoft Corp">
// Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>

namespace OpenTelemetry.Lib;

using System.Diagnostics;

/// <summary>
/// Process performance metrics.
/// </summary>
public class PerformanceMetrics
{
    private readonly Process currentProcess;
    private readonly PerformanceCounter cpuCounter;
    private readonly PerformanceCounter exceptionCounter;
    private readonly PerformanceCounter contentionCounter;
    private readonly PerformanceCounter pageFaultsCounter;
    private readonly PerformanceCounter ioDataBytesCounter;
    private readonly PerformanceCounter bytesInAllHeapsCounter;
    private readonly PerformanceCounter privateBytesCounter;

    /// <summary>
    /// Initializes a new instance of the <see cref="PerformanceMetrics"/> class.
    /// </summary>
    public PerformanceMetrics()
    {
        this.currentProcess = Process.GetCurrentProcess();
        var processName = this.currentProcess.ProcessName;
        this.cpuCounter = new PerformanceCounter("Process", "% Processor Time", processName, true);
        this.exceptionCounter = new PerformanceCounter(".NET CLR Exceptions", "# of Exceptions Thrown", processName, true);
        this.contentionCounter = new PerformanceCounter(".NET CLR LocksAndThreads", "Total # of Contentions", processName, true);
        this.pageFaultsCounter = new PerformanceCounter("Process", "Page Faults/sec", processName, true);
        this.ioDataBytesCounter = new PerformanceCounter("Process", "IO Data Bytes/sec", processName, true);
        this.bytesInAllHeapsCounter = new PerformanceCounter(".NET CLR Memory", "# Bytes in all Heaps", processName, true);
        this.privateBytesCounter = new PerformanceCounter("Process", "Private Bytes", processName, true);
    }

    /// <summary>
    /// Get current process name.
    /// </summary>
    /// <returns>The name of current process.</returns>
    public string GetProcessName() => this.currentProcess.ProcessName;

    /// <summary>
    /// Get the process ID.
    /// </summary>
    /// <returns>Process id.</returns>
    public int GetProcessId() => this.currentProcess.Id;

    /// <summary>
    /// Get current CPU usage percentage.
    /// </summary>
    /// <returns>CPU usage percentage.</returns>
    public double GetCpuUsage() => this.cpuCounter.NextValue() / Environment.ProcessorCount;

    /// <summary>
    /// Get current memory usage in MB.
    /// </summary>
    /// <returns>Memory usage in MB.</returns>
    public long GetUsedMemory()
    {
        this.currentProcess.Refresh();
        return this.currentProcess.WorkingSet64 / (1024 * 1024); // Convert from bytes to MB
    }

    /// <summary>
    /// Get the number of threads in the current process.
    /// </summary>
    /// <returns>The number of threads.</returns>
    public int GetThreadCount() => this.currentProcess.Threads.Count;

    /// <summary>
    /// Get the number of handles in the current process.
    /// </summary>
    /// <returns>The number of handles.</returns>
    public int GetHandleCount() => this.currentProcess.HandleCount;

    /// <summary>
    /// Get the number of exceptions thrown.
    /// </summary>
    /// <returns>Number of exception thrown.</returns>
    public int GetExceptionCount() => (int)this.exceptionCounter.NextValue();

    /// <summary>
    /// Get the number of resource contentions.
    /// </summary>
    /// <returns>Number of contentions.</returns>
    public int GetContentionCount() => (int)this.contentionCounter.NextValue();


    /// <summary>
    /// Get page faults per second.
    /// </summary>
    /// <returns>Page fault count per second.</returns>
    public double GetPageFaultsRate()
    {
        return this.pageFaultsCounter.NextValue();
    }

    /// <summary>
    /// Get IO data bytes per second.
    /// </summary>
    /// <returns>IO bytes per second.</returns>
    public double GetIOBytesRate()
    {
        return this.ioDataBytesCounter.NextValue();
    }

    /// <summary>
    /// Stack memory size in bytes, calculated by privateMemory - heapMemory
    /// </summary>
    /// <returns>Stack memory size in bytes</returns>
    public long GetStackMemorySize()
    {
        return (long)this.privateBytesCounter.NextValue() - (long)this.bytesInAllHeapsCounter.NextValue();
    }

    /// <summary>
    /// Heap memory size in bytes.
    /// </summary>
    /// <returns>Heap memory size in bytes.</returns>
    public long GetHeapMemorySize()
    {
        return (long)this.bytesInAllHeapsCounter.NextValue();
    }
}
