using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BenchmarkDotNet.Attributes;

namespace Benchmark
{
    public class BenchmarkBase
    {
        internal byte[] Data;

        [Params(1024, 1024 * 1024, 1024 * 1024 * 1024)]
        public int TestArrayLength { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            Data = Enumerable.Range(0, TestArrayLength).Select(i => (byte)i).ToArray();
        }
    }
}
