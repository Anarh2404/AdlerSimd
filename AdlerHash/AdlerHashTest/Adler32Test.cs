using System;
using System.Linq;
using Xunit;

namespace AdlerHashTest
{
    public class Adler32Test
    {
        [Theory]
        [InlineData(1024, 3838443024)]
        [InlineData(1024 * 1024, 1185183625)]
        [InlineData(32 * 1024 * 1024, 2524836307)] 
        [InlineData(128 * 1024 * 1024, 4178691958)]
        [InlineData(363898432, 3303655557)] 
        [InlineData(1024 * 1024 * 1024, 3590863875)]
        public void AdlerAlignedOptimizedTest(int size, uint expected)
        {
            var bytes = Enumerable.Range(0, size).Select(i => (byte)i).ToArray();
            
            
            var hash = AdlerHash.Adler32.GetSimpleOptimized(bytes, 1, 0);

            
            Assert.Equal(expected, hash);
        }
        
        [Theory]
        [InlineData(1024, 3838443024)]
        [InlineData(1024 * 1024, 1185183625)]
        [InlineData(32 * 1024 * 1024, 2524836307)] 
        [InlineData(128 * 1024 * 1024, 4178691958)]
        [InlineData(363898432, 3303655557)] 
        [InlineData(1024 * 1024 * 1024, 3590863875)]
        public void AdlerAlignedOptimizedTwoPassesTest(int size, uint expected)
        {
            ReadOnlySpan<byte> bytes = Enumerable.Range(0, size).Select(i => (byte)i).ToArray();
            uint s1 = 1;
            uint s2 = 0;

            
            var hash = AdlerHash.Adler32.GetSimpleOptimized(bytes.Slice(0, size / 2), s1, s2);
            s1 = hash & 0xffff;
            s2 = hash >> 16;
            hash = AdlerHash.Adler32.GetSimpleOptimized(bytes.Slice(size / 2), s1, s2);


            Assert.Equal(expected, hash);
        }
        
        [Theory]
        [InlineData(1024 +3, 3744136723)]
        [InlineData(1024 * 1024 + 7, 2345170846)]
        [InlineData(32 * 1024 * 1024 + 11, 4227920394)] 
        [InlineData(128 * 1024 * 1024 + 13, 471910340)]
        [InlineData(363898431, 4233480262)]
        [InlineData(1024 * 1024 * 1024 + 27, 932068706)]
        public void AdlerNotAlignedOptimizedTest(int size, uint expected)
        {
            var bytes = Enumerable.Range(0, size).Select(i => (byte)i).ToArray();
            
            
            var hash = AdlerHash.Adler32.GetSimpleOptimized(bytes, 1, 0);

            
            Assert.Equal(expected, hash);
        }
        
        [Theory]
        [InlineData(1024 +3, 3744136723)]
        [InlineData(1024 * 1024 + 7, 2345170846)]
        [InlineData(32 * 1024 * 1024 + 11, 4227920394)] 
        [InlineData(128 * 1024 * 1024 + 13, 471910340)]
        [InlineData(363898431, 4233480262)]
        [InlineData(1024 * 1024 * 1024 + 27, 932068706)]
        public void AdlerNotAlignedOptimizedTwoPassesTest(int size, uint expected)
        {
            ReadOnlySpan<byte> bytes = Enumerable.Range(0, size).Select(i => (byte)i).ToArray();
            uint s1 = 1;
            uint s2 = 0;

            
            var hash = AdlerHash.Adler32.GetSimpleOptimized(bytes.Slice(0, size / 2), s1, s2);
            s1 = hash & 0xffff;
            s2 = hash >> 16;
            hash = AdlerHash.Adler32.GetSimpleOptimized(bytes.Slice(size / 2), s1, s2);


            Assert.Equal(expected, hash);
        }
        
        [Theory]
        [InlineData(1024, 3838443024)]
        [InlineData(1024 * 1024, 1185183625)]
        [InlineData(32 * 1024 * 1024, 2524836307)] 
        [InlineData(128 * 1024 * 1024, 4178691958)]
        [InlineData(363898432, 3303655557)] 
        [InlineData(1024 * 1024 * 1024, 3590863875)]
        public void AdlerAlignedSseTest(int size, uint expected)
        {
            var bytes = Enumerable.Range(0, size).Select(i => (byte)i).ToArray();
            
            
            var hash = AdlerHash.Adler32.GetSse(bytes, 1, 0);

            
            Assert.Equal(expected, hash);
        }

        
        [Theory]
        [InlineData(1024, 3838443024)]
        [InlineData(1024 * 1024, 1185183625)]
        [InlineData(32 * 1024 * 1024, 2524836307)] 
        [InlineData(128 * 1024 * 1024, 4178691958)]
        [InlineData(363898432, 3303655557)] 
        [InlineData(1024 * 1024 * 1024, 3590863875)]
        public void AdlerAlignedSseTwoPassesTest(int size, uint expected)
        {
            ReadOnlySpan<byte> bytes = Enumerable.Range(0, size).Select(i => (byte)i).ToArray();
            uint s1 = 1;
            uint s2 = 0;
            
            
            var hash = AdlerHash.Adler32.GetSse(bytes.Slice(0, size / 2), s1, s2);
            s1 = hash & 0xffff;
            s2 = hash >> 16;
            hash = AdlerHash.Adler32.GetSse(bytes.Slice(size / 2), s1, s2);

            
            Assert.Equal(expected, hash);
        }
        
        [Theory]
        [InlineData(1024 +3, 3744136723)]
        [InlineData(1024 * 1024 + 7, 2345170846)]
        [InlineData(32 * 1024 * 1024 + 11, 4227920394)] 
        [InlineData(128 * 1024 * 1024 + 13, 471910340)]
        [InlineData(363898431, 4233480262)]
        [InlineData(1024 * 1024 * 1024 + 27, 932068706)]
        public void AdlerNotAlignedSseTest(int size, uint expected)
        {
            var bytes = Enumerable.Range(0, size).Select(i => (byte)i).ToArray();
            
            
            var hash = AdlerHash.Adler32.GetSse(bytes, 1, 0);

            
            Assert.Equal(expected, hash);
        }
        
        [Theory]
        [InlineData(1024 +3, 3744136723)]
        [InlineData(1024 * 1024 + 7, 2345170846)]
        [InlineData(32 * 1024 * 1024 + 11, 4227920394)] 
        [InlineData(128 * 1024 * 1024 + 13, 471910340)]
        [InlineData(363898431, 4233480262)]
        [InlineData(1024 * 1024 * 1024 + 27, 932068706)]
        public void AdlerNotAlignedSseTwoPassesTest(int size, uint expected)
        {
            ReadOnlySpan<byte> bytes = Enumerable.Range(0, size).Select(i => (byte)i).ToArray();
            uint s1 = 1;
            uint s2 = 0;

            
            var hash = AdlerHash.Adler32.GetSse(bytes.Slice(0, size / 2), s1, s2);
            s1 = hash & 0xffff;
            s2 = hash >> 16;
            hash = AdlerHash.Adler32.GetSse(bytes.Slice(size / 2), s1, s2);


            Assert.Equal(expected, hash);
        }
    }
}
