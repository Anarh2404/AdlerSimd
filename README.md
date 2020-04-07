# AdlerSimd

It is adapted from Jean-Loup Gailly's and Mark Adler's original implementation in zlib. A copy of the zlib copyright and license can be found in LICENSE-ZLIB.


## Benchmark

``` ini

BenchmarkDotNet=v0.12.0, OS=Windows 10.0.18363
Intel Core i7-8700 CPU 3.20GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET Core SDK=3.1.201
  [Host]     : .NET Core 3.1.3 (CoreCLR 4.700.20.11803, CoreFX 4.700.20.12001), X64 RyuJIT
  DefaultJob : .NET Core 3.1.3 (CoreCLR 4.700.20.11803, CoreFX 4.700.20.12001), X64 RyuJIT


```

### Adler32

|           Method | TestArrayLength |            Mean |         Error |        StdDev | Ratio |
|----------------- |---------------- |----------------:|--------------:|--------------:|------:|
|    **Adler32Simple** |         **1048576** |     **3,565.44 us** |     **19.201 us** |     **17.961 us** |  **1.00** |
| Adler32Optimized |         1048576 |       539.25 us |      5.096 us |      4.767 us |  0.15 |
|       Adler32Sse |         1048576 |        48.05 us |      0.217 us |      0.181 us |  0.01 |
|                  |                 |                 |               |               |       |
|    **Adler32Simple** |       **268435456** |   **918,145.36 us** |  **8,338.326 us** |  **7,799.675 us** |  **1.00** |
| Adler32Optimized |       268435456 |   137,536.77 us |    872.057 us |    815.722 us |  0.15 |
|       Adler32Sse |       268435456 |    18,843.57 us |    161.503 us |    143.168 us |  0.02 |
|                  |                 |                 |               |               |       |
|    **Adler32Simple** |      **1073741824** | **3,655,335.97 us** | **26,997.570 us** | **25,253.544 us** |  **1.00** |
| Adler32Optimized |      1073741824 |   553,457.84 us |  2,208.464 us |  1,957.746 us |  0.15 |
|       Adler32Sse |      1073741824 |    75,345.60 us |    705.915 us |    625.776 us |  0.02 |

### Adler64

|           Method | TestArrayLength |            Mean |         Error |        StdDev | Ratio |
|----------------- |---------------- |----------------:|--------------:|--------------:|------:|
|    **Adler64Simple** |         **1048576** |     **3,278.89 us** |     **11.125 us** |     **10.406 us** |  **1.00** |
| Adler64Optimized |         1048576 |       537.42 us |      7.890 us |      6.160 us |  0.16 |
|       Adler64Sse |         1048576 |        62.79 us |      0.287 us |      0.254 us |  0.02 |
|                  |                 |                 |               |               |       |
|    **Adler64Simple** |       **268435456** |   **840,513.09 us** |  **5,000.305 us** |  **4,432.640 us** |  **1.00** |
| Adler64Optimized |       268435456 |   136,463.75 us |    516.511 us |    431.310 us |  0.16 |
|       Adler64Sse |       268435456 |    21,027.56 us |    208.845 us |    195.354 us |  0.02 |
|                  |                 |                 |               |               |       |
|    **Adler64Simple** |      **1073741824** | **3,407,771.96 us** | **41,674.338 us** | **36,943.215 us** |  **1.00** |
| Adler64Optimized |      1073741824 |   553,194.05 us |  6,017.076 us |  5,628.376 us |  0.16 |
|       Adler64Sse |      1073741824 |    84,412.36 us |    652.594 us |    544.946 us |  0.02 |