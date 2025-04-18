using System;
using System.Numerics;

namespace AdlerHash;

public static partial class Adler64
{
    private const int   NMAX      = 5_552;           // zlib‑совместимый лимит

    public static ulong GetVectorized(ReadOnlySpan<byte> data, ulong s1, ulong s2)
    {
        // маленькие буферы или нет SIMD → скалярно
        if (data.Length < Vector<byte>.Count * 2)
            return GetSimpleOptimized(data, 1, 0);

        int vecBytes = Vector<byte>.Count;           // 16 / 32 / 64
        int lanesU32 = Vector<uint>.Count;           // 4  / 8  / 16
        int groups   = vecBytes / lanesU32;          // всегда 4

        // константа 0,1,2,… (uint) для вычисления весов
        var inc = new Vector<uint>[groups];
        for (int g = 0; g < groups; g++)
        {
            uint[] tmp = new uint[lanesU32];
            for (int i = 0; i < lanesU32; i++) tmp[i] = (uint)(g * lanesU32 + i);
            inc[g] = new Vector<uint>(tmp);
        }
        
        int   off = 0, left = data.Length;

        while (left > 0)
        {
            int blkLen    = Math.Min(left, NMAX);
            int bytesLeft = blkLen;
            ulong aPrev   = s1;

            Vector<uint> vSum = Vector<uint>.Zero;   // ∑ bytes  (uint в SIMD)
            Vector<uint> vWgt = Vector<uint>.Zero;   // ∑ bytes·w

            // ───── SIMD‑ядро ───────────────────────────────────────────────
            while (bytesLeft >= vecBytes)
            {
                var vb = new Vector<byte>(data.Slice(off, vecBytes));

                Vector<ushort> v16a, v16b;
                Vector.Widen(vb, out v16a, out v16b);

                Vector<uint> u0, u1, u2, u3;
                Vector.Widen(v16a, out u0, out u1);
                Vector.Widen(v16b, out u2, out u3);

                vSum += u0 + u1 + u2 + u3;

                var baseVec = new Vector<uint>((uint)bytesLeft);
                vWgt += u0 * (baseVec - inc[0])
                      + u1 * (baseVec - inc[1])
                      + u2 * (baseVec - inc[2])
                      + u3 * (baseVec - inc[3]);

                off       += vecBytes;
                bytesLeft -= vecBytes;
            }

            // ───── хвост блока (< vecBytes) ───────────────────────────────
            ulong tailSum = 0, tailWgt = 0;
            for (int i = 0; i < bytesLeft; i++)
            {
                byte t = data[off + i];
                tailSum += t;
                tailWgt += (ulong)(bytesLeft - i) * t;
            }
            off += bytesLeft;

            ulong blockSum = Horizontal(vSum) + tailSum;
            ulong weighted = Horizontal(vWgt) + tailWgt;

            s1 += blockSum;
            s2 += (ulong)blkLen * aPrev + weighted;

            s1 %= MOD64;
            s2 %= MOD64;

            left -= blkLen;
        }

        return (s2 << 32) | s1;               // B старшие, A младшие 32 бита
    }

    // ───────────────── helpers ────────────────────────────────────────────
    private static ulong Horizontal(in Vector<uint> v)
    {
        ulong s = 0;
        for (int i = 0; i < Vector<uint>.Count; i++) s += v[i];
        return s;
    }
}