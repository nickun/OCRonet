using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ocronet.Dynamic
{
    public class NarrayRowUtil
    {
        protected static void CHECK_ARG(bool condition, string message)
        {
            if (!condition)
                throw new Exception("CHECK_ARG: " + message);
        }

        public static int RowArgMax<T>(Narray<T> values, int i)
        {
            if (values.Dim(1) < 1) return -1;
            int mj = 0;
            T mv = values[i, 0];
            for (int j = 1; j < values.Dim(1); j++)
            {
                T value = values[i, j];
                if (value.Equals(mv)) continue;
                if (Convert.ToDouble(value) < Convert.ToDouble(mv)) continue;
                mv = value;
                mj = j;
            }
            return mj;
        }

        public static int RowArgMin<T>(Narray<T> values, int i)
        {
            if (values.Dim(1) < 1) return -1;
            int mj = 0;
            T mv = values[i, 0];
            for (int j = 1; j < values.Dim(1); j++)
            {
                T value = values[i, j];
                if (value.Equals(mv)) continue;
                if (Convert.ToDouble(value) > Convert.ToDouble(mv)) continue;
                mv = value;
                mj = j;
            }
            return mj;
        }

        public static void RowGet<T, S>(Narray<T> outv, Narray<S> data, int row)
        {
            outv.Resize(data.Dim(1));
            for (int i = 0; i < outv.Length(); i++)
                outv[i] = (T)Convert.ChangeType(data[row, i], typeof(T));
        }

        public static void RowPut<T, S>(Narray<T> data, int row, Narray<S> v)
        {
            if (!(v.Length() == data.Dim(1)))
                throw new Exception("CHECK: v.Length() == data.Dim(1)");
            for (int i = 0; i < v.Length(); i++)
                data[row, i] = (T) Convert.ChangeType(v[i], typeof(T));
        }

        public static void RowPush<T>(Narray<T> table, Narray<T> data)
        {
            if (table.Length1d() == 0)
            {
                table.Copy(data);
                table.Reshape(1, table.Length());
                return;
            }
            CHECK_ARG(table.Dim(1) == data.Length(), "table.Dim(1) == data.Length()");
            table.Reserve(table.Length1d() + data.Length());
            table.SetDims(table.Dim(0) + 1, table.Dim(1), 0, 0);
            int irow = table.Dim(0) - 1;
            for (int k = 0; k < table.Dim(1); k++)
                table[irow, k] = data.UnsafeAt1d(k);
        }

        public static void RowCopy<T>(Narray<T> a, Narray<T> b, int i)
        {
            a.Resize(b.Dim(1));
            for (int k = 0; k < b.Dim(1); k++)
                a[k] = b[i, k];
        }

        public static void RowCopy<T>(Narray<T> values, int i, int j)
        {
            for (int k = 0; k < values.Dim(1); k++)
                values[i, k] = values[j, k];
        }

        public static void RowCopy<T>(Narray<T> a, int i, Narray<T> b)
        {
            CHECK_ARG(a.Dim(1) == b.Length(), "a.Dim(1) == b.Length()");
            for (int k = 0; k < a.Dim(1); k++)
                a[i, k] = b[k];
        }

        public static void RowPermute<T>(Narray<T> data, Narray<int> permutation)
        {
            CHECK_ARG(data.Dim(0) == permutation.Length(), "data.Dim(0) == permutation.Length()");
            Narray<bool> finished = new Narray<bool>(data.Dim(0));
            finished.Fill(false);
            for (int start = 0; start < finished.Length(); start++)
            {
                if (finished[start]) continue;
                int index = start;
                Narray<T> value = new Narray<T>();
                RowCopy(value, data, index);
                for ( ; ; )
                {
                    int next = permutation[index];
                    if (next == start) break;
                    RowCopy(data, index, next);
                    index = next;
                    CHECK_ARG(!finished[index], "!finished[index]");
                    finished[index] = true;
                    index = next;
                }
                RowCopy(data, index, value);
                finished[index] = true;
            }
        }

    }
}
