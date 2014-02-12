using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Ocronet.Dynamic.ImgLib
{
    public class BrushFire
    {
        /// <summary>
        /// SGI compiler bug: can't make this a template function with
        /// an unused last argument for the template parameter
        /// </summary>
        public static void Go(Metric m, ref Floatarray distance, ref Narray<Point> source, float maxdist)
        {
            const float BIG = 1e38f;

            int w = distance.Dim(0);
            int h = distance.Dim(1);
            distance.Resize(w,h);
            source.Resize(w,h);

            Queue<Point> queue = new Queue<Point>(w*h);

            int i, j;
            for(i = 0; i < w; i++) for(j = 0; j < h; j++) {
                if(distance.At(i, j) > 0) {
                    queue.Enqueue(new Point(i, j));
                    distance[i, j] = 0;
                    source[i, j] = new Point(i, j);
                } else {
                    distance[i, j] = BIG;
                    source[i, j] = new Point(-1, -1);
                }
            }

            while(queue.Count != 0) {
                Point q = queue.Dequeue();
                float d = m.metric(new Point(q.X - 1, q.Y), source.At(q.X, q.Y));
                if(d <= maxdist && q.X > 0 && d < distance.At(q.X - 1, q.Y)) {
                    queue.Enqueue(new Point(q.X - 1, q.Y));
                    source[q.X - 1, q.Y] = source.At(q.X, q.Y);
                    distance[q.X - 1, q.Y] = d;
                }
                d = m.metric(new Point(q.X, q.Y - 1), source.At(q.X, q.Y));
                if(d <= maxdist && q.Y > 0 && d < distance.At(q.X, q.Y - 1)) {
                    queue.Enqueue(new Point(q.X, q.Y - 1));
                    source[q.X, q.Y - 1] = source.At(q.X, q.Y);
                    distance[q.X, q.Y - 1] = d;
                }
                d = m.metric(new Point(q.X + 1, q.Y), source.At(q.X, q.Y));
                if(d <= maxdist && q.X < w - 1 && d < distance.At(q.X + 1, q.Y)) {
                    queue.Enqueue(new Point(q.X + 1, q.Y));
                    source[q.X + 1, q.Y] = source.At(q.X, q.Y);
                    distance[q.X + 1, q.Y] = d;
                }
                d = m.metric(new Point(q.X, q.Y + 1), source.At(q.X, q.Y));
                if(d <= maxdist && q.Y < h - 1 && d < distance.At(q.X, q.Y + 1)) {
                    queue.Enqueue(new Point(q.X, q.Y + 1));
                    source[q.X, q.Y + 1] = source.At(q.X, q.Y);
                    distance[q.X, q.Y + 1] = d;
                }
            }
        }

        /// <summary>
        /// Brushfire transformation using 1-norm.
        /// </summary>
        public static void brushfire_1(ref Floatarray distance, Narray<Point> source, float maxdist)
        {
            BrushFire.Go(Metric1.Default, ref distance, ref source, maxdist);
        }

        /// <summary>
        /// Brushfire transformation using 2-norm.
        /// </summary>
        public static void brushfire_2(ref Floatarray distance, ref Narray<Point> source, float maxdist)
        {
            BrushFire.Go(Metric2.Default, ref distance, ref source, maxdist);
            for (int i = 0; i < distance.Length1d(); i++)
                distance.Put1d(i, (float)Math.Sqrt(distance.At1d(i)));
        }

        /// <summary>
        /// Brushfire transformation using infinity-norm.
        /// </summary>
        public static void brushfire_inf(ref Floatarray distance, Narray<Point> source, float maxdist)
        {
            BrushFire.Go(MetricInf.Default, ref distance, ref source, maxdist);
        }
    }
}
