using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ocronet.Dynamic.Recognizers
{
    /// <summary>
    /// OutputVector is a sparse vector class, used for representing
    /// classifier outputs.
    /// </summary>
    public class OutputVector
    {
        private int _len;
        private Intarray _keys;
        private Floatarray _values;
        private Floatarray _result;

        public OutputVector()
        {
            _len = 0;
            _keys = new Intarray();
            _values = new Floatarray();
            _result = null;
        }

        public OutputVector(int n) : this()
        {
            Init(n);
        }

        /// <summary>
        /// If it's initialized with an array, the result vector
        /// is copied into that array when the vector gets destroyed.
        /// This allows calls like classifier.Outputs(v,x); with
        /// floatarray v.
        /// </summary>
        public OutputVector(Floatarray v) : this()
        {
            _result = new Floatarray();
            _result.Copy(v);
            v.Clear();
        }

        #region Sparse vector access.

        public void Clear()
        {
            _keys.Fill(0);
            _keys.Clear();
            _values.Fill(0f);
            _values.Clear();
            _len = 0;
        }

        public int nKeys()
        {
            return _keys.Length();
        }

        public int Key(int i)
        {
            return _keys.At1d(i);
        }

        public float Value(int i)
        {
            return _values.At1d(i);
        }

        public Intarray Keys
        {
            get { return _keys; }
        }

        public Floatarray Values
        {
            get { return _values; }
        }

        public int BestIndex
        {
            get { return MaxIndex; }
        }

        public int MaxIndex
        {
            get { return NarrayUtil.ArgMax(_values); }
        }

        public int MinIndex
        {
            get { return NarrayUtil.ArgMin(_values); }
        }

        #endregion // Sparse vector access.

        #region Dense vector conversions and access.

        public void Init(int n = 0)
        {
            _keys.Resize(n);
            for (int i = 0; i < n; i++)
                _keys.Put1d(i, i);
            _values.Resize(n);
            _values.Fill<float>(0.0f);
        }

        public void Copy(Floatarray v, float eps = 1e-11f)
        {
            Clear();
            int n = v.Length();
            for (int i = 0; i < n; i++)
            {
                float value = v.At1d(i);
                if (Math.Abs(value) >= eps)
                {
                    _keys.Push(i);
                    _values.Push(value);
                }
            }
            _len = v.Length();
            _keys.Resize(_len);
            for (int i = 0; i < _len; i++)
                _keys.Put1d(i, i);
            _values.Copy(v);
        }

        public int Length()
        {
            return _len;
        }

        public float this[int index]
        {
            get
            {
                for (int j = 0; j < _keys.Length(); j++)
                    if (_keys.UnsafeAt1d(j) == index)
                        return _values[j];
                _keys.Push(index);
                _values.Push(0f);
                if (index >= _len) _len = index + 1;
                return _values.Last();
            }
            set
            {
                for (int j = 0; j < _keys.Length(); j++)
                    if (_keys.UnsafeAt1d(j) == index)
                    {
                        _values[j] = value;
                        return;
                    }
                _keys.Push(index);
                _values.Push(value);
                if (index >= _len) _len = index + 1;
            }
        }

        public Floatarray AsArray()
        {
            Floatarray result = new Floatarray();
            result.Resize(Length());
            result.Fill(0f);
            for (int i = 0; i < _keys.Length(); i++)
                result.UnsafePut1d(_keys.UnsafeAt1d(i), _values.UnsafeAt1d(i));
            return result;
        }

        #endregion // Dense vector conversions and access.

        #region Some common operators.

        public static OutputVector operator /(OutputVector outvector, float val)
        {
            OutputVector res = outvector;
            res._values = res._values / val;
            return res;
        }

        public float Sum()
        {
            float total = 0.0f;
            for (int i = 0; i < _values.Length(); i++)
                total += _values.UnsafeAt1d(i);
            return total;
        }

        public void Normalize()
        {
            _values = _values / Sum();
        }

        public void Normalize2()
        {
            float min = Math.Min(0, Min());
            float max = Math.Max(1, Max());
            float diff = Math.Abs(min - max);
            _values = _values - min;
            _values = _values / diff;
        }

        public int ArgMax()
        {
            return NarrayUtil.ArgMax(_values);
        }

        public float Max()
        {
            return NarrayUtil.Max(_values);
        }

        public int ArgMin()
        {
            return NarrayUtil.ArgMin(_values);
        }

        public float Min()
        {
            return NarrayUtil.Min(_values);
        }

        #endregion // Some common operators.
    }
}
