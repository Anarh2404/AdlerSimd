using BenchmarkDotNet.Attributes;

namespace Benchmark
{
    [MarkdownExporter]
    public class Adler64Benchmark : BenchmarkBase
    {
        [Benchmark(Baseline = true)]
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
