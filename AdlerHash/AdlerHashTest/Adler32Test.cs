using System;
using System.Linq;
using Xunit;

namespace AdlerHashTest
{
    public class Adler32Test
    {
        [Fact]
        public void AdlerAlignmentSseTest()
        {
            var bytes = Enumerable.Range(0, 1024).Select(i => (byte)i).ToArray();

            var simpleHash = AdlerHash.Adler32.GetAdler32Simple(bytes, 1, 0);
            var sseHash = AdlerHash.Adler32.GetAdler32Sse(bytes, 1, 0);

            Assert.Equal(simpleHash, sseHash);
        }


        [Fact]
        public void AdlerNotAlignmentSseTest()
        {
            var bytes = Enumerable.Range(0, 1123).Select(i => (byte)i).ToArray();

            var simpleHash = AdlerHash.Adler32.GetAdler32Simple(bytes, 1, 0);
            var sseHash = AdlerHash.Adler32.GetAdler32Sse(bytes, 1, 0);

            Assert.Equal(simpleHash, sseHash);
        }
    }
}
