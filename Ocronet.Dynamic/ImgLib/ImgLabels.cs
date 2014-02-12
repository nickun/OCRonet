using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Ocronet.Dynamic.ImgLib
{
    public static class ImgLabels
    {
        public static int[] colors = {
            0xff00ff,
            0x009f4f,
            0xff0000,
            0x0000ff,
            0xff0000,
            0xffff00,
            0x6f007f,
            0x00ff00,
            0x004f9f,
            0x7f9f00,
            0xaf006f,
            0x00ffff,
            0xff5f00,
        };

        /// <summary>
        /// Propagate labels across the entire image from a set of non-zero seeds.
        /// </summary>
        public static void propagate_labels(ref Intarray image)
        {
            Floatarray dist = new Floatarray();
            Narray<Point> source = new Narray<Point>();
            dist.Copy(image);
            BrushFire.brushfire_2(ref dist, ref source, 1000000);
            for (int i = 0; i < dist.Length1d(); i++)
            {
                Point p = source.At1d(i);
                if (image.At1d(i) == 0) image.Put1d(i, image[p.X, p.Y]);
            }
        }

        public static void propagate_labels_to(ref Intarray target, Intarray seed)
        {
            Floatarray dist = new Floatarray();
            Narray<Point> source = new Narray<Point>();
            dist.Copy(seed);
            BrushFire.brushfire_2(ref dist, ref source, 1000000);
            for (int i = 0; i < dist.Length1d(); i++)
            {
                Point p = source.At1d(i);
                if (target.At1d(i) > 0) target.Put1d(i, seed[p.X, p.Y]);
            }
        }

        public static bool dontcare(int x)
        {
            return (x & 0xffffff) == 0xffffff;
        }

        public static void remove_dontcares(ref Intarray image)
        {
            Floatarray dist = new Floatarray();
            Narray<Point> source = new Narray<Point>();
            dist.Resize(image.Dim(0), image.Dim(1));
            for (int i = 0; i < dist.Length1d(); i++)
                if (!dontcare(image.At1d(i))) dist.Put1d(i, (image.At1d(i) > 0 ? 1 : 0));
            BrushFire.brushfire_2(ref dist, ref source, 1000000);
            for (int i = 0; i < dist.Length1d(); i++)
            {
                Point p = source.At1d(i);
                if (dontcare(image.At1d(i))) image.Put1d(i, image[p.X, p.Y]);
            }
        }

        /// <summary>
        /// Renumber the non-zero pixels in an image to start with pixel value start.
        /// The numerical order of pixels is preserved.
        /// </summary>
        public static int renumber_labels(Intarray image, int start=1)
        {
            //SortedList<int, int> translation = new SortedList<int, int>(256);
            Dictionary<int, int> translation = new Dictionary<int, int>(256);
            int n = start;
            for(int i=0; i<image.Length1d(); i++) {
                int pixel = image.At1d(i);
                if(pixel==0 || pixel==0xffffff) continue;
                if (!translation.ContainsKey(pixel))
                {
                    translation.Add(pixel, n);
                    n++;
                }
            }
            n = start;
            int[] keys = translation.Keys.ToArray();
            foreach (int key in keys)
                translation[key] = n++;
            for(int i=0;i<image.Length1d();i++)
            {
                int pixel = image.At1d(i);
                if(pixel==0 || pixel==0xffffff) continue;
                image.Put1d(i, translation[pixel]);
            }
            return n;
        }

        /// <summary>
        /// Label the connected components of an image.
        /// </summary>
        public static int label_components(ref Intarray image, bool four_connected = false)
        {
            int w = image.Dim(0), h = image.Dim(1);
            // We slice the image into columns and call make_set()
            // for every continuous segment within each column.
            // Maximal number of segments per column is (h + 1) / 2.
            // We do it `w' times, so it's w * (h + 1) / 2.
            // We also need to add 1 because index 0 is not used, but counted.
            UnionFind uf = new UnionFind(w * (h + 1) / 2 + 1);
            uf.make_set(0);
            int top = 1;
            for(int i=0; i<image.Length1d(); i++) image.Put1d(i, (image.At1d(i) > 0 ? 1 : 0));
            //for(int i=0;i<w;i++) {image(i,0) = 0; image(i,h-1) = 0;}
            //for(int j=0;j<h;j++) {image(0,j) = 0; image(w-1,j) = 0;}
            for(int i=0; i<w; i++) {
                int current_label = 0;
                for(int j=0; j<h; j++) {
                    int pixel = image[i,j];
                    int range = four_connected ? 0 : 1;
                    for(int delta=-range; delta<=range; delta++) {
                        int adj_label = NarrayUtil.Bat(image, i-1, j+delta, 0);
                        if(pixel == 0) {
                            current_label = 0;
                            continue;
                        }
                        if(current_label == 0) {
                            current_label = top;
                            uf.make_set(top);
                            top++;
                        }
                        if(adj_label > 0) {
                            current_label = uf.find_set(current_label);
                            adj_label = uf.find_set(adj_label);
                            if(current_label != adj_label) {
                                uf.make_union(current_label, adj_label);
                                current_label = uf.find_set(current_label);
                                adj_label = uf.find_set(adj_label);
                            }
                        }
                        image[i,j] = current_label;
                    }
                }
            }
            for(int i=0;i<image.Length1d();i++) {
                if(image.At1d(i) == 0) continue;
                image.Put1d(i, uf.find_set(image.At1d(i)));
            }
            return renumber_labels(image, 1);
        }


        /// <summary>
        /// Compute the bounding boxes for the pixels in the image.
        /// </summary>
        public static void bounding_boxes(ref Narray<Rect> result, Intarray image)
        {
            result.Clear();
            int n = NarrayUtil.Max(image);
            if (n < 1) return;
            result.Resize(n + 1);
            result.Fill(Rect.CreateEmpty());
            for (int i = 0; i < image.Dim(0); i++)
                for (int j = 0; j < image.Dim(1); j++)
                {
                    int value = image[i, j];
                    Rect r = result[value];
                    r.Include(i, j);
                    result[value] = r;
                    //original: result(value).include(i, j);
                }
        }

        public static int interesting_colors(int x)
        {
            int r = 0;
            int g = 0;
            int b = 0;
            for (int i = 0; i < 8; i++)
            {
                r = (r << 1) | (x & 1); x >>= 1;
                g = (g << 1) | (x & 1); x >>= 1;
                b = (b << 1) | (x & 1); x >>= 1;
            }
            return (r << 16) | (g << 8) | b;
        }

        public static void simple_recolor(Intarray image)
        {
            /*for (int i = 0; i < image.Length1d(); i++)
            {
                if (image.At1d(i) == 0) continue;
                image.At1d(i) = enumerator[image.at1d(i)];
            }*/
            for (int i = 0; i < image.Length1d(); i++)
            {
                int value = image.At1d(i);
                if (value == 0 || value == 0xffffff) continue;
                image.Put1d(i, interesting_colors(1 + value % 19));
            }
        }
    }
}
