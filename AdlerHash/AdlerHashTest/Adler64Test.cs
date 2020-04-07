using System;
using System.Linq;
using Xunit;

namespace AdlerHashTest
{
    public class Adler64Test
    {
        [Theory]
        [InlineData(1024, 263370418247826945)]
        [InlineData(1024 * 1024, 12590163184829595649)]
        [InlineData(32 * 1024 * 1024, 15851262394320814081)]
        [InlineData(128 * 1024 * 1024, 12371384551367245840)]
        [InlineData(363898432, 3190231801733494035)]
        [InlineData(1024 * 1024 * 1024, 14699720625962877084)]
        public void AdlerSseAlignedDataTest(int size, ulong result)
        {
            ReadOnlySpan<byte> testData = Enumerable.Range(0, size).Select(i => (byte)i).ToArray();


            var sseHash = AdlerHash.Adler64.GetSse(testData, 1, 0);


            Assert.Equal(result, sseHash);
        }


        [Theory]
        [InlineData(1024 + 7, 267295945341926934)]
        [InlineData(1024 * 1024 + 13, 1608137134164148303)]
        [InlineData(32 * 1024 * 1024 + 17, 14626287238456017033)]
        [InlineData(128 * 1024 * 1024 + 23, 5742096667360887053)]
        [InlineData(363898431, 6830630035825276116)]
        [InlineData(363898437, 3434988892577248861)]
        [InlineData(1024 * 1024 * 1024 + 31, 17005606288496788077)]
        public void AdlerSseNotAlignedDataTest(int size, ulong result)
        {
            ReadOnlySpan<byte> testData = Enumerable.Range(0, size).Select(i => (byte)i).ToArray();


            var sseHash = AdlerHash.Adler64.GetSse(testData, 1, 0);


            Assert.Equal(result, sseHash);
        }


        [Theory]
        [InlineData(1024, 263370418247826945)]
        [InlineData(1024 * 1024, 12590163184829595649)]
        [InlineData(32 * 1024 * 1024, 15851262394320814081)]
        [InlineData(128 * 1024 * 1024, 12371384551367245840)]
        [InlineData(363898432, 3190231801733494035)]
        [InlineData(1024 * 1024 * 1024, 14699720625962877084)]
        public void AdlerSimpleOptimizedAlignedDataTest(int size, ulong result)
        {
            ReadOnlySpan<byte> testData = Enumerable.Range(0, size).Select(i => (byte)i).ToArray();


            var sseHash = AdlerHash.Adler64.GetSimpleOptimized(testData, 1, 0);


            Assert.Equal(result, sseHash);
        }


        [Theory]
        [InlineData(1024 + 7, 267295945341926934)]
        [InlineData(1024 * 1024 + 13, 1608137134164148303)]
        [InlineData(32 * 1024 * 1024 + 17, 14626287238456017033)]
        [InlineData(128 * 1024 * 1024 + 23, 5742096667360887053)]
        [InlineData(363898431, 6830630035825276116)]
        [InlineData(363898437, 3434988892577248861)]
        [InlineData(1024 * 1024 * 1024 + 31, 17005606288496788077)]
        public void AdlerSimpleOptimizedNotAlignedDataTest(int size, ulong result)
        {
            ReadOnlySpan<byte> testData = Enumerable.Range(0, size).Select(i => (byte)i).ToArray();


            var hash = AdlerHash.Adler64.GetSimpleOptimized(testData, 1, 0);


            Assert.Equal(result, hash);
        }


        [Theory]
        [InlineData(1024, 263370418247826945)]
        [InlineData(1024 * 1024, 12590163184829595649)]
        [InlineData(32 * 1024 * 1024, 15851262394320814081)]
        [InlineData(128 * 1024 * 1024, 12371384551367245840)]
        [InlineData(363898432, 3190231801733494035)]
        [InlineData(1024 * 1024 * 1024, 14699720625962877084)]
        public void AdlerSseTwoPassesTest(int size, ulong result)
        {
            ReadOnlySpan<byte> testData = Enumerable.Range(0, size).Select(i => (byte)i).ToArray();
            ulong s1 = 1;
            ulong s2 = 0;


            var firstPass = AdlerHash.Adler64.GetSse(testData.Slice(0, size / 2), s1, s2);
            s1 = firstPass & 0xffffffff;
            s2 = firstPass >> 32;
            var hash = AdlerHash.Adler64.GetSse(testData.Slice(size / 2), s1, s2);


            Assert.Equal(result, hash);
        }


        [Theory]
        [InlineData(1024, 263370418247826945)]
        [InlineData(1024 * 1024, 12590163184829595649)]
        [InlineData(32 * 1024 * 1024, 15851262394320814081)]
        [InlineData(128 * 1024 * 1024, 12371384551367245840)]
        [InlineData(363898432, 3190231801733494035)]
        [InlineData(1024 * 1024 * 1024, 14699720625962877084)]
        public void AdlerSimpleOptimizedTwoPassesTest(int size, ulong result)
        {
            ReadOnlySpan<byte> testData = Enumerable.Range(0, size).Select(i => (byte)i).ToArray();
            ulong s1 = 1;
            ulong s2 = 0;


            var sseHash = AdlerHash.Adler64.GetSimpleOptimized(testData.Slice(0, size / 2), s1, s2);
            s1 = sseHash & 0xffffffff;
            s2 = sseHash >> 32;
            var sseHash2 = AdlerHash.Adler64.GetSimpleOptimized(testData.Slice(size / 2), s1, s2);


            Assert.Equal(result, sseHash2);
        }
    }
}
