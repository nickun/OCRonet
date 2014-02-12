using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ocronet.Dynamic
{
    /// <summary>
    /// Random number generator.
    /// Same as in Lush (http://lush.sourceforge.net/)
    /// </summary>
    public class DRandomizer
    {
        static volatile DRandomizer _instance;
        static int MMASK = 0x7fffffff;
        static int MSEED = 161803398;
        static float FAC = (float)(1.0 / (1.0 + MMASK));
        static float FAC2 = (float)(1.0 / 0x01000000L);
        int inext, inextp;
        int[] ma = new int[56];		/* Should not be modified */ 
        bool drand_ini;
        private static readonly object _syncRoot = new object();

        public static DRandomizer Default
        {
            get
            {
                if (_instance == null)
                {
                    lock (_syncRoot)
                    {
                        if (_instance == null)
                            _instance = new DRandomizer();
                    }
                }
                return _instance;
            }
        }

        public void init_drand(int x)
        {
            drand_ini = true;
            dseed(x);
        }

        public void dseed(int x)
        {
            int mj, mk;
            int i, ii;

            mj = MSEED - (x < 0 ? -x : x);
            mj &= MMASK;
            ma[55] = mj;
            mk = 1;
            for (i = 1; i <= 54; i++)
            {
                ii = (21 * i) % 55;
                ma[ii] = mk;
                mk = (mj - mk) & MMASK;
                mj = ma[ii];
            }
            for (ii = 1; ii <= 4; ii++)
                for (i = 1; i < 55; i++)
                {
                    ma[i] -= ma[1 + (i + 30) % 55];
                    ma[i] &= MMASK;
                }
            inext = 0;
            inextp = 31;			/* Special constant */
        }

        public double drand()
        {
            if (!drand_ini)
                init_drand(0);
            int mj;
            if (++inext == 56)
                inext = 1;
            if (++inextp == 56)
                inextp = 1;
            mj = ((ma[inext] - ma[inextp]) * 84589 + 45989) & MMASK;
            ma[inext] = mj;
            return mj * FAC;
        }

        public int nrand()
        {
            if (!drand_ini)
                init_drand(0);
            int mj;
            if (++inext == 56)
                inext = 1;
            if (++inextp == 56)
                inextp = 1;
            mj = ((ma[inext] - ma[inextp]) * 84589 + 45989) & MMASK;
            ma[inext] = mj;
            return mj;
        }

        public double drand(double vHiLo)
        {
            return vHiLo * 2 * drand() - vHiLo;
        }

        public double drand(double vHi, double vLo)
        {
            return (vLo - vHi) * drand() + vHi;
        }

        /// <summary>
        /// Now a quick and dirty way to build
        /// a quasi-normal random number.
        /// </summary>
        /// <returns></returns>
        public double dgauss()
        {
            int i;
            int mj, sum;
            mj = 0;
            sum = 0;
            for (i = 12; i > 0; i--)
            {
                if (++inext == 56)
                    inext = 1;
                if (++inextp == 56)
                    inextp = 1;
                mj = (ma[inext] - ma[inextp]) & MMASK;
                ma[inext] = mj;
                if ((mj & 0x00800000) > 0)
                    mj =  (int)(mj | 0xff000000);
                else
                    mj &= 0x00ffffff;
                sum += mj;
            }
            ma[inext] = (mj * 84589 + 45989) & MMASK;
            return sum * FAC2;
        }

        public double dgauss(double sigma)
        {
            return sigma * dgauss();
        }

        public double dgauss(double m, double sigma)
        {
            return sigma * dgauss() + m;
        }

    }
}
