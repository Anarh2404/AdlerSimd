using System;

namespace AdlerHash;

public static partial class Adler32
{
    internal static uint GetSimple(ReadOnlySpan<byte> buffer, uint s1, uint s2)
    {
        foreach (var n in buffer)
        {
            s1 = (s1 + n) % MOD32;
            s2 = (s2 + s1) % MOD32;
        }

        return (s2 << 16) | s1;
    }
}