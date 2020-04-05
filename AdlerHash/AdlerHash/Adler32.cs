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

        public static uint GetAdler32(ReadOnlySpan<byte> buffer, uint adler = 1)
        {
            uint s1 = adler & 0xffff;
            uint s2 = adler >> 16;
            if (Ssse3.IsSupported)
            {
                return GetAdler32Sse(buffer, s1, s2);
            }
            return GetAdler32Simple(buffer, s1, s2);
        }

        internal static uint GetAdler32Simple(ReadOnlySpan<byte> buffer, uint s1, uint s2)
        {
            foreach (var n in buffer)
            {
                s1 = (s1 + n) % MOD32;
                s2 = (s2 + s1) % MOD32;
            }

            return (s2 << 16) | s1;
        }

        internal static unsafe uint GetAdler32Sse(ReadOnlySpan<byte> buffer, uint s1, uint s2)
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
                    uint n = NMAX32;
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

                        Vector128<short> mad1 = Ssse3.MultiplyAddAdjacent(bytes1, tap1);
                        Vector128<int> mad12 = Sse2.MultiplyAddAdjacent(mad1, ones);
                        v_s2 = Sse2.Add(v_s2, mad12.AsUInt32());



                        Vector128<ushort> sad2 = Sse2.SumAbsoluteDifferences(bytes2, zero);
                        v_s1 = Sse2.Add(v_s1, sad2.AsUInt32());

                        Vector128<short> mad2 = Ssse3.MultiplyAddAdjacent(bytes2, tap2);
                        Vector128<int> mad22 = Sse2.MultiplyAddAdjacent(mad2, ones);
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
    }
}
