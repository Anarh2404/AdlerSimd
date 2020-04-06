# AdlerSimd

It is adapted from Jean-Loup Gailly's and Mark Adler's original implementation in zlib. A copy of the zlib copyright and license can be found in LICENSE-ZLIB.

``` ini

BenchmarkDotNet=v0.12.0, OS=Windows 10.0.18363
Intel Core i7-8700 CPU 3.20GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET Core SDK=3.1.201
  [Host]     : .NET Core 3.1.3 (CoreCLR 4.700.20.11803, CoreFX 4.700.20.12001), X64 RyuJIT
  DefaultJob : .NET Core 3.1.3 (CoreCLR 4.700.20.11803, CoreFX 4.700.20.12001), X64 RyuJIT


```
|           Method | TestArrayLength |            Mean |         Error |        StdDev |
|----------------- |---------------- |----------------:|--------------:|--------------:|
|    **Adler32Simple** |         **1048576** |     **3,594.23 us** |     **25.449 us** |     **23.805 us** |
| Adler32Optimized |         1048576 |       541.45 us |      5.078 us |      4.750 us |
|       Adler32Sse |         1048576 |        49.10 us |      0.567 us |      0.503 us |
|    Adler64Simple |         1048576 |     3,334.92 us |     25.314 us |     22.440 us |
| Adler64Optimized |         1048576 |       536.59 us |      2.535 us |      2.371 us |
|       Adler64Sse |         1048576 |        62.91 us |      0.319 us |      0.283 us |
|    **Adler32Simple** |       **268435456** |   **923,191.13 us** | **13,772.644 us** | **12,209.090 us** |
| Adler32Optimized |       268435456 |   139,355.93 us |    989.044 us |    925.152 us |
|       Adler32Sse |       268435456 |    19,108.23 us |    158.698 us |    148.446 us |
|    Adler64Simple |       268435456 |   849,647.52 us |  6,967.051 us |  6,516.984 us |
| Adler64Optimized |       268435456 |   137,933.44 us |    594.936 us |    527.395 us |
|       Adler64Sse |       268435456 |    21,754.38 us |    423.876 us |    504.595 us |
|    **Adler32Simple** |      **1073741824** | **3,676,858.77 us** | **24,584.128 us** | **22,996.009 us** |
| Adler32Optimized |      1073741824 |   558,903.21 us |  3,535.110 us |  3,306.744 us |
|       Adler32Sse |      1073741824 |    77,804.67 us |  1,516.769 us |  2,449.299 us |
|    Adler64Simple |      1073741824 | 3,405,335.00 us | 31,084.832 us | 29,076.772 us |
| Adler64Optimized |      1073741824 |   551,345.78 us |  3,071.511 us |  2,722.814 us |
|       Adler64Sse |      1073741824 |   395,009.99 us |  3,018.541 us |  2,823.545 us |
