using System;
using System.Numerics;
using System.Runtime.Intrinsics.X86;

namespace AdlerHash;

public static partial class Adler64
{
    private const ulong MOD64 = 4294967291;
    private const uint NMAX64 = 363898400;
    private const int BLOCK_SIZE = 32;

    public static ulong Compute(ReadOnlySpan<byte> buffer, ulong adler = 1)
    {
        ulong s1 = adler & 0xffffffff;
        ulong s2 = adler >> 32;
        if (Ssse3.IsSupported)
        {
            return GetSse(buffer, s1, s2);
        }
        if (Vector.IsHardwareAccelerated)
        {
            return GetVectorized(buffer, s1, s2);
        }
        return GetSimpleOptimized(buffer, s1, s2);
    }
}