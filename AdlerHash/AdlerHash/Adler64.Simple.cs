using System;

namespace AdlerHash;

public static partial class Adler64
{
    internal static ulong GetSimple(ReadOnlySpan<byte> buffer, ulong s1, ulong s2)
    {
        foreach (var n in buffer)
        {
            s1 = (s1 + n) % MOD64;
            s2 = (s2 + s1) % MOD64;
        }

        return (s2 << 32) | s1;
    }
}