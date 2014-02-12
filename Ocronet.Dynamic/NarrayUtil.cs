using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ocronet.Dynamic
{
    public class NarrayUtil
    {
        protected static void CHECK_ARG(bool condition, string message)
        {
            if (!condition)
                throw new Exception("CHECK_ARG: " + message);
        }

        /// <summary>
        /// Array subscripting with extending boundary conditions.
        /// </summary>
        public static T Ext<T>(Narray<T> a, int i, int j)
        {
            i = Math.Max(0, Math.Min(i, a.Dim(0) - 1));
            j = Math.Max(0, Math.Min(j, a.Dim(1) - 1));
            return a.UnsafeAt(i, j);
        }
        /// <summary>
        /// Array subscripting with extending boundary conditions.
        /// </summary>
        public static void ExtPut<T>(Narray<T> a, int i, int j, T value)
        {
            i = Math.Max(0, Math.Min(i, a.Dim(0) - 1));
            j = Math.Max(0, Math.Min(j, a.Dim(1) - 1));
            a.UnsafePut(i, j, value);
        }

        public static T Bat<T>(Narray<T> a, int i, int j, T value)
        {
            unchecked
            {
                if ((uint)(i) >= (uint)(a.Dim(0))) return value;
                if ((uint)(j) >= (uint)(a.Dim(1))) return value;
            }
            return a.UnsafeAt(i, j);
        }

        #region Max

        /// <summary>
        /// Compute the global max of the array.
        /// </summary>
        public static byte Max(Bytearray a)
        {
            byte value = a.At1d(0);
            for (int i = 1; i < a.Length1d(); i++)
            {
                byte nvalue = a.At1d(i);
                if (nvalue <= value) continue;
                value = nvalue;
            }
            return value;
        }
        public static int Max(Intarray a)
        {
            int value = a.At1d(0);
            for (int i = 1; i < a.Length1d(); i++)
            {
                int nvalue = a.At1d(i);
                if (nvalue <= value) continue;
                value = nvalue;
            }
            return value;
        }
        public static float Max(Floatarray a)
        {
            float value = a.At1d(0);
            for (int i = 1; i < a.Length1d(); i++)
            {
                float nvalue = a.At1d(i);
                if (nvalue <= value) continue;
                value = nvalue;
            }
            return value;
        }
        public static double Max(Doublearray a)
        {
            double value = a.At1d(0);
            for (int i = 1; i < a.Length1d(); i++)
            {
                double nvalue = a.At1d(i);
                if (nvalue <= value) continue;
                value = nvalue;
            }
            return value;
        }

        #endregion // Max

        #region Min

        /// <summary>
        /// Compute the global min of the array.
        /// </summary>
        public static int Min(Bytearray a)
        {
            byte value = a.At1d(0);
            for (int i = 1; i < a.Length1d(); i++)
            {
                byte nvalue = a.At1d(i);
                if (nvalue >= value) continue;
                value = nvalue;
            }
            return value;
        }
        public static int Min(Intarray a)
        {
            int value = a.At1d(0);
            for (int i = 1; i < a.Length1d(); i++)
            {
                int nvalue = a.At1d(i);
                if (nvalue >= value) continue;
                value = nvalue;
            }
            return value;
        }
        public static float Min(Floatarray a)
        {
            float value = a.At1d(0);
            for (int i = 1; i < a.Length1d(); i++)
            {
                float nvalue = a.At1d(i);
                if (nvalue >= value) continue;
                value = nvalue;
            }
            return value;
        }
        public static double Min(Doublearray a)
        {
            double value = a.At1d(0);
            for (int i = 1; i < a.Length1d(); i++)
            {
                double nvalue = a.At1d(i);
                if (nvalue >= value) continue;
                value = nvalue;
            }
            return value;
        }

        #endregion // Min

        public static void Greater(Bytearray outa, byte inval, byte no, byte yes)
        {
            for (int i = 0; i < outa.Length1d(); i++)
            {
                if (outa.UnsafeAt1d(i) > inval)
                    outa.UnsafePut1d(i, yes);
                else
                    outa.UnsafePut1d(i, no);
            }
        }

        #region ArgMax

        public static int ArgMax(Bytearray a)
        {
            if (!(/*a.Rank() == 1 && */a.Dim(0) > 0))
                throw new Exception("CHECK_ARG: a.Rank()==1 && a.Dim(0)>0");
            byte value = a.At1d(0);
            int index = 0;
            for (int i = 1; i < a.Length1d(); i++)
            {
                byte nvalue = a.At1d(i);
                if (nvalue <= value) continue;
                value = nvalue;
                index = i;
            }
            return index;
        }
        public static int ArgMax(Intarray a)
        {
            if (!(/*a.Rank() == 1 && **/a.Dim(0) > 0))
                throw new Exception("CHECK_ARG: a.Rank()==1 && a.Dim(0)>0");
            int value = a.At1d(0);
            int index = 0;
            for (int i = 1; i < a.Length1d(); i++)
            {
                int nvalue = a.At1d(i);
                if (nvalue <= value) continue;
                value = nvalue;
                index = i;
            }
            return index;
        }
        public static int ArgMax(Floatarray a)
        {
            if (!(/*a.Rank() == 1 && */a.Dim(0) > 0))
                throw new Exception("CHECK_ARG: a.Rank()==1 && a.Dim(0)>0");
            float value = a.At1d(0);
            int index = 0;
            for (int i = 1; i < a.Length1d(); i++)
            {
                float nvalue = a.At1d(i);
                if (nvalue <= value) continue;
                value = nvalue;
                index = i;
            }
            return index;
        }
        public static int ArgMax(Doublearray a)
        {
            if (!(a.Rank() == 1 && a.Dim(0) > 0))
                throw new Exception("CHECK_ARG: a.Rank()==1 && a.Dim(0)>0");
            double value = a.At1d(0);
            int index = 0;
            for (int i = 1; i < a.Length1d(); i++)
            {
                double nvalue = a.At1d(i);
                if (nvalue <= value) continue;
                value = nvalue;
                index = i;
            }
            return index;
        }

        public static int ArgMin(Bytearray a)
        {
            if (!(/*a.Rank() == 1 && **/a.Dim(0) > 0))
                throw new Exception("CHECK_ARG: a.Rank()==1 && a.Dim(0)>0");
            byte value = a.At1d(0);
            int index = 0;
            for (int i = 1; i < a.Length1d(); i++)
            {
                byte nvalue = a.At1d(i);
                if (nvalue >= value) continue;
                value = nvalue;
                index = i;
            }
            return index;
        }
        public static int ArgMin(Intarray a)
        {
            if (!(/*a.Rank() == 1 && **/a.Dim(0) > 0))
                throw new Exception("CHECK_ARG: a.Rank()==1 && a.Dim(0)>0");
            int value = a.At1d(0);
            int index = 0;
            for (int i = 1; i < a.Length1d(); i++)
            {
                int nvalue = a.At1d(i);
                if (nvalue >= value) continue;
                value = nvalue;
                index = i;
            }
            return index;
        }
        public static int ArgMin(Floatarray a)
        {
            if (!(/*a.Rank() == 1 && **/a.Dim(0) > 0))
                throw new Exception("CHECK_ARG: a.Rank()==1 && a.Dim(0)>0");
            float value = a.At1d(0);
            int index = 0;
            for (int i = 1; i < a.Length1d(); i++)
            {
                float nvalue = a.At1d(i);
                if (nvalue >= value) continue;
                value = nvalue;
                index = i;
            }
            return index;
        }
        public static int ArgMin(Doublearray a)
        {
            if (!(/*a.Rank() == 1 && **/a.Dim(0) > 0))
                throw new Exception("CHECK_ARG: a.Rank()==1 && a.Dim(0)>0");
            double value = a.At1d(0);
            int index = 0;
            for (int i = 1; i < a.Length1d(); i++)
            {
                double nvalue = a.At1d(i);
                if (nvalue >= value) continue;
                value = nvalue;
                index = i;
            }
            return index;
        }

        #endregion // ArgMax

        #region Arithmetic operations

        /// <summary>
        /// Add outarray[i] + val
        /// </summary>
        public static void Add(Bytearray outarray, byte val)
        {
            for (int i = 0; i < outarray.Length1d(); i++)
                outarray.UnsafePut1d(i, (byte)(outarray.UnsafeAt1d(i) + val));
        }
        /// <summary>
        /// Add outarray[i] + val
        /// </summary>
        public static void Add(Intarray outarray, int val)
        {
            for (int i = 0; i < outarray.Length1d(); i++)
                outarray.UnsafePut1d(i, outarray.UnsafeAt1d(i) + val);
        }
        /// <summary>
        /// Add outarray[i] + val
        /// </summary>
        public static void Add(Floatarray outarray, float val)
        {
            for (int i = 0; i < outarray.Length1d(); i++)
                outarray.UnsafePut1d(i, outarray.UnsafeAt1d(i) + val);
        }

        /// <summary>
        /// Subtraction val - outarray[i]
        /// </summary>
        public static void Sub(byte val, Bytearray outarray)
        {
            for(int i=0; i<outarray.Length1d(); i++)
                outarray.Put1d(i, (byte)(val - outarray.At1d(i)));
        }
        /// <summary>
        /// Subtraction val - outarray[i]
        /// </summary>
        public static void Sub(int val, Intarray outarray)
        {
            for (int i = 0; i < outarray.Length1d(); i++)
                outarray.Put1d(i, val - outarray.At1d(i));
        }
        /// <summary>
        /// Subtraction val - outarray[i]
        /// </summary>
        public static void Sub(float val, Floatarray outarray)
        {
            for (int i = 0; i < outarray.Length1d(); i++)
                outarray.Put1d(i, val - outarray.At1d(i));
        }

        /// <summary>
        /// Subtraction outarray[i] - val
        /// </summary>
        public static void Sub(Bytearray outarray, byte val)
        {
            for (int i = 0; i < outarray.Length1d(); i++)
                outarray.Put1d(i, (byte)(outarray.At1d(i) - val));
        }
        /// <summary>
        /// Subtraction outarray[i] - val
        /// </summary>
        public static void Sub(Intarray outarray, int val)
        {
            for (int i = 0; i < outarray.Length1d(); i++)
                outarray.Put1d(i, outarray.At1d(i) - val);
        }
        /// <summary>
        /// Subtraction outarray[i] - val
        /// </summary>
        public static void Sub(Floatarray outarray, float val)
        {
            for (int i = 0; i < outarray.Length1d(); i++)
                outarray.Put1d(i, outarray.At1d(i) - val);
        }

        /// <summary>
        /// Division outarray[i] / val
        /// </summary>
        public static void Div(Bytearray outarray, byte val)
        {
            for (int i = 0; i < outarray.Length1d(); i++)
                outarray.Put1d(i, (byte)(outarray.At1d(i) / val));
        }
        /// <summary>
        /// Division outarray[i] / val
        /// </summary>
        public static void Div(Intarray outarray, int val)
        {
            for (int i = 0; i < outarray.Length1d(); i++)
                outarray.Put1d(i, outarray.At1d(i) / val);
        }
        /// <summary>
        /// Division outarray[i] / val
        /// </summary>
        public static void Div(Floatarray outarray, float val)
        {
            for (int i = 0; i < outarray.Length1d(); i++)
                outarray.Put1d(i, outarray.At1d(i) / val);
        }

        public static double Sum<T>(Narray<T> data)
        {
            double result = 0.0;
            int n = data.Length1d();
            for (int i = 0; i < n; i++)
                result += Convert.ToDouble(data.At1d(i));
            return result;
        }

        /// <summary>
        /// Euclidean distance squared.
        /// </summary>
        public static double Dist2Squared(Floatarray a, Floatarray b)
        {
            if (!a.SameDims(b))
                throw new Exception("Dist2Squared: !a.SameDims(b)");
            double total = 0.0;
            for (int i = 0; i < a.Length1d(); i++)
            {
                float val = a.At1d(i) - b.At1d(i);
                total += val * val;
            }
            if (double.IsNaN(total))
                throw new Exception("Dist2Squared: total is NaN !");
            return total;
        }

        /// <summary>
        /// Euclidean distance.
        /// </summary>
        public static double Dist2(Floatarray a, Floatarray b)
        {
            return Math.Sqrt(Dist2Squared(a, b));
        }

        /// <summary>
        /// Euclidean norm squared.
        /// </summary>
        public static double Norm2Squared(Floatarray a)
        {
            double total = 0.0;
            for (int i = 0; i < a.Length1d(); i++)
            {
                float value = a.At1d(i);
                total += value * value;
            }
            return total;
        }

        /// <summary>
        /// Euclidean norm.
        /// </summary>
        public static double Norm2(Floatarray a)
        {
            return Math.Sqrt(Norm2Squared(a));
        }

        /// <summary>
        /// Normalize the Euclidean norm of the array.
        /// </summary>
        public static void Normalize2(Floatarray a)
        {
            double scale = 1.0 / Norm2(a);
            for (int i = 0; i < a.Length1d(); i++)
                a.Put1d(i, (float)(a.At1d(i) * scale));
        }

        /// <summary>
        /// 1-norm.
        /// </summary>
        public static double Norm1(Floatarray a)
        {
            double total = 0.0;
            for (int i = 0; i < a.Length1d(); i++)
                total += Math.Abs(a.UnsafeAt1d(i));
            return total;
        }

        /// <summary>
        /// Normalize the 1-norm of the array.
        /// </summary>
        public static void Normalize1(Floatarray a)
        {
            double scale = 1.0 / Norm1(a);
            for (int i = 0; i < a.Length1d(); i++)
                a.UnsafePut1d(i, (float)(a.UnsafeAt1d(i) * scale));
        }

        #endregion // arithmetic operations

        /// <summary>
        /// Reverse an array
        /// </summary>
        public static void Reverse<T>(Narray<T> outa, Narray<T> ina)
        {
            outa.Clear();
            for (int i = ina.Length() - 1; i >= 0; i--)
                outa.Push(ina[i]);
        }

        /// <summary>
        /// Quicksort an array, generating a permutation of the indexes.
        /// </summary>
        public static void Quicksort<T>(Narray<int> outindex, Narray<T> values)
        {
            outindex.Resize(values.Length());
            for (int i = 0; i < values.Length(); i++) outindex[i] = i;
            Quicksort(outindex, values, 0, outindex.Length());
        }

        public static void Quicksort<T>(Narray<int> outindex, Narray<T> values, int start, int end)
        {
            if (start >= end - 1) return;

            // pick a pivot
            // NB: it's OK for this to be a reference pointing into values
            // since we aren't actually moving the elements of values[] around

            T pivot = values[outindex[(start + end - 1) / 2]];

            // first, split into two parts: less than the pivot
            // and greater-or-equal

            int lo = start;
            int hi = end;
            for (; ; )
            {
                while (lo < hi && Convert.ToSingle(values[outindex[lo]]) < Convert.ToSingle(pivot)) lo++;
                while (lo < hi && Convert.ToSingle(values[outindex[hi - 1]]) >= Convert.ToSingle(pivot)) hi--;
                if (lo == hi || lo == hi - 1) break;
                //old: qswap_(index[lo], index[hi - 1]);
                int tmpv = outindex[lo];
                outindex[lo] = outindex[hi - 1];
                outindex[hi - 1] = tmpv;
                lo++;
                hi--;
            }
            int split1 = lo;

            // now split into two parts: equal to the pivot
            // and strictly greater.

            hi = end;
            for (; ; )
            {
                while (lo < hi && Convert.ToSingle(values[outindex[lo]]) == Convert.ToSingle(pivot)) lo++;
                while (lo < hi && Convert.ToSingle(values[outindex[hi - 1]]) > Convert.ToSingle(pivot)) hi--;
                if (lo == hi || lo == hi - 1) break;
                //old: qswap_(index[lo], index[hi - 1]);
                int tmpv = outindex[lo];
                outindex[lo] = outindex[hi - 1];
                outindex[hi - 1] = tmpv;
                lo++;
                hi--;
            }
            int split2 = lo;

            Quicksort(outindex, values, start, split1);
            Quicksort(outindex, values, split2, end);
        }

        public static void Quicksort<T>(Narray<T> values, int start, int end)
        {
            if (start >= end - 1) return;

            // pick a pivot
            // NB: this cannot be a reference to the value (since we're moving values around)
            T pivot = values[(start + end - 1) / 2];

            // first, split into two parts: less than the pivot
            // and greater-or-equal
            int lo = start;
            int hi = end;
            for (; ; )
            {
                while (lo < hi && Convert.ToSingle(values[lo]) < Convert.ToSingle(pivot)) lo++;
                while (lo < hi && Convert.ToSingle(values[hi - 1]) >= Convert.ToSingle(pivot)) hi--;
                if (lo == hi || lo == hi - 1) break;
                //old: qswap_(values[lo], values[hi - 1]);
                T tmpv = values[lo];
                values[lo] = values[hi - 1];
                values[hi - 1] = tmpv;
                lo++;
                hi--;
            }
            int split1 = lo;

            // now split into two parts: equal to the pivot
            // and strictly greater.
            hi = end;
            for (; ; )
            {
                while (lo < hi && Convert.ToSingle(values[lo]) == Convert.ToSingle(pivot)) lo++;
                while (lo < hi && Convert.ToSingle(values[hi - 1]) > Convert.ToSingle(pivot)) hi--;
                if (lo == hi || lo == hi - 1) break;
                //old: qswap_(values[lo], values[hi - 1]);
                T tmpv = values[lo];
                values[lo] = values[hi - 1];
                values[hi - 1] = tmpv;
                lo++;
                hi--;
            }
            int split2 = lo;

            Quicksort(values, start, split1);
            Quicksort(values, split2, end);
        }

        public static void Quicksort<T>(Narray<T> values)
        {
            Quicksort(values, 0, values.Length());
        }

        /// <summary>
        /// Find unique elements.
        /// </summary>
        public static void Uniq<T>(Narray<T> values)
        {
            if (values.Length() == 0) return;
            Quicksort(values);
            int j = 1;
            for (int i = 1; i < values.Length(); i++)
            {
                if (values[i].Equals(values[j - 1])) continue;
                values[j++] = values[i];
            }
            values.Truncate(j);
        }

        public static float Fractile(Narray<float> a, double f)
        {
            Floatarray temp = new Floatarray();
            if (!(f >= 0 && f <= 1))
                throw new Exception("CHECK: f >= 0 && f <= 1");
            temp.Copy(a);
            temp.Reshape(temp.Length1d());
            Quicksort(temp);
            return temp[(int)(f * temp.Length())];
        }

        public static float Median(Narray<float> a)
        {
            Narray<float> s = new Narray<float>();
            s.Copy(a);
            s.Reshape(s.Length1d());
            Quicksort(s);
            int n = s.Length();
            if (n == 0)
                return 0;
            if ((n % 2) > 0)
                return s[n / 2];
            else
                return (s[n / 2 - 1] + s[n / 2]) / 2.0f;
        }
        public static float Median(Narray<int> a)
        {
            Narray<int> s = new Narray<int>();
            s.Copy(a);
            s.Reshape(s.Length1d());
            Quicksort(s);
            int n = s.Length();
            if (n == 0)
                return 0;
            if ((n % 2) > 0)
                return s[n / 2];
            else
                return (s[n / 2 - 1] + s[n / 2]) / 2.0f;
        }

        public static bool contains_only<T, U, V>(Narray<T> a, U value1, V value2)
        {
            for (int i = 0; i < a.Length1d(); i++)
            {
                if (!a.At1d(i).Equals(value1) && !a.At1d(i).Equals(value2))
                    return false;
            }
            return true;
        }

        public static int first_index_of<T>(Narray<T> a, T target)
        {
            for (int i = 0; i < a.Length(); i++)
                if (a[i].Equals(target)) return i;
            return -1;
        }
        public static int first_index_of(Narray<int> a, int target)
        {
            for (int i = 0; i < a.Length(); i++)
                if (a[i] == target) return i;
            return -1;
        }

        /// <summary>
        /// Permute the elements of an array given a permutation.
        /// </summary>
        public static void Permute<T>(Narray<T> data, Narray<int> permutation)
        {
            if (!data.SameDims(permutation))
                throw new Exception("CHECK_ARG: data.SameDims(permutation)");
            Narray<bool> finished = new Narray<bool>(data.Length());
            finished.Fill(false);
            for (int start = 0; start < finished.Length(); start++)
            {
                if (finished[start]) continue;
                int index = start;
                T value = data[index];
                for ( ; ; )
                {
                    int next = permutation[index];
                    if (next == start) break;
                    data[index] = data[next];
                    index = next;
                    //CHECK_ARG(!finished[index] && "not a permutation");
                    if (finished[index])
                        throw new Exception("CHECK_ARG: !finished[index]");
                    finished[index] = true;
                    index = next;
                }
                data[index] = value;
                finished[index] = true;
            }
        }

        public static void Shuffle<T>(Narray<T> values)
        {
            Floatarray temp = new Floatarray(values.Length());
            Intarray index = new Intarray();
            for (int i = 0; i < temp.Length(); i++)
                temp.UnsafePut1d(i, DRandomizer.Default.drand());
            Quicksort(index, temp);
            Permute(values, index);
        }

        public static void RPermutation(Intarray index, int n)
        {
            index.Resize(n);
            for (int i = 0; i < n; i++) index.UnsafePut1d(i, i);
            Shuffle(index);
        }

        /// <summary>
        /// Original name: randomly_permute
        /// </summary>
        public static void RandomlyPermute<T>(Narray<T> v)
        {
            int n = v.Length();
            for (int i = 0; i < n - 1; i++)
            {
                int target = DRandomizer.Default.nrand() % (n - i) + i;
                T temp = v[target];
                v[target] = v[i];
                v[i] = temp;
            }
        }

    }
}
