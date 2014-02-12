using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ocronet.Dynamic.IOData;

namespace Ocronet.Dynamic.Recognizers
{
    public static class ClassifierUtil
    {
        public static float rNormal()
        {
            double x, y, s;
            do
            {
                x = 2 * DRandomizer.Default.drand() - 1;
                y = 2 * DRandomizer.Default.drand() - 1;
                s = x * x + y * y;
            } while (s > 1.0);
            return (float)(x * Math.Sqrt(-Math.Log(s) / s));
        }

        public static float rNormal(float mu, float sigma)
        {
            return rNormal() * sigma + mu;
        }

        public static float rLogNormal(float x, float r)
        {
            if (!(r > 1.0))
                throw new Exception("CHECK: r > 1.0");
            float n = rNormal((float)Math.Log(x), (float)Math.Log(r));
            float result = (float)Math.Exp(n);
            if (float.IsNaN(result))
                throw new Exception("CHECK: !float.IsNaN(result)");
            return result;
        }

        public static void fill_random(Floatarray v, float lo, float hi)
        {
            for (int i = 0; i < v.Length1d(); i++)
                v.Put1d(i, (float)((hi - lo) * DRandomizer.Default.drand() + lo));
        }

        public static void Sparsify(Floatarray a, int n)
        {
            NBest nbest = new NBest(n);
            for (int i = 0; i < a.Length1d(); i++)
                nbest.Add(i, Math.Abs(a.At1d(i)));
            double threshold = nbest.Value(n - 1);
            for (int i = 0; i < a.Length1d(); i++)
                if (Math.Abs(a.At1d(i)) < threshold)
                    a.Put1d(i, 0f);
        }

        public static int Binsearch<T>(Narray<T> v, T x)
        {
            float xf = Convert.ToSingle(x);
            int n = v.Length();
            if (n < 1) return -1;
            int lo = 0;
            int hi = v.Length();
            if (n > 2)
            {               // quick sanity check
                int i = DRandomizer.Default.nrand() % (n - 1), j = i + (DRandomizer.Default.nrand() % (n - i));
                if (!(Convert.ToSingle(v[i]) <= Convert.ToSingle(v[j])))
                    throw new Exception("CHECK: v[i] <= v[j]");
            }
            for ( ; ; )
            {
                int mean = (lo + hi) / 2;
                if (mean == lo) return mean;
                float value = Convert.ToSingle(v[mean]);
                if (value == xf) return mean;
                else if (value < xf) lo = mean;
                else hi = mean;
            }
        }

        public static bool Bincontains<T>(Narray<T> v, T x)
        {
            int index = Binsearch(v, x);
            return v[index].Equals(x);
        }

        public static int count_samples(Intarray classes)
        {
            int count = 0;
            for (int i = 0; i < classes.Length(); i++)
                if (classes[i] >= 0) count++;
            return count;
        }

        public static void weighted_sample(Intarray samples, Floatarray weights, int n)
        {
            Floatarray cs = new Floatarray();
            cs.Copy(weights);
            for (int i = 1; i < cs.Length(); i++)
                cs[i] += cs[i - 1];
            cs /= NarrayUtil.Max(cs);
            samples.Clear();
            for (int i = 0; i < n; i++)
            {
                float value = (float)DRandomizer.Default.drand();
                int where = Binsearch(cs, value);
                samples.Push(where);
            }
        }

        public static double Perplexity(Floatarray weights)
        {
            Floatarray w = new Floatarray();
            w.Copy(weights);
            w /= NarrayUtil.Sum(w);
            double total = 0.0;
            for (int i = 0; i < w.Length(); i++)
            {
                float value = w[i];
                total += value * Math.Log(value);
            }
            return Math.Exp(-total);
        }

        public static double Entropy(Floatarray a)
        {
            double z = NarrayUtil.Sum(a);
            double total = 0.0;
            for (int i = 0; i < a.Length(); i++)
            {
                double p = a[i] / z;
                if (p < 1e-8) continue;
                total += p * Math.Log(p);
            }
            return -total;
        }

        public static float estimate_errors(IModel classifier, IDataset ds, int n = 1000000)
        {
            Floatarray v = new Floatarray();
            int errors = 0;
            int count = 0;
            for (int i = 0; i < ds.nSamples(); i++)
            {
                int cls = ds.Cls(i);
                if (cls == -1) continue;
                ds.Input1d(v, i);
                int pred = classifier.Classify(v);
                count++;
                if (pred != cls) errors++;
            }
            return errors / (float)count;
        }

        public static void segmentation_as_bitmap(Bytearray image, Intarray cseg)
        {
            image.MakeLike(cseg);
            for (int i = 0; i < image.Length1d(); i++)
            {
                int value = cseg.At1d(i);
                if (value == 0 || value == 0xffffff) image.Put1d(i, 255);
                //if (value == 0xffffff) image.Put1d(i, 255);
            }
        }


    }
}
