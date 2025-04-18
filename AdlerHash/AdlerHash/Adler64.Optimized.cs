using System;

namespace AdlerHash;

public static partial class Adler64
{
    internal static ulong GetSimpleOptimized(ReadOnlySpan<byte> buf, ulong adler, ulong sum2)
    {
        ulong n;
        ulong len = (ulong)buf.Length;
        if (len == 1)
        {
            adler += buf[0];
            if (adler >= MOD64)
                adler -= MOD64;
            sum2 += adler;
            if (sum2 >= MOD64)
                sum2 -= MOD64;
            return adler | (sum2 << 32);
        }
        var idx = 0;
        if (len < 16)
        {
            while (len-- != 0)
            {
                adler += buf[idx++];
                sum2 += adler;
            }
            if (adler >= MOD64)
                adler -= MOD64;
            sum2 %= MOD64;           /* only added so many BASE's */
            return adler | (sum2 << 32);
        }

        /* do length NMAX blocks -- requires just one modulo operation */

        while (len >= NMAX64)
        {
            len -= NMAX64;
            n = NMAX64 / 16;          /* NMAX is divisible by 16 */
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
            adler %= MOD64;
            sum2 %= MOD64;
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
            adler %= MOD64;
            sum2 %= MOD64;
        }

        /* return recombined sums */
        return adler | (sum2 << 32);
    }
}