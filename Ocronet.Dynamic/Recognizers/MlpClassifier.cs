using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ocronet.Dynamic.Utils;
using Ocronet.Dynamic.IOData;

namespace Ocronet.Dynamic.Recognizers
{
    public class MlpClassifier : IBatchDense
    {
        protected Floatarray w1, b1, w2, b2;
        //protected float eta;
        protected float cv_error;
        protected float nn_error;
        protected bool crossvalidate;
        protected int personal_number = -1;  // personal number

        public MlpClassifier()
        {
            w1 = new Floatarray();
            b1 = new Floatarray();
            w2 = new Floatarray();
            b2 = new Floatarray();
            PDef("eta", 0.5, "default learning rate");
            PDef("eta_init", 0.5, "initial eta");
            PDef("eta_varlog", 1.5, "eta variance in lognormal");
            PDef("hidden_varlog", 1.2, "nhidden variance in lognormal");
            PDef("rounds", 8, "number of training rounds");
            PDef("miters", 8, "number of presentations in multiple of training set");
            PDef("nensemble", 4, "number of mlps in ensemble");
            PDef("hidden_min", 5, "minimum number of hidden units");
            PDef("hidden_lo", 20, "minimum number of hidden units at start");
            PDef("hidden_hi", 80, "maximum number of hidden units at start");
            PDef("hidden_max", 300, "maximum number of hidden units");
            PDef("sparse", -1, "sparsify the hidden layer");
            PDef("cv_split", 0.8, "cross validation split");
            PDef("cv_max", 5000, "max # samples to use for cross validation");
            PDef("normalization", -1, "kind of normalization of the input");
            PDef("noopt", 0, "disable optimization search");
            PDef("crossvalidate", 1, "perform crossvalidation");
            //eta = PGetf("eta");
            cv_error = 1e30f;
            nn_error = 1e30f;
            crossvalidate = PGetb("crossvalidate");
            Persist(w1, "w1");
            Persist(b1, "b1");
            Persist(w2, "w2");
            Persist(b2, "b2");
        }

        public MlpClassifier(int PersonalNumber)
            : this( )
        {
            personal_number = PersonalNumber;
        }

        public override string Name
        {
            get { return "mlp"; }
        }

        public string FullName
        {
            get { return personal_number >= 0 ? "MlpClassifier" + personal_number : "MlpClassifier"; }
        }

        public override void Info()
        {
            bool bak = Logger.Default.verbose;
            Logger.Default.verbose = true;
            Logger.Default.WriteLine("MLP");
            PPrint();
            Logger.Default.WriteLine(String.Format("nInput {0} nHidden {1} nOutput {2}",
                w1.Dim(1), w1.Dim(0), w2.Dim(0)));
            if (w1.Length() > 0 && w2.Length() > 0)
            {
                Logger.Default.WriteLine(String.Format("w1 [{0},{1}] b1 [{2},{3}]",
                    NarrayUtil.Min(w1), NarrayUtil.Max(w1), NarrayUtil.Min(b1), NarrayUtil.Max(b1)));
                Logger.Default.WriteLine(String.Format("w2 [{0},{1}] b2 [{2},{3}]",
                    NarrayUtil.Min(w2), NarrayUtil.Max(w2), NarrayUtil.Min(b2), NarrayUtil.Max(b2)));
            }
            Logger.Default.verbose = bak;
        }

        protected void normalize(Floatarray v)
        {
            float kind = PGetf("normalization");
            if (kind < 0) return;
            if (kind == 1)
            {
                double total = 0.0;
                for (int i = 0; i < v.Length(); i++) total += Math.Abs(v[i]);
                for (int i = 0; i < v.Length(); i++) v[i] = (float)(v[i] / total);
            }
            else if (kind == 2)
            {
                double total = 0.0;
                for (int i = 0; i < v.Length(); i++) total += Math.Sqrt(v[i]);
                total = Math.Sqrt(total);
                for (int i = 0; i < v.Length(); i++) v[i] = (float)(v[i] / total);
            }
            else if (kind < 999)
            {
                double total = 0.0;
                for (int i = 0; i < v.Length(); i++) total += Math.Pow(v[i], kind);
                total = Math.Pow(total, 1.0 / kind);
                for (int i = 0; i < v.Length(); i++) v[i] = (float)(v[i] / total);
            }
            else if (kind == 999)
            {
                double total = 0.0;
                for (int i = 0; i < v.Length(); i++) if (Math.Abs(v[i]) > total) total = Math.Abs(v[i]);
                for (int i = 0; i < v.Length(); i++) v[i] = (float)(v[i] / total);
            }
            else
            {
                throw new Exception("bad normalization in mlp");
            }
        }

        public override int nFeatures()
        {
            return w1.Dim(1);
        }

        public int nHidden()
        {
            return w1.Dim(0);
        }

        public float Complexity()
        {
            return w1.Dim(0);
        }

        public override int nClasses()
        {
            return w2.Dim(0);
        }

        public void Copy(MlpClassifier other)
        {
            w1.Copy(other.w1);
            b1.Copy(other.b1);
            w2.Copy(other.w2);
            b2.Copy(other.b2);
            if (c2i.Length() < 1)
                c2i.Copy(other.c2i);
            if (i2c.Length() < 1)
                i2c.Copy(other.i2c);
        }

        public override float OutputsDense(Floatarray result, Floatarray x_raw)
        {
            CHECK_ARG(x_raw.Length() == w1.Dim(1), "x_raw.Length() == w1.Dim(1)");
            Floatarray z = new Floatarray();
            int sparse = PGeti("sparse");
            Floatarray y = new Floatarray();
            Floatarray x = new Floatarray();
            x.Copy(x_raw);
            mvmul0(y, w1, x);
            y += b1;
            for (int i = 0; i < y.Length(); i++)
                y[i] = sigmoid(y[i]);
            if (sparse > 0)
                ClassifierUtil.Sparsify(y, sparse);
            mvmul0(z, w2, y);
            z += b2;
            for (int i = 0; i < z.Length(); i++)
                z[i] = sigmoid(z[i]);
            result.Copy(z);
            //int idx = NarrayUtil.ArgMax(result);
            //float val = NarrayUtil.Max(result);
            return Convert.ToSingle(Math.Abs(NarrayUtil.Sum(z) - 1.0));
        }

        public void ChangeHidden(int newn)
        {
            MlpClassifier temp = new MlpClassifier();
            int ninput = w1.Dim(1);
            int nhidden = w1.Dim(0);
            int noutput = w2.Dim(0);
            temp.InitRandom(ninput, newn, noutput);
            for (int i = 0; i < newn; i++)
            {
                if (i >= nhidden)
                {
                    for (int j = 0; j < ninput; j++)
                        temp.w1[i, j] = ClassifierUtil.rNormal(0.0f, 1.0f);
                    temp.b1[i] = ClassifierUtil.rNormal(0.0f, 1.0f);
                }
                else
                {
                    for (int j = 0; j < ninput; j++)
                        temp.w1[i, j] = w1[i, j];
                    temp.b1[i] = b1[i];
                }
            }
            for (int i = 0; i < noutput; i++)
            {
                for (int j = 0; j < newn; j++)
                {
                    if (j >= nhidden)
                    {
                        temp.w2[i, j] = 1e-2f * ClassifierUtil.rNormal(0.0f, 1.0f);
                    }
                    else
                    {
                        temp.w2[i, j] = w2[i, j];
                    }
                }
            }
            this.Copy(temp);
        }

        public void InitData(IDataset ds, int nhidden, Intarray newc2i = null, Intarray newi2c = null)
        {
            CHECK_ARG(nhidden > 1 && nhidden < 1000000, "nhidden > 1 && nhidden < 1000000");
            int ninput = ds.nFeatures();
            int noutput = ds.nClasses();
            w1.Resize(nhidden, ninput);
            b1.Resize(nhidden);
            w2.Resize(noutput, nhidden);
            b2.Resize(noutput);
            Intarray indexes = new Intarray();
            NarrayUtil.RPermutation(indexes, ds.nSamples());
            Floatarray v = new Floatarray();
            for (int i = 0; i < w1.Dim(0); i++)
            {
                int row = indexes[i];
                ds.Input1d(v, row);
                float normv = (float)NarrayUtil.Norm2(v);
                v /= normv * normv;
                NarrayRowUtil.RowPut(w1, i, v);
            }
            ClassifierUtil.fill_random(b1, -1e-6f, 1e-6f);
            ClassifierUtil.fill_random(w2, -1.0f / nhidden, 1.0f / nhidden);
            ClassifierUtil.fill_random(b2, -1e-6f, 1e-6f);
            if (newc2i != null)
                c2i.Copy(newc2i);
            if (newi2c != null)
                i2c.Copy(newi2c);
        }

        protected void InitRandom(int ninput, int nhidden, int noutput)
        {
            w1.Resize(nhidden, ninput);
            b1.Resize(nhidden);
            w2.Resize(noutput, nhidden);
            b2.Resize(noutput);
            float range = 1.0f / Math.Max(ninput, nhidden);
            ClassifierUtil.fill_random(w1, -range, range);
            ClassifierUtil.fill_random(b1, -range, range);
            ClassifierUtil.fill_random(w2, -range, range);
            ClassifierUtil.fill_random(b2, -range, range);
        }

        public float CrossValidatedError()
        {
            return cv_error;
        }

        /// <summary>
        /// do a single stochastic gradient descent step
        /// </summary>
        public void TrainOne(Floatarray z, Floatarray target, Floatarray x, float eta)
        {
            CHECK_ARG(target.Length() == w2.Dim(0), "target.Length() == w2.Dim(0)");
            CHECK_ARG(x.Length() == w1.Dim(1), "x.Length() == w1.Dim(1)");

            int sparse = PGeti("sparse");
            int nhidden = this.nHidden();
            int noutput = nClasses();
            Floatarray delta1 = new Floatarray(nhidden);
            Floatarray delta2 = new Floatarray(noutput);
            Floatarray y = new Floatarray(nhidden);

            mvmul0(y, w1, x);
            y += b1;
            for (int i = 0; i < nhidden; i++)
                y[i] = sigmoid(y[i]);
            if (sparse > 0) ClassifierUtil.Sparsify(y, sparse);
            mvmul0(z, w2, y);
            z += b2;
            for (int i = 0; i < noutput; i++)
                z[i] = sigmoid(z[i]);

            for (int i = 0; i < noutput; i++)
                delta2[i] = (z[i] - target[i]) * dsigmoidy(z[i]);
            vmmul0(delta1, delta2, w2);
            for (int i = 0; i < nhidden; i++)
                delta1[i] = delta1[i] * dsigmoidy(y[i]);

            outer_add(w2, delta2, y, -eta);
            for (int i = 0; i < noutput; i++)
                b2[i] -= eta * delta2[i];
            outer_add(w1, delta1, x, -eta);
            for (int i = 0; i < nhidden; i++)
                b1[i] -= eta * delta1[i];
        }

        public override void TrainDense(IDataset ds)
        {
            int nclasses = ds.nClasses();
            float miters = PGetf("miters");
            int niters = (int)(ds.nSamples() * miters);
            niters = Math.Max(1000, Math.Min(10000000,niters));
            double err = 0.0;
            Floatarray x = new Floatarray();
            Floatarray z = new Floatarray();
            Floatarray target = new Floatarray(nclasses);
            int count = 0;
            for (int i = 0; i < niters; i++)
            {
                int row = i % ds.nSamples();
                ds.Output(target, row);
                ds.Input1d(x, row);
                TrainOne(z, target, x, PGetf("eta"));
                err += NarrayUtil.Dist2Squared(z, target);
                count++;
            }
            err /= count;
            Global.Debugf("info", "   {4} n {0} niters={1} eta={2:0.#####} errors={3:0.########}",
                   ds.nSamples(), niters, PGetf("eta"), err, FullName);
        }


        #region Helper functions for neural networks

        /// <summary>
        /// random samples from the normal density
        /// </summary>
        protected static void mvmul0(Floatarray result, Floatarray a, Floatarray v)
        {
            int n = a.Dim(0);
            int m = a.Dim(1);
            CHECK_ARG(m == v.Length(), "m == v.Length()");
            result.Resize(n);
            result.Fill(0f);
            for (int j = 0; j < m; j++)
            {
                float value = v.UnsafeAt(j);
                if (value == 0f)
                    continue;
                for (int i = 0; i < n; i++)
                    result.UnsafePut(i, result.UnsafeAt(i) + (a.UnsafeAt(i, j) * value));
            }
        }

