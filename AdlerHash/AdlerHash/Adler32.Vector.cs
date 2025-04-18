using System;
using System.Numerics;

namespace AdlerHash;

public static partial class Adler32
{
    private const int NMAX = 5552; //  избегаем переполнения B (как в zlib)

    /// <summary>Векторизованная Adler‑32.</summary>
    internal static uint GetVectorized(ReadOnlySpan<byte> data, uint s1, uint s2)
    {
        // маленькие буферы или отсутствие SIMD → скалярно
        if (data.Length < Vector<byte>.Count * 2)
            return GetSimpleOptimized(data, 1, 0);

        int vecBytes = Vector<byte>.Count; // 16 (SSE2) / 32 (AVX2) / 64 (AVX‑512 / NEON)
        int lanesU32 = Vector<uint>.Count; //  4 / 8 / 16
        int groups = vecBytes / lanesU32; // всегда 4

        // --- шаговые приращения 0…vecBytes‑1 (не меняются между вызовами) ---
        var inc = new Vector<uint>[groups];
        for (int g = 0; g < groups; g++)
        {
            uint[] tmp = new uint[lanesU32];
            for (int i = 0; i < lanesU32; i++) tmp[i] = (uint)(g * lanesU32 + i);
            inc[g] = new Vector<uint>(tmp);
        }
            
        int offset = 0, remaining = data.Length;

        while (remaining > 0)
        {
            int blockLen = Math.Min(remaining, NMAX); // работаем блоками ≤ 5 552 байт
            int bytesLeft = blockLen;

            uint aPrev = s1; // A в начале блока
            Vector<uint> vecSumA = Vector<uint>.Zero; // ∑ bytes
            Vector<uint> vecSumW = Vector<uint>.Zero; // ∑ bytes × weight

            // ───── SIMD‑часть ───────────────────────────────────────────────
            while (bytesLeft >= vecBytes)
            {
                // 1) читаем vecBytes байт
                var vb = new Vector<byte>(data.Slice(offset, vecBytes));

                // 2) byte → ushort → uint
                Vector<ushort> v16a, v16b;
                Vector.Widen(vb, out v16a, out v16b);

                Vector<uint> u0, u1, u2, u3;
                Vector.Widen(v16a, out u0, out u1);
                Vector.Widen(v16b, out u2, out u3);

                // 3) ∑ bytes
                vecSumA += u0 + u1 + u2 + u3;

                // 4) ∑ bytes × (n‑i+1)
                var baseVec = new Vector<uint>((uint)bytesLeft); // n‑i базируется на оставшихся байтах
                vecSumW += u0 * (baseVec - inc[0])
                           + u1 * (baseVec - inc[1])
                           + u2 * (baseVec - inc[2])
                           + u3 * (baseVec - inc[3]);

                offset += vecBytes;
                bytesLeft -= vecBytes;
            }

            // ───── хвост блока (< vecBytes) ────────────────────────────────
            uint tailSum = 0, tailWeighted = 0;
            for (int i = 0; i < bytesLeft; i++)
            {
                byte t = data[offset + i];
                tailSum += t;
                tailWeighted += (uint)(bytesLeft - i) * t; // веса: r…1
            }

            offset += bytesLeft;

            // ───── сворачиваем в скаляры и завершаем блок ──────────────────
            uint blockSum = HorizontalAdd(vecSumA) + tailSum;
            uint weighted = HorizontalAdd(vecSumW) + tailWeighted;

            s1 += blockSum; // A = A_prev + ∑ bytes
            s2 += (uint)blockLen * aPrev + weighted; // B = B_prev + n·A_prev + ∑(w·d)

            s1 %= MOD32;
            s2 %= MOD32;

            remaining -= blockLen;
        }

        return (s2 << 16) | s1;
    }

    // ────────────────────────────────────────────────────────────────────────────────
    private static uint HorizontalAdd(in Vector<uint> v)
    {
        uint s = 0;
        for (int i = 0; i < Vector<uint>.Count; i++) s += v[i];
        return s;
    }
}