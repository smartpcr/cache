## Evaluate hybrid cache

L2: file cache and windows registry


Benchmark

```txt
| Method               | PayloadSize | Mean             | Error          | StdDev         | Gen0         | Gen1      | Gen2      | Allocated      |
|--------------------- |------------ |-----------------:|---------------:|---------------:|-------------:|----------:|----------:|---------------:|
| MemoryCacheBenchmark | 16          |         1.336 us |      0.0241 us |      0.0277 us |       0.3452 |    0.0038 |         - |        6.37 KB |
| FileCacheBenchmark   | 16          |               NA |             NA |             NA |           NA |        NA |        NA |             NA |
| WinRegistryBenchmark | 16          |       391.679 us |     26.4761 us |     78.0654 us |       5.3711 |         - |         - |      100.64 KB |
| HybridBenchmark      | 16          |               NA |             NA |             NA |           NA |        NA |        NA |             NA |
| MemoryCacheBenchmark | 16348       |         3.137 us |      0.0580 us |      0.1284 us |       1.2131 |    0.0496 |         - |       22.32 KB |
| FileCacheBenchmark   | 16348       |               NA |             NA |             NA |           NA |        NA |        NA |             NA |
| WinRegistryBenchmark | 16348       |    18,070.460 us |    598.1434 us |  1,725.7807 us |    4593.7500 | 1375.0000 |  343.7500 |    78572.69 KB |
| HybridBenchmark      | 16348       |               NA |             NA |             NA |           NA |        NA |        NA |             NA |
| MemoryCacheBenchmark | 5242880     |       923.542 us |     17.8276 us |     21.2225 us |     171.8750 |  171.8750 |  171.8750 |     5127.38 KB |
| FileCacheBenchmark   | 5242880     |               NA |             NA |             NA |           NA |        NA |        NA |             NA |
| WinRegistryBenchmark | 5242880     | 4,053,757.810 us | 76,695.9072 us | 91,301.0299 us | 1367000.0000 | 8000.0000 | 1000.0000 | 25260498.37 KB |
| HybridBenchmark      | 5242880     |     1,948.237 us |     52.1963 us |    145.5023 us |      70.3125 |   70.3125 |   70.3125 |     5138.03 KB |
```