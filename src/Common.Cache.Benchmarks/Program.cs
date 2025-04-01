// See https://aka.ms/new-console-template for more information

using System;
using BenchmarkDotNet.Running;
using Common.Cache.Benchmarks;

BenchmarkRunner.Run<CacheProviderBenchmark>();