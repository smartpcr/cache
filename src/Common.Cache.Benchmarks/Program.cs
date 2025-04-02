//-------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
//-------------------------------------------------------------------------------

namespace Common.Cache.Benchmarks
{
    using System.Threading;

    using BenchmarkDotNet.Attributes;
    using BenchmarkDotNet.Running;

    [MemoryDiagnoser]
    public class Program
    {
        public static void Main(string[] args)
        {
            ThreadPool.SetMinThreads(100, 100);
            ThreadPool.SetMaxThreads(100, 100);
            BenchmarkSwitcher.FromAssemblies(new[] { typeof(Program).Assembly }).Run(args);
        }
    }
}