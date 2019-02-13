using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ocronet.Dynamic.Recognizers.Lenet;

namespace Ocronet.Dynamic.Debug
{
    public static class NarrayShow
    {

        public static void ShowConsole(Narray<byte> a)
        {
            for (int y = a.Dim(1)-1; y >= 0; y--)
            {
                for (int x = 0; x < a.Dim(0); x++)
                    Console.Write("{0,-3}", a[x, y]);
                Console.WriteLine();
            }
        }

        public static void ShowConsole(Narray<float> a)
        {
            for (int y = a.Dim(1) - 1; y >= 0; y--)
            {
                for (int x = 0; x < a.Dim(0); x++)
                    Console.Write("{0}", Convert.ToInt32(a[x, y]));
                Console.WriteLine();
            }
        }

        public static void ShowConsole(StdInput a)
        {
            for (int y = 0; y < a.Height; y++)
            {
                for (int x = 0; x < a.Width; x++)
                    Console.Write("{0,-3}", a[y, x]);
                Console.WriteLine();
            }
        }
    }
}
