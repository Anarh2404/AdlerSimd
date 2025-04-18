using System;

namespace AdlerHash;

public static partial class Adler32
{
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
        uint tmp =  a >> 16;
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
}