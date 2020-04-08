using BenchmarkDotNet.Attributes;

namespace Benchmark
{
    [MarkdownExporter]
    public class Adler32Benchmark : BenchmarkBase
    {
        [Benchmark(Baseline = true)]
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
        public uint Adler32Avx()
        {
            return AdlerHash.Adler32.GetAvx(Data, 1, 0);
        }
    }
}
