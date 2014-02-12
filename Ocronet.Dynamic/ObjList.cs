using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ocronet.Dynamic
{
    public class ObjList<T>
    {
        private Narray<T> data;

        public ObjList()
        {
            data = new Narray<T>();
        }
        public ObjList(int n)
        {
            data = new Narray<T>(n);
        }

        public void ReserveTo(int n)
        {
            data.ReserveTo(n);
        }

        public int Length()
        {
            return data.Length();
        }

        public int Dim(int d)
        {
            if (d != 0)
                throw new Exception("ObjList: rank must be 0");
            return data.Dim(d);
        }

        public T Push(T val)
        {
            return data.Push(val);
        }

        public T Push()
        {
            return data.Push();
        }

        public T Pop()
        {
            return data.Pop();
        }

        public T Last()
        {
            return data.Last();
        }

        /// <summary>
        /// 1D subscripting.
        /// </summary>
        public T this[int i0]
        {
            get
            {
                return data.At1d(i0);
            }
            set
            {
                data.Put1d(i0, value);
            }
        }

        public void Set(int i0, T val)
        {
            data[i0] = val;
        }

        public void Move(ObjList<T> other)
        {
            data.Move(other.data);
        }

        public void Resize(int n)
        {
            data.Dealloc();
            data.Resize(n);
        }

        public void Clear()
        {
            data.Clear();
        }

        public void Dealloc()
        {
            data.Dealloc();
        }

    }
}
