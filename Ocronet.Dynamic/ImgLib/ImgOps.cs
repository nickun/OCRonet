using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ocronet.Dynamic.ImgLib
{
    public static class ImgOps
    {
        public static void pad_by<T>(ref Narray<T> image, int px, int py, T value = default(T))
        {
            if (px == 0 && py == 0)
                return;
            int w = image.Dim(0), h = image.Dim(1);
            Narray<T> temp = new Narray<T>(w + 2 * px, h + 2 * py);
            temp.Fill(value);
            for (int i = 0; i < image.Dim(0); i++)
            {
                for (int j = 0; j < image.Dim(1); j++)
                {
                    unchecked
                    {
                        if ((uint)(i + px) >= (uint)(temp.Dim(0)) || (uint)(j + py) >= (uint)(temp.Dim(1)))
                            continue;
                    }
                    temp[i + px, j + py] = image[i, j];
                }
            }
            image.Move(temp);
        }

        public static void extract_subimage<T, S>(Narray<T> subimage, Narray<S> image, int x0, int y0, int x1, int y1)
        {
            x0 = Math.Max(x0, 0);
            y0 = Math.Max(y0, 0);
            x1 = Math.Min(x1, image.Dim(0));
            y1 = Math.Min(y1, image.Dim(1));
            int w = x1-x0;
            int h = y1-y0;
            subimage.Resize(w,h);
            for (int i=0; i<w; i++)
                for (int j=0; j<h; j++)
                    subimage[i, j] = (T)Convert.ChangeType(image[x0 + i, y0 + j], typeof(T));
        }

        public static void getd0<T, S>(Narray<T> image, Narray<S> slice, int index)
        {
            slice.Resize(image.Dim(1));
            for (int i = 0; i < image.Dim(1); i++)
                slice.UnsafePut(i, (S)Convert.ChangeType(image.UnsafeAt(index, i), typeof(S)));
        }
        public static void getd1<T, S>(Narray<T> image, Narray<S> slice, int index)
        {
            slice.Resize(image.Dim(0));
            for (int i = 0; i < image.Dim(0); i++)
                slice.UnsafePut(i, (S)Convert.ChangeType(image.UnsafeAt(i, index), typeof(S)));
        }
        public static void getd0(Floatarray image, Floatarray slice, int index)
        {
            slice.Resize(image.Dim(1));
            for (int i = 0; i < image.Dim(1); i++)
                slice.UnsafePut(i, image.UnsafeAt(index, i));
        }
        public static void getd1(Floatarray image, Floatarray slice, int index)
        {
            slice.Resize(image.Dim(0));
            for (int i = 0; i < image.Dim(0); i++)
                slice.UnsafePut(i, image.UnsafeAt(i, index));
        }

        public static void putd0<T, S>(Narray<T> image, Narray<S> slice, int index)
        {
            if (!(slice.Rank() == 1 && slice.Dim(0) == image.Dim(1)))
                throw new Exception("ASSERT: slice.Rank()==1 && slice.Dim(0)==image.Dim(1)");
            for (int i = 0; i < image.Dim(1); i++)
                image.UnsafePut(index, i, (T)Convert.ChangeType(slice.UnsafeAt(i), typeof(T)));
        }
        public static void putd1<T, S>(Narray<T> image, Narray<S> slice, int index)
        {
            if (!(slice.Rank() == 1 && slice.Dim(0) == image.Dim(0)))
                throw new Exception("ASSERT: slice.Rank()==1 && slice.Dim(0)==image.Dim(1)");
            for (int i = 0; i < image.Dim(0); i++)
                image.UnsafePut(i, index, (T)Convert.ChangeType(slice.UnsafeAt(i), typeof(T)));
        }
        public static void putd0(Floatarray image, Floatarray slice, int index)
        {
            if (!(slice.Rank() == 1 && slice.Dim(0) == image.Dim(1)))
                throw new Exception("ASSERT: slice.Rank()==1 && slice.Dim(0)==image.Dim(1)");
            for (int i = 0; i < image.Dim(1); i++)
                image.UnsafePut(index, i, slice.UnsafeAt(i));
        }
        public static void putd1<T, S>(Floatarray image, Floatarray slice, int index)
        {
            if (!(slice.Rank() == 1 && slice.Dim(0) == image.Dim(0)))
                throw new Exception("ASSERT: slice.Rank()==1 && slice.Dim(0)==image.Dim(1)");
            for (int i = 0; i < image.Dim(0); i++)
                image.UnsafePut(i, index, slice.UnsafeAt(i));
        }

        public static T xref<T>(Narray<T> a, int x, int y)
        {
            if (x < 0) x = 0;
            else if (x >= a.Dim(0)) x = a.Dim(0) - 1;
            if (y < 0) y = 0;
            else if (y >= a.Dim(1)) y = a.Dim(1) - 1;
            return a.UnsafeAt(x, y);
        }

        public static float xref(Narray<float> a, int x, int y)
        {
            if (x < 0) x = 0;
            else if (x >= a.Dim(0)) x = a.Dim(0) - 1;
            if (y < 0) y = 0;
            else if (y >= a.Dim(1)) y = a.Dim(1) - 1;
            return a.UnsafeAt(x, y);
        }

        public static T bilin<T>(Narray<T> a, float x, float y)
        {
            int i = (int)(x);
            int j = (int)(y);
            float l = x - i;
            float m = y - j;
            float s00 = Convert.ToSingle(xref(a, i, j));
            float s01 = Convert.ToSingle(xref(a, i, j + 1));
            float s10 = Convert.ToSingle(xref(a, i + 1, j));
            float s11 = Convert.ToSingle(xref(a, i + 1, j + 1));
            return (T)Convert.ChangeType(((1.0f - l) * ((1.0f - m) * s00 + m * s01) +
                             l * ((1.0f - m) * s10 + m * s11)), typeof(T));
        }

        public static float bilin(Narray<float> a, float x, float y)
        {
            int i = (int)(x);
            int j = (int)(y);
            float l = x - i;
            float m = y - j;
            float s00 = xref(a, i, j);
            float s01 = xref(a, i, j + 1);
            float s10 = xref(a, i + 1, j);
            float s11 = xref(a, i + 1, j + 1);
            return (1.0f - l) * ((1.0f - m) * s00 + m * s01) +
                             l * ((1.0f - m) * s10 + m * s11);
        }

    }
}
