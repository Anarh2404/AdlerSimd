using System;
using System.Numerics;
using System.Runtime.Intrinsics.X86;

namespace AdlerHash;

public static partial class Adler32
{
    private const uint MOD32 = 65521U;
    private const uint NMAX32 = 5552;
    private const int BLOCK_SIZE = 32;

    public static uint Compute(ReadOnlySpan<byte> buffer, uint adler = 1)
    {
        uint s1 = adler & 0xffff;
        uint s2 = adler >> 16;
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