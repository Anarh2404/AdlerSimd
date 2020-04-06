using System;
using System.Linq;
using Xunit;

namespace AdlerHashTest
{
    public class Adler64Test
    {
        [Theory]
        [InlineData(1024)]
        [InlineData(1024 * 1024)]
        [InlineData(32 * 1024 * 1024)]
        [InlineData(128 * 1024 * 1024)]
        [InlineData(363898431)]
        [InlineData(363898432)]
        [InlineData(1024 * 1024 * 1024)]
        public void AdlerAlignmentSseTest(int size)
        {
            var bytes = Enumerable.Range(0, size).Select(i => (byte)i).ToArray();

            var sseHash = AdlerHash.Adler64.GetSse(bytes, 1, 0);
            var simpleHash = AdlerHash.Adler64.GetSimple(bytes, 1, 0);


            Assert.Equal(simpleHash, sseHash);
        }


        [Theory]
        [InlineData(1024)]
        [InlineData(1024 * 1024)]
        [InlineData(32 * 1024 * 1024)]
        [InlineData(128 * 1024 * 1024)]
        [InlineData(363898431)]
        [InlineData(363898432)]
        [InlineData(1024 * 1024 * 1024)]
        public void AdlerNotAlignmentSseTest(int size)
        {
            var bytes = Enumerable.Range(0, size).Select(i => (byte)i).ToArray();

            var simpleHash = AdlerHash.Adler64.GetSimple(bytes, 1, 0);
            var sseHash = AdlerHash.Adler64.GetSse(bytes, 1, 0);

            Assert.Equal(simpleHash, sseHash);
        }

        [Theory]
        [InlineData(1024)]
        [InlineData(1024 * 1024)]
        [InlineData(32 * 1024 * 1024)]
        [InlineData(128 * 1024 * 1024)]
        [InlineData(363898431)]
        [InlineData(363898432)]
        [InlineData(1024 * 1024 * 1024)]
        public void AdlerAlignmentSTest(int size)
        {
            var bytes = Enumerable.Range(0, size).Select(i => (byte)i).ToArray();

            var sseHash = AdlerHash.Adler64.GetSimpleOptimized(bytes, 1, 0);
            var simpleHash = AdlerHash.Adler64.GetSimple(bytes, 1, 0);

            Assert.Equal(simpleHash, sseHash);
        }


        [Theory]
        [InlineData(1024)]
        [InlineData(1024 * 1024)]
        [InlineData(32 * 1024 * 1024)]
        [InlineData(128 * 1024 * 1024)]
        [InlineData(363898431)]
        [InlineData(363898432)]
        [InlineData(1024 * 1024 * 1024)]
        public void AdlerNotAlignmentSTest(int size)
        {
            var bytes = Enumerable.Range(0, size).Select(i => (byte)i).ToArray();

            var simpleHash = AdlerHash.Adler64.GetSimple(bytes, 1, 0);
            var sseHash = AdlerHash.Adler64.GetSimpleOptimized(bytes, 1, 0);

            Assert.Equal(simpleHash, sseHash);
        }


        [Theory]
        [InlineData(1024)]
        [InlineData(1024 * 1024)]
        [InlineData(32 * 1024 * 1024)]
        [InlineData(128 * 1024 * 1024)]
        [InlineData(363898431)]
        [InlineData(363898432)]
        [InlineData(1024 * 1024 * 1024)]
        public void AdlerAlignmentSseTest2(int size)
        {
            ReadOnlySpan<byte> bytes = Enumerable.Range(0, size).Select(i => (byte)i).ToArray();


            ulong s1 = 1;
            ulong s2 = 0;
            var simpleHash = AdlerHash.Adler64.GetSimple(bytes.Slice(0, size / 2), s1, s2);
            s1 = simpleHash & 0xffffffff;
            s2 = simpleHash >> 32;
            var simpleHash2 = AdlerHash.Adler64.GetSimple(bytes.Slice(size / 2), s1, s2);
            var simpleHash3 = AdlerHash.Adler64.GetSimple(bytes, 1, 0);


            s1 = 1;
            s2 = 0;
            var sseHash = AdlerHash.Adler64.GetSse(bytes.Slice(0, size / 2), s1, s2);
            s1 = sseHash & 0xffffffff;
            s2 = sseHash >> 32;
            var sseHash2 = AdlerHash.Adler64.GetSse(bytes.Slice(size / 2), s1, s2);
            var sseHash3 = AdlerHash.Adler64.GetSse(bytes, 1, 0);


            Assert.Equal(simpleHash2, simpleHash3);
            Assert.Equal(sseHash2, sseHash3);
            Assert.Equal(simpleHash3, sseHash3);
        }


        [Theory]
        [InlineData(1024)]
        [InlineData(1024 * 1024)]
        [InlineData(32 * 1024 * 1024)]
        [InlineData(128 * 1024 * 1024)]
        [InlineData(363898431)]
        [InlineData(363898432)]
        [InlineData(1024 * 1024 * 1024)]
        public void AdlerAlignmentSimpleOptimized(int size)
        {
            ReadOnlySpan<byte> bytes = Enumerable.Range(0, size).Select(i => (byte)i).ToArray();


            ulong s1 = 1;
            ulong s2 = 0;
            var simpleHash = AdlerHash.Adler64.GetSimple(bytes.Slice(0, size / 2), s1, s2);
            s1 = simpleHash & 0xffffffff;
            s2 = simpleHash >> 32;
            var simpleHash2 = AdlerHash.Adler64.GetSimple(bytes.Slice(size / 2), s1, s2);
            var simpleHash3 = AdlerHash.Adler64.GetSimple(bytes, 1, 0);


            s1 = 1;
            s2 = 0;
            var sseHash = AdlerHash.Adler64.GetSimpleOptimized(bytes.Slice(0, size / 2), s1, s2);
            s1 = sseHash & 0xffffffff;
            s2 = sseHash >> 32;
            var sseHash2 = AdlerHash.Adler64.GetSimpleOptimized(bytes.Slice(size / 2), s1, s2);
            var sseHash3 = AdlerHash.Adler64.GetSimpleOptimized(bytes, 1, 0);


            Assert.Equal(simpleHash2, simpleHash3);
            Assert.Equal(sseHash2, sseHash3);
            Assert.Equal(simpleHash3, sseHash3);
        }
    }
}
