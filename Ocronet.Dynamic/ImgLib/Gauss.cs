using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ocronet.Dynamic.ImgLib
{
    public class Gauss
    {
        public static void Gauss1d<T>(Narray<T> outa, Narray<T> ina, float sigma)
        {
            outa.Resize(ina.Dim(0));
            // make a normalized mask
            int range = 1 + (int)(3.0*sigma);
            Doublearray mask = new Doublearray(2 * range + 1);
            for (int i=0; i<=range; i++) {
                double y = Math.Exp(-i*i/2.0/sigma/sigma);
                mask[range+i] = mask[range-i] = y;
            }
            double total = 0.0;
            for (int i=0; i<mask.Dim(0); i++)
                total += mask[i];
            for (int i=0; i<mask.Dim(0); i++)
                mask[i] /= total;

            // apply it
            int n = ina.Length();
            for (int i=0; i<n; i++) {
                total = 0.0;
                for (int j=0; j<mask.Dim(0); j++) {
                    int index = i+j-range;
                    if (index<0)
                        index = 0;
                    if (index>=n)
                        index = n-1;
                    total += Convert.ToDouble(ina[index]) * mask[j]; // it's symmetric
                }
                outa[i] = (T)Convert.ChangeType(total, typeof(T));
            }
        }

        public static void Gauss1d(Floatarray outa, Floatarray ina, float sigma)
        {
            outa.Resize(ina.Dim(0));
            // make a normalized mask
            int range = 1 + (int)(3.0 * sigma);
            Floatarray mask = new Floatarray(2 * range + 1);
            for (int i = 0; i <= range; i++)
            {
                float y = (float)Math.Exp(-i * i / 2.0 / sigma / sigma);
                mask[range + i] = mask[range - i] = y;
            }
            float total = 0.0f;
            for (int i = 0; i < mask.Dim(0); i++)
                total += mask[i];
            for (int i = 0; i < mask.Dim(0); i++)
                mask[i] /= total;

            // apply it
            int n = ina.Length();
            for (int i = 0; i < n; i++)
            {
                total = 0.0f;
                for (int j = 0; j < mask.Dim(0); j++)
                {
                    int index = i + j - range;
                    if (index < 0)
                        index = 0;
                    if (index >= n)
                        index = n - 1;
                    total += ina[index] * mask[j]; // it's symmetric
                }
                outa[i] = total;
            }
        }

        public static void Gauss2d<T>(Narray<T> a, float sx, float sy)
        {
            Floatarray r = new Floatarray();
            Floatarray s = new Floatarray();
            for (int i = 0; i < a.Dim(0); i++)
            {
                ImgOps.getd0(a, r, i);
                Gauss1d(s, r, sy);
                ImgOps.putd0(a, s, i);
            }

            for (int j = 0; j < a.Dim(1); j++)
            {
                ImgOps.getd1(a, r, j);
                Gauss1d(s, r, sx);
                ImgOps.putd1(a, s, j);
            }
        }

        public static void Gauss2d(Floatarray a, float sx, float sy)
        {
            Floatarray r = new Floatarray();
            Floatarray s = new Floatarray();
            for (int i = 0; i < a.Dim(0); i++)
            {
                ImgOps.getd0(a, r, i);
                Gauss1d(s, r, sy);
                ImgOps.putd0(a, s, i);
            }

            for (int j = 0; j < a.Dim(1); j++)
            {
                ImgOps.getd1(a, r, j);
                Gauss1d(s, r, sx);
                ImgOps.putd1(a, s, j);
            }
        }

    }
}
