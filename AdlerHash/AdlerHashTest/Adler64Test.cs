using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace AdlerHashTest
{
    public class Adler64Test
    {
        [Fact]
        public void AdlerAlignmentSseTest()
        {
            var bytes = Enumerable.Range(0, 1024).Select(i => (byte)i).ToArray();

            var simpleHash = AdlerHash.Adler64.GetAdler64Simple(bytes, 1, 0);
            var sseHash = AdlerHash.Adler64.GetAdler64Sse(bytes, 1, 0);

            Assert.Equal(simpleHash, sseHash);
        }


        [Fact]
        public void AdlerNotAlignmentSseTest()
        {
            var bytes = Enumerable.Range(0, 1123).Select(i => (byte)i).ToArray();

            var simpleHash = AdlerHash.Adler64.GetAdler64Simple(bytes, 1, 0);
            var sseHash = AdlerHash.Adler64.GetAdler64Sse(bytes, 1, 0);

            Assert.Equal(simpleHash, sseHash);
        }
    }
}
