using BenchmarkDotNet.Attributes;

namespace Benchmark
{
    [MarkdownExporter]
    public class AdlerBenchmark : BenchmarkBase
    {
        [Benchmark]
        public uint Adler32Simple()
        {
            return AdlerHash.Adler32.GetAdler32Simple(Data, 1, 0);
        }

        [Benchmark]
        public uint Adler32Sse()
        {
            return AdlerHash.Adler32.GetAdler32Sse(Data, 1, 0);
        }

        [Benchmark]
        public ulong Adler64Simple()
        {
            return AdlerHash.Adler64.GetAdler64Simple(Data, 1, 0);
        }

        [Benchmark]
        public ulong Adler64Sse()
        {
            return AdlerHash.Adler64.GetAdler64Sse(Data, 1, 0);
        }
    }
}
