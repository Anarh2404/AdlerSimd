using System;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace AdlerHash
{
    public static class Adler32
    {
        private const uint MOD32 = 65521U;
        private const uint NMAX32 = 5552;
        private const int BLOCK_SIZE = 32;
        private const int BLOCK_SIZE_64 = 64;

        public static uint GetAdler32(ReadOnlySpan<byte> buffer, uint adler = 1)
        {
            uint s1 = adler & 0xffff;
            uint s2 = adler >> 16;
            if (Ssse3.IsSupported)
            {
                return GetSse(buffer, s1, s2);
            }
            return GetSimpleOptimized(buffer, s1, s2);
        }

        internal static uint GetSimple(ReadOnlySpan<byte> buffer, uint s1, uint s2)
        {
            foreach (var n in buffer)
            {
                s1 = (s1 + n) % MOD32;
                s2 = (s2 + s1) % MOD32;
            }

            return (s2 << 16) | s1;
        }

        internal static uint GetSimpleOptimized(ReadOnlySpan<byte> buf, uint adler, uint sum2)
        {
            uint n;
            uint len = (uint)buf.Length;
            if (len == 1)
            {
                adler += buf[0];
                if (adler >= MOD32)
                    adler -= MOD32;
                sum2 += adler;
                if (sum2 >= MOD32)
                    sum2 -= MOD32;
                return adler | (sum2 << 16);
            }
            var idx = 0;
            if (len < 16)
            {
                while (len-- != 0)
                {
                    adler += buf[idx++];
                    sum2 += adler;
                }
                if (adler >= MOD32)
                    adler -= MOD32;
                sum2 = MOD28(sum2);            /* only added so many BASE's */
                return adler | (sum2 << 16);
            }

            /* do length NMAX blocks -- requires just one modulo operation */

            while (len >= NMAX32)
            {
                len -= NMAX32;
                n = NMAX32 / 16;          /* NMAX is divisible by 16 */
                do
                {
                    /* 16 sums unrolled */
                    adler += buf[idx + 0];
                    sum2 += adler;
                    adler += buf[idx + 1];
                    sum2 += adler;
                    adler += buf[idx + 2];
                    sum2 += adler;
                    adler += buf[idx + 3];
                    sum2 += adler;
                    adler += buf[idx + 4];
                    sum2 += adler;
                    adler += buf[idx + 5];
                    sum2 += adler;
                    adler += buf[idx + 6];
                    sum2 += adler;
                    adler += buf[idx + 7];
                    sum2 += adler;
                    adler += buf[idx + 8];
                    sum2 += adler;
                    adler += buf[idx + 9];
                    sum2 += adler;
                    adler += buf[idx + 10];
                    sum2 += adler;
                    adler += buf[idx + 11];
                    sum2 += adler;
                    adler += buf[idx + 12];
                    sum2 += adler;
                    adler += buf[idx + 13];
                    sum2 += adler;
                    adler += buf[idx + 14];
                    sum2 += adler;
                    adler += buf[idx + 15];
                    sum2 += adler;

                    idx += 16;
                } while (--n != 0);
                adler = MOD(adler);
                sum2 = MOD(sum2);
            }

            /* do remaining bytes (less than NMAX, still just one modulo) */
            if (len > 0)
            {                  /* avoid modulos if none remaining */
                while (len >= 16)
                {
                    len -= 16;
                    /* 16 sums unrolled */
                    adler += buf[idx + 0];
                    sum2 += adler;
                    adler += buf[idx + 1];
                    sum2 += adler;
                    adler += buf[idx + 2];
                    sum2 += adler;
                    adler += buf[idx + 3];
                    sum2 += adler;
                    adler += buf[idx + 4];
                    sum2 += adler;
                    adler += buf[idx + 5];
                    sum2 += adler;
                    adler += buf[idx + 6];
                    sum2 += adler;
                    adler += buf[idx + 7];
                    sum2 += adler;
                    adler += buf[idx + 8];
                    sum2 += adler;
                    adler += buf[idx + 9];
                    sum2 += adler;
                    adler += buf[idx + 10];
                    sum2 += adler;
                    adler += buf[idx + 11];
                    sum2 += adler;
                    adler += buf[idx + 12];
                    sum2 += adler;
                    adler += buf[idx + 13];
                    sum2 += adler;
                    adler += buf[idx + 14];
                    sum2 += adler;
                    adler += buf[idx + 15];
                    sum2 += adler;
                    idx += 16;
                }
                while (len-- != 0)
                {
                    adler += buf[idx++];
                    sum2 += adler;
                }
                adler = MOD(adler);
                sum2 = MOD(sum2);
            }

            /* return recombined sums */
            return adler | (sum2 << 16);
        }

        private static uint CHOP(uint a)
        {
            uint tmp = a >> 16;
            a &= 0xffffU;
            a += (tmp << 4) - tmp;
            return a;
        }
        private static uint MOD28(uint a)
        {
            a = CHOP(a);
            if (a >= MOD32)
                a -= MOD32;
            return a;
        }
        private static uint MOD(uint a)
        {
            a = CHOP(a);
            a = MOD28(a);
            return a;
        }


        internal static unsafe uint GetSse(ReadOnlySpan<byte> buffer, uint s1, uint s2)
        {
            uint len = (uint)buffer.Length;

            uint blocks = len / BLOCK_SIZE;
            len = len - blocks * BLOCK_SIZE;

            Vector128<sbyte> tap1 = Vector128.Create(32, 31, 30, 29, 28, 27, 26, 25, 24, 23, 22, 21, 20, 19, 18, 17);
            Vector128<sbyte> tap2 = Vector128.Create(16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1);
            Vector128<byte> zero = Vector128<byte>.Zero;
            Vector128<short> ones = Vector128.Create(1, 1, 1, 1, 1, 1, 1, 1);

            fixed (byte* bufPtr = &MemoryMarshal.GetReference(buffer))
            {
                var buf = bufPtr;

                while (blocks != 0)
                {
                    uint n = NMAX32 / BLOCK_SIZE;
                    if (n > blocks)
                    {
                        n = blocks;
                    }

                    blocks -= n;

                    // Process n blocks of data. At most NMAX data bytes can be
                    // processed before s2 must be reduced modulo BASE.
                    Vector128<uint> v_ps = Vector128.Create(0, 0, 0, s1 * n);
                    Vector128<uint> v_s2 = Vector128.Create(0, 0, 0, s2);
                    Vector128<uint> v_s1 = Vector128.Create(0u, 0, 0, 0);

                    do
                    {
                        // Load 32 input bytes.
                        Vector128<byte> bytes1 = Sse2.LoadVector128(&buf[0]);
                        Vector128<byte> bytes2 = Sse2.LoadVector128(&buf[16]);


                        // Add previous block byte sum to v_ps.
                        v_ps = Sse2.Add(v_ps, v_s1);



                        // Horizontally add the bytes for s1, multiply-adds the
                        // bytes by [ 32, 31, 30, ... ] for s2.
                        Vector128<ushort> sad1 = Sse2.SumAbsoluteDifferences(bytes1, zero);
                        v_s1 = Sse2.Add(v_s1, sad1.AsUInt32());
                        Vector128<short> mad11 = Ssse3.MultiplyAddAdjacent(bytes1, tap1);
                        Vector128<int> mad12 = Sse2.MultiplyAddAdjacent(mad11, ones);
                        v_s2 = Sse2.Add(v_s2, mad12.AsUInt32());


                        Vector128<ushort> sad2 = Sse2.SumAbsoluteDifferences(bytes2, zero);
                        v_s1 = Sse2.Add(v_s1, sad2.AsUInt32());
                        Vector128<short> mad21 = Ssse3.MultiplyAddAdjacent(bytes2, tap2);
                        Vector128<int> mad22 = Sse2.MultiplyAddAdjacent(mad21, ones);
                        v_s2 = Sse2.Add(v_s2, mad22.AsUInt32());

                        buf += BLOCK_SIZE;

                        n--;
                    } while (n != 0);

                    var shift = Sse2.ShiftLeftLogical(v_ps, 5);
                    v_s2 = Sse2.Add(v_s2, shift);


                    // Sum epi32 ints v_s1(s2) and accumulate in s1(s2).

                    // A B C D -> B A D C
                    const int S2301 = 2 << 6 | 3 << 4 | 0 << 2 | 1;
                    // A B C D -> C D A B
                    const int S1032 = 1 << 6 | 0 << 4 | 3 << 2 | 2;

                    v_s1 = Sse2.Add(v_s1, Sse2.Shuffle(v_s1, S2301));
                    v_s1 = Sse2.Add(v_s1, Sse2.Shuffle(v_s1, S1032));
                    s1 += Sse2.ConvertToUInt32(v_s1);
                    v_s2 = Sse2.Add(v_s2, Sse2.Shuffle(v_s2, S2301));
                    v_s2 = Sse2.Add(v_s2, Sse2.Shuffle(v_s2, S1032));
                    s2 = Sse2.ConvertToUInt32(v_s2);

                    s1 %= MOD32;
                    s2 %= MOD32;
                }

                if (len > 0)
                {
                    if (len >= 16)
                    {
                        s2 += (s1 += *buf++);
                        s2 += (s1 += *buf++);
                        s2 += (s1 += *buf++);
                        s2 += (s1 += *buf++);
                        s2 += (s1 += *buf++);
                        s2 += (s1 += *buf++);
                        s2 += (s1 += *buf++);
                        s2 += (s1 += *buf++);
                        s2 += (s1 += *buf++);
                        s2 += (s1 += *buf++);
                        s2 += (s1 += *buf++);
                        s2 += (s1 += *buf++);
                        s2 += (s1 += *buf++);
                        s2 += (s1 += *buf++);
                        s2 += (s1 += *buf++);
                        s2 += (s1 += *buf++);
                        len -= 16;
                    }

                    while (len-- > 0)
                    {
                        s2 += (s1 += *buf++);
                    }
                    if (s1 >= MOD32)
                    {
                        s1 -= MOD32;
                    }

                    s2 %= MOD32;
                }

                return s1 | (s2 << 16);
            }
        }

        internal static unsafe uint GetAvx2(ReadOnlySpan<byte> buffer, uint s1, uint s2)
        {
            uint len = (uint)buffer.Length;

            uint blocks = len / BLOCK_SIZE_64;
            len = len - blocks * BLOCK_SIZE_64;

            Vector256<sbyte> tap1 = Vector256.Create(64, 63, 62, 61, 60, 59, 58, 57, 56, 55, 54, 53, 52, 51, 50, 49, 48, 47, 46, 45, 44, 43, 42, 41, 40, 39, 38, 37, 36, 35, 34, 33);
            Vector256<sbyte> tap2 = Vector256.Create(32, 31, 30, 29, 28, 27, 26, 25, 24, 23, 22, 21, 20, 19, 18, 17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1);
            Vector256<byte> zero = Vector256<byte>.Zero;
            Vector256<short> ones = Vector256.Create(1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1);

            fixed (byte* bufPtr = &MemoryMarshal.GetReference(buffer))
            {
                var buf = bufPtr;

                while (blocks != 0)
                {
                    uint n = NMAX32 / BLOCK_SIZE_64;
                    if (n > blocks)
                    {
                        n = blocks;
                    }

                    blocks -= n;

                    // Process n blocks of data. At most NMAX data bytes can be
                    // processed before s2 must be reduced modulo BASE.
                    Vector256<uint> v_ps = Vector256.Create(0u, 0, 0, 0, 0, 0, 0, s1 * n);
                    Vector256<uint> v_s2 = Vector256.Create(0u, 0, 0, 0, 0, 0, 0, s2);
                    Vector256<uint> v_s1 = Vector256.Create(0u, 0, 0, 0, 0, 0, 0, 0);

                    do
                    {
                        // Load 32 input bytes.
                        Vector256<byte> bytes1 = Avx.LoadVector256(&buf[0]);
                        Vector256<byte> bytes2 = Avx.LoadVector256(&buf[32]);


                        // Add previous block byte sum to v_ps.
                        v_ps = Avx2.Add(v_ps, v_s1);



                        // Horizontally add the bytes for s1, multiply-adds the
                        // bytes by [ 32, 31, 30, ... ] for s2.
                        Vector256<ushort> sad1 = Avx2.SumAbsoluteDifferences(bytes1, zero);
                        v_s1 = Avx2.Add(v_s1, sad1.AsUInt32());
                        Vector256<short> mad11 = Avx2.MultiplyAddAdjacent(bytes1, tap1);
                        Vector256<int> mad12 = Avx2.MultiplyAddAdjacent(mad11, ones);
                        v_s2 = Avx2.Add(v_s2, mad12.AsUInt32());


                        Vector256<ushort> sad2 = Avx2.SumAbsoluteDifferences(bytes2, zero);
                        v_s1 = Avx2.Add(v_s1, sad2.AsUInt32());
                        Vector256<short> mad21 = Avx2.MultiplyAddAdjacent(bytes2, tap2);
                        Vector256<int> mad22 = Avx2.MultiplyAddAdjacent(mad21, ones);
                        v_s2 = Avx2.Add(v_s2, mad22.AsUInt32());

                        buf += BLOCK_SIZE;

                        n--;
                    } while (n != 0);

                    var shift = Avx2.ShiftLeftLogical(v_ps, 5);
                    v_s2 = Avx2.Add(v_s2, shift);


                    // Sum epi32 ints v_s1(s2) and accumulate in s1(s2).

                    // A B C D -> B A D C
                    const int S2301 = 2 << 6 | 3 << 4 | 0 << 2 | 1;
                    // A B C D -> C D A B
                    const int S1032 = 1 << 6 | 0 << 4 | 3 << 2 | 2;

                    v_s1 = Avx2.Add(v_s1, Avx2.Shuffle(v_s1, S2301));
                    v_s1 = Avx2.Add(v_s1, Avx2.Shuffle(v_s1, S1032));
                    s1 += Avx2.ConvertToUInt32(v_s1);
                    s1 += v_s1.GetElement(4);
                    v_s2 = Avx2.Add(v_s2, Avx2.Shuffle(v_s2, S2301));
                    v_s2 = Avx2.Add(v_s2, Avx2.Shuffle(v_s2, S1032));
                    s2 = Avx2.ConvertToUInt32(v_s2);
                    s2 += v_s2.GetElement(4);

                    s1 %= MOD32;
                    s2 %= MOD32;
                }

                if (len > 0)
                {
                    if (len >= 16)
                    {
                        s2 += (s1 += *buf++);
                        s2 += (s1 += *buf++);
                        s2 += (s1 += *buf++);
                        s2 += (s1 += *buf++);
                        s2 += (s1 += *buf++);
                        s2 += (s1 += *buf++);
                        s2 += (s1 += *buf++);
                        s2 += (s1 += *buf++);
                        s2 += (s1 += *buf++);
                        s2 += (s1 += *buf++);
                        s2 += (s1 += *buf++);
                        s2 += (s1 += *buf++);
                        s2 += (s1 += *buf++);
                        s2 += (s1 += *buf++);
                        s2 += (s1 += *buf++);
                        s2 += (s1 += *buf++);
                        len -= 16;
                    }

                    while (len-- > 0)
                    {
                        s2 += (s1 += *buf++);
                    }
                    if (s1 >= MOD32)
                    {
                        s1 -= MOD32;
                    }

                    s2 %= MOD32;
                }

                return s1 | (s2 << 16);
            }
        }
        
        internal static unsafe uint GetAvx(ReadOnlySpan<byte> buffer, uint s1, uint s2)
        {
            uint len = (uint)buffer.Length;

            uint blocks = len / BLOCK_SIZE_64;
            len = len - blocks * BLOCK_SIZE_64;
            
            Vector256<sbyte> tap1 = Vector256.Create(32, 31, 30, 29, 28, 27, 26, 25, 24, 23, 22, 21, 20, 19, 18, 17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1);
            Vector256<byte> zero = Vector256<byte>.Zero;
            Vector256<short> ones = Vector256.Create(1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1);

            fixed (byte* bufPtr = &MemoryMarshal.GetReference(buffer))
            {
                var buf = bufPtr;

                while (blocks != 0)
                {
                    uint n = NMAX32 / BLOCK_SIZE_64;
                    if (n > blocks)
                    {
                        n = blocks;
                    }

                    blocks -= n;

                    // Process n blocks of data. At most NMAX data bytes can be
                    // processed before s2 must be reduced modulo BASE.
                    Vector256<uint> v_ps = Vector256.Create(0u, 0, 0, 0, 0, 0, 0, s1 * n);
                    Vector256<uint> v_s2 = Vector256.Create(0u, 0, 0, 0, 0, 0, 0, s2);
                    Vector256<uint> v_s1 = Vector256.Create(0u, 0, 0, 0, 0, 0, 0, 0);

                    do
                    {
                        // Load 32 input bytes.
                        Vector256<byte> bytes1 = Avx.LoadVector256(&buf[0]);
                        Vector256<byte> bytes2 = Avx.LoadVector256(&buf[32]);


                        // Add previous block byte sum to v_ps.
                        v_ps = Avx2.Add(v_ps, v_s1);



                        // Horizontally add the bytes for s1, multiply-adds the
                        // bytes by [ 32, 31, 30, ... ] for s2.
                        Vector256<ushort> sad1 = Avx2.SumAbsoluteDifferences(bytes1, zero);
                        v_s1 = Avx2.Add(v_s1, sad1.AsUInt32());
                        Vector256<short> mad11 = Avx2.MultiplyAddAdjacent(bytes1, tap1);
                        Vector256<int> mad12 = Avx2.MultiplyAddAdjacent(mad11, ones);
                        v_s2 = Avx2.Add(v_s2, mad12.AsUInt32());

                        // Add previous block byte sum to v_ps.
                        v_ps = Avx2.Add(v_ps, v_s1);



                        // Horizontally add the bytes for s1, multiply-adds the
                        // bytes by [ 32, 31, 30, ... ] for s2.
                        Vector256<ushort> sad2 = Avx2.SumAbsoluteDifferences(bytes2, zero);
                        v_s1 = Avx2.Add(v_s1, sad2.AsUInt32());
                        Vector256<short> mad21 = Avx2.MultiplyAddAdjacent(bytes2, tap1);
                        Vector256<int> mad22 = Avx2.MultiplyAddAdjacent(mad21, ones);
                        v_s2 = Avx2.Add(v_s2, mad22.AsUInt32());

                        buf += BLOCK_SIZE_64;

                        n--;
                    } while (n != 0);

                    var shift = Avx2.ShiftLeftLogical(v_ps, 5);
                    v_s2 = Avx2.Add(v_s2, shift);


                    // Sum epi32 ints v_s1(s2) and accumulate in s1(s2).

                    // A B C D -> B A D C
                    const int S2301 = 2 << 6 | 3 << 4 | 0 << 2 | 1;
                    // A B C D -> C D A B
                    const int S1032 = 1 << 6 | 0 << 4 | 3 << 2 | 2;

                    v_s1 = Avx2.Add(v_s1, Avx2.Shuffle(v_s1, S2301));
                    v_s1 = Avx2.Add(v_s1, Avx2.Shuffle(v_s1, S1032));
                    s1 += Avx2.ConvertToUInt32(v_s1);
                    s1 += v_s1.GetElement(4);
                    v_s2 = Avx2.Add(v_s2, Avx2.Shuffle(v_s2, S2301));
                    v_s2 = Avx2.Add(v_s2, Avx2.Shuffle(v_s2, S1032));
                    s2 = Avx2.ConvertToUInt32(v_s2);
                    s2 += v_s2.GetElement(4);

                    s1 %= MOD32;
                    s2 %= MOD32;
                }

                if (len > 0)
                {
                    if (len >= 16)
                    {
                        s2 += (s1 += *buf++);
                        s2 += (s1 += *buf++);
                        s2 += (s1 += *buf++);
                        s2 += (s1 += *buf++);
                        s2 += (s1 += *buf++);
                        s2 += (s1 += *buf++);
                        s2 += (s1 += *buf++);
                        s2 += (s1 += *buf++);
                        s2 += (s1 += *buf++);
                        s2 += (s1 += *buf++);
                        s2 += (s1 += *buf++);
                        s2 += (s1 += *buf++);
                        s2 += (s1 += *buf++);
                        s2 += (s1 += *buf++);
                        s2 += (s1 += *buf++);
                        s2 += (s1 += *buf++);
                        len -= 16;
                    }

                    while (len-- > 0)
                    {
                        s2 += (s1 += *buf++);
                    }
                    if (s1 >= MOD32)
                    {
                        s1 -= MOD32;
                    }

                    s2 %= MOD32;
                }

                return s1 | (s2 << 16);
            }
        }
    }
}
