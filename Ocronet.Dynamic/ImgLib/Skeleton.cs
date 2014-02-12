using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ocronet.Dynamic.ImgLib
{
    public class Skeleton
    {
        public static byte OFF = 0, ON = 1, SKEL = 2, DEL = 3;
        public static byte[] ttable = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0,
                0, 0, /* 00 */
                0, 1, 1, 1, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, /* 10 */
                0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, /* 20 */
                0, 1, 1, 1, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, /* 30 */
                0, 0, 1, 0, 1, 0, 1, 0, 1, 1, 1, 1, 1, 0, 1, 0, /* 40 */
                0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, /* 50 */
                0, 0, 1, 0, 1, 0, 1, 0, 1, 1, 1, 1, 1, 0, 1, 0, /* 60 */
                0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, /* 70 */
                0, 0, 1, 0, 1, 0, 1, 0, 1, 1, 1, 1, 1, 0, 1, 0, /* 80 */
                1, 1, 1, 1, 1, 0, 1, 0, 1, 1, 1, 1, 1, 0, 1, 0, /* 90 */
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, /* a0 */
                1, 1, 1, 1, 1, 0, 1, 0, 1, 1, 1, 1, 1, 0, 1, 0, /* b0 */
                0, 0, 1, 0, 1, 0, 1, 0, 1, 1, 1, 1, 1, 0, 1, 0, /* c0 */
                0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, /* d0 */
                0, 0, 1, 0, 1, 0, 1, 0, 1, 1, 1, 1, 1, 0, 1, 0, /* e0 */
                0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0  /* f0 */
        };

        public static int[] nx= { 1, 1, 0, -1, -1, -1, 0, 1 };
        public static int[] ny= { 0, 1, 1, 1, 0, -1, -1, -1 };

        public static void Thin(ref Bytearray uci)
        {
            int w = uci.Dim(0) - 1;
            int h = uci.Dim(1) - 1;

            for (int i = 0, n = uci.Length1d(); i < n; i++)
            {
                if (uci.At1d(i) > 0)
                    uci.Put1d(i, ON);
                else
                    uci.Put1d(i, OFF);
            }

            bool flag;
            do
            {
                flag = false;
                for (int j = 0; j < 8; j += 2)
                {
                    for (int x = 1; x < w; x++)
                        for (int y = 1; y < h; y++)
                        {
                            if (uci[x, y] != ON)
                                continue;
                            if (uci[x + nx[j], y + ny[j]] != OFF)
                                continue;
                            int b = 0;
                            for (int i = 7; i >= 0; i--)
                            {
                                b <<= 1;
                                b |= (uci[x + nx[i], y + ny[i]] != OFF ? 1 : 0);
                            }
                            if (ttable[b] > 0)
                                uci[x, y] = SKEL;
                            else
                            {
                                uci[x, y] = DEL;
                                flag = true;
                            }
                        }
                    if (!flag)
                        continue;
                    for (int x = 1; x < w; x++)
                        for (int y = 1; y < h; y++)
                            if (uci[x, y] == DEL)
                                uci[x, y] = OFF;
                }
            } while (flag);

            for (int i = 0, n = uci.Length1d(); i < n; i++)
            {
                if (uci.At1d(i) == SKEL)
                    uci.Put1d(i, 255);
                else
                    uci.Put1d(i, 0);
            }
        }
    }
}
