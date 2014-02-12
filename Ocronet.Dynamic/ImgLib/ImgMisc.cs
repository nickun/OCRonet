using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Ocronet.Dynamic.ImgLib
{
    public static class ImgMisc
    {
        public static void Crop<T>(Narray<T> a, Rect box)
        {
            Narray<T> t = new Narray<T>();
            ImgOps.extract_subimage(t, a, box.x0, box.y0, box.x1, box.y1);
            a.Move(t);
        }

        public static void Crop<T>(Narray<T> result, Narray<T> source, Rect r)
        {
            Crop<T>(result, source, r.x0, r.y0, r.x1 - r.x0, r.y1 - r.y0);
        }

        public static void Crop<T>(Narray<T> result, Narray<T> source,
            int x, int y, int w, int h)
        {
            result.Resize(w, h);
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                    result[i, j] = source[x + i, y + j];
            }
        }

        public static void Transpose<T>(Narray<T> a)
        {
            Narray<T> t = new Narray<T>();
            t.Resize(a.Dim(1), a.Dim(0));
            for (int x = 0; x < a.Dim(0); x++)
            {
                for (int y = 0; y < a.Dim(1); y++)
                    t[y, x] = a[x, y];
            }
            a.Move(t);
        }

        public static void replace_values<T>(Narray<T> a, T from, T to)
        {
            for (int i = 0; i < a.Length1d(); i++)
            {
                if (a.At1d(i).Equals(from))
                    a.Put1d(i, to);
            }
        }

        /// <summary>
        /// Remove singular points over image.
        /// uses in skeleton segmenter
        /// </summary>
        public static void remove_singular_points(ref Bytearray image, int d)
        {
            for (int i = d; i < image.Dim(0) - d - 1; i++)
            {
                for (int j = d; j < image.Dim(1) - d - 1; j++)
                {
                    if (is_singular(image, i, j))
                    {
                        for (int k = -d; k <= d; k++)
                            for (int l = -d; l <= d; l++)
                                image[i + k, j + l] = 0;
                    }
                }
            }
        }

        public static bool is_singular(Bytearray image, int i, int j)
        {
            return neighbors(image, i, j) > 2;
        }

        public static int neighbors(Bytearray image, int i, int j)
        {
            if (i < 1 || i >= image.Dim(0) - 1 || j < 1 || j > image.Dim(1) - 1) return 0;
            if (image[i, j] == 0) return 0;
            int count = -1;
            for (int k = -1; k <= 1; k++)
                for (int l = -1; l <= 1; l++)
                    if (image[i + k, j + l] > 0) count++;
            return count;
        }
    }
}
