using BenchmarkDotNet.Attributes;

namespace Benchmark
{
    [MarkdownExporter]
    public class AdlerBenchmark : BenchmarkBase
    {
        [Benchmark]
        public uint Adler32Simple()
        {
            return AdlerHash.Adler32.GetSimple(Data, 1, 0);
        }

        [Benchmark]
        public uint Adler32Optimized()
        {
            return AdlerHash.Adler32.GetSimpleOptimized(Data, 1, 0);
        }

        [Benchmark]
        public uint Adler32Sse()
        {
            return AdlerHash.Adler32.GetSse(Data, 1, 0);
        }

        [Benchmark]
        public ulong Adler64Simple()
        {
            return AdlerHash.Adler64.GetSimple(Data, 1, 0);
        }

        [Benchmark]
        public ulong Adler64Optimized()
        {
            return AdlerHash.Adler64.GetSimpleOptimized(Data, 1, 0);
        }

        [Benchmark]
        public ulong Adler64Sse()
        {
            return AdlerHash.Adler64.GetSse(Data, 1, 0);
        }
    }
}