        protected static void vmmul0(Floatarray result, Floatarray v, Floatarray a)
        {
            int n = a.Dim(0);
            int m = a.Dim(1);
            CHECK_ARG(n == v.Length(), "n == v.Length()");
            result.Resize(m);
            result.Fill(0f);
            for (int i = 0; i < n; i++)
            {
                float value = v.UnsafeAt(i);//v[i];
                if (value == 0f)
                    continue;
                for (int j = 0; j < m; j++)
                    result.UnsafePut(j, result.UnsafeAt(j) + (a.UnsafeAt(i, j) * value));
            }
        }

        protected static int count_zeros(Floatarray a)
        {
            int n = a.Length1d();
            int count = 0;
            for (int i = 0; i < n; i++)
                if (a.UnsafeAt1d(i) == 0f) count++;
            return count;
        }

        protected static void outer_add(Floatarray a, Floatarray u, Floatarray v, float eps)
        {
            int n = a.Dim(0);
            int m = a.Dim(1);
            CHECK_ARG(n == u.Length(), "n == u.Length()");
            CHECK_ARG(m == v.Length(), "m == v.Length()");
            if (count_zeros(u) >= count_zeros(v))
            {
                for (int i = 0; i < n; i++)
                {
                    if (u.UnsafeAt(i) == 0) continue;
                    for (int j = 0; j < m; j++)
                    {
                        a.UnsafePut(i, j, a.UnsafeAt(i, j) + (eps * u.UnsafeAt(i) * v.UnsafeAt(j)));
                    }
                }
            }
            else
            {
                for (int j = 0; j < m; j++)
                {
                    if (v.UnsafeAt(j) == 0) continue;
                    for (int i = 0; i < n; i++)
                    {
                        a.UnsafePut(i, j, a.UnsafeAt(i, j) + (eps * u.UnsafeAt(i) * v.UnsafeAt(j)));
                    }
                }
            }
        }

        /// <summary>
        /// sigmoid function for neural networks
        /// 0 .. 1
        /// </summary>
        public static float sigmoid(float x)
        {
            return Convert.ToSingle(1.0 / (1.0 + Math.Exp(-Math.Min(Math.Max(x, -20.0), 20.0))));
        }

        public static float dsigmoidy(float y)
        {
            return y * (1 - y);
        }

        /// <summary>
        /// logarithmically spaced samples in an interval
        /// </summary>
        protected static float logspace(int i, int n, float lo, float hi)
        {
            return Convert.ToSingle(Math.Exp((i / (float)(n - 1)) * (Math.Log(hi) - Math.Log(lo)) + Math.Log(lo)));
        }

        #region Lenet sigmoid function

        static double PR = 0.66666666;
        static double PO = 1.71593428;
        static double A0 = 1.0;
        static double A1 = 0.125 * PR;
        static double A2 = 0.0078125 * PR * PR;
        static double A3 = 0.000325520833333 * PR * PR * PR;

        /// <summary>
        /// "standard" sigmoid
        /// -1.71593428 .. 1.71593428
        /// </summary>
        public static float sigmoid_speed(float x)
        {
            double y;

            if (x >= 0.0)
                if (x < (double)13)
                    y = A0 + x * (A1 + x * (A2 + x * (A3)));
                else
                    return (float)PO;
            else
                if (x > -(double)13)
                    y = A0 - x * (A1 - x * (A2 - x * (A3)));
                else
                    return (float)-PO;
            y *= y;
            y *= y;
            y *= y;
            y *= y;
            return (float)((x > 0.0) ? PO * (y - 1.0) / (y + 1.0) : PO * (1.0 - y) / (y + 1.0));
        }

        /// <summary>
        /// derivative of the "standard" sigmoid
        /// </summary>
        public static float dstdsigmoid(float x)
        {
            if (x < 0.0)
                x = -x;
            if (x < (double)13)
            {
                double y;
                y = A0 + x * (A1 + x * (A2 + x * (A3)));
                y *= y;
                y *= y;
                y *= y;
                y *= y;
                y = (y - 1.0) / (y + 1.0);
                return (float)(PR * PO - PR * PO * y * y);
            }
            else
                return 0.0f;
        }

        #endregion

        #endregion // Helper functions for neural networks


    }
}
