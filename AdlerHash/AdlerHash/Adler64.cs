using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text;

namespace AdlerHash
{
    public static class Adler64
    {
        private const ulong MOD64 = 4294967291;
        private const uint NMAX64 = 363898415;
        private const int BLOCK_SIZE = 32;

        public static ulong GetAdler64(ReadOnlySpan<byte> buffer, ulong adler = 1)
        {
            ulong s1 = adler & 0xffff;
            ulong s2 = adler >> 32;
            if (Ssse3.IsSupported)
            {
                return GetAdler64Sse(buffer, s1, s2);
            }
            return GetAdler64Simple(buffer, s1, s2);
        }

        internal static ulong GetAdler64Simple(ReadOnlySpan<byte> buffer, ulong s1, ulong s2)
        {
            foreach (var n in buffer)
            {
                s1 = (s1 + n) % MOD64;
                s2 = (s2 + s1) % MOD64;
            }

            return (s2 << 32) | s1;
        }

        internal unsafe static ulong GetAdler64Sse(ReadOnlySpan<byte> buffer, ulong s1, ulong s2)
        {
            uint len = (uint)buffer.Length;

            uint blocks = len / BLOCK_SIZE;
            len = len - blocks * BLOCK_SIZE;

            Vector128<sbyte> tap1 = Vector128.Create(32, 31, 30, 29, 28, 27, 26, 25, 24, 23, 22, 21, 20, 19, 18, 17);
            Vector128<sbyte> tap2 = Vector128.Create(16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1);
            Vector128<byte> zero = Vector128<byte>.Zero;
            Vector128<short> onesShort = Vector128.Create(1, 1, 1, 1, 1, 1, 1, 1);
            Vector128<int> onesInt = Vector128.Create(1, 1, 1, 1);
            Vector128<byte> shuffleMask2301 = Vector128.Create((byte)4, 5, 6, 7, 0, 1, 2, 3, 12, 13, 14, 15, 8, 9, 10, 11);
            Vector128<byte> shuffleMask1032 = Vector128.Create((byte)8u, 9, 10, 11, 12, 13, 14, 15, 0, 1, 2, 3, 4, 5, 6, 7);
            Vector128<byte> shuffleMaskTrim = Vector128.Create(0, 1, 2, 3, 255, 255, 255, 255, 8, 9, 10, 11, 255, 255, 255, 255);
            // A B C D -> B A D C
            const int S2301 = 2 << 6 | 3 << 4 | 0 << 2 | 1;


            fixed (byte* bufPtr = &MemoryMarshal.GetReference(buffer))
            {
                var buf = bufPtr;

                while (blocks != 0)
                {
                    uint n = NMAX64;
                    if (n > blocks)
                    {
                        n = blocks;
                    }

                    blocks -= n;

                    // Process n blocks of data. At most NMAX data bytes can be
                    // processed before s2 must be reduced modulo BASE.
                    Vector128<ulong> v_ps = Vector128.Create(0, s1 * n);
                    Vector128<ulong> v_s2 = Vector128.Create(0, s2);
                    Vector128<ulong> v_s1 = Vector128.Create(0ul, 0);

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
                        v_s1 = Sse2.Add(v_s1, sad1.AsUInt64());

                        Vector128<short> mad1 = Ssse3.MultiplyAddAdjacent(bytes1, tap1);
                        Vector128<int> mad12 = Sse2.MultiplyAddAdjacent(mad1, onesShort);
                        var mad121 = Sse2.Add(mad12, Sse2.Shuffle(mad12, S2301));
                        var madTrimmed1 = Ssse3.Shuffle(mad121.AsByte(), shuffleMaskTrim);
                        var madTimmed1ULong = madTrimmed1.AsUInt64();
                        v_s2 = Sse2.Add(v_s2, madTimmed1ULong);



                        Vector128<ushort> sad2 = Sse2.SumAbsoluteDifferences(bytes2, zero);
                        v_s1 = Sse2.Add(v_s1, sad2.AsUInt64());

                        Vector128<short> mad2 = Ssse3.MultiplyAddAdjacent(bytes2, tap2);
                        Vector128<int> mad22 = Sse2.MultiplyAddAdjacent(mad2, onesShort);
                        var mad221 = Sse2.Add(mad22, Sse2.Shuffle(mad22, S2301));
                        var madTrimmed2 = Ssse3.Shuffle(mad221.AsByte(), shuffleMaskTrim);
                        var madTimmed2ULong = madTrimmed2.AsUInt64();
                        v_s2 = Sse2.Add(v_s2, madTimmed2ULong);


                        buf += BLOCK_SIZE;

                        n--;
                    } while (n != 0);


                    var shifted = Sse2.ShiftLeftLogical(v_ps, 5);
                    v_s2 = Sse2.Add(v_s2, shifted);

                    s1 += v_s1.GetElement(0);
                    s1 += v_s1.GetElement(1);

                    s2 += v_s2.GetElement(0);
                    s2 += v_s2.GetElement(1);

                    s1 %= MOD64;
                    s2 %= MOD64;
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
                    if (s1 >= MOD64)
                    {
                        s1 -= MOD64;
                    }

                    s2 %= MOD64;
                }

                return s1 | (s2 << 32);
            }
        }
    }
}
