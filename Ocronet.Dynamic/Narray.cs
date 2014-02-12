using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ocronet.Dynamic
{
    using index_t = System.Int32;

    public class Narray<T> : IDisposable
    {
        // a pointer to the actual data being held
        public T[] data;
        // the total number of elements held by the pointer
        private index_t allocated;
        // the total number of elements that are currently
        // considered accessible / initialized
        private index_t total;
        // the individual dimensions of the array
        private index_t[] dims = new index_t[5];

        #region Static Routines
        public static void swap_<S>(ref S a, ref S b) { S t = a; a = b; b = t; }
        public static void na_transfer(ref T dst, ref T src) { dst = src; }
        public static void na_transfer(Narray<T> dest, Narray<T> src) { dest.Move(src); }

        /// <summary>
        /// check that i is in [0,n-1]
        /// </summary>
        protected static void check_range(index_t i, index_t n)
        {
            unchecked
            {
                if ((uint)(i) >= (uint)(n)) throw new Exception("narray: index out of range");
            }
        }

        /// <summary>
        /// check the given condition to be true and throw message as the
        /// exception if it is not
        /// </summary>
        static void check(bool condition, string message)
        {
            if (!condition) throw new Exception(message);
        }


        static double growth_factor() { return 1.5; }

        static index_t roundup_(index_t i) {
            index_t v = 1;
            while(v <= i) {
                v = (index_t)(v*growth_factor())+1;
            }
            return Math.Max(v, 10);
        }
        #endregion

        #region Class Routiness
        /// <summary>
        /// check that the array is of rank 1
        /// </summary>
        void check_rank1()
        {
            check(dims[1] == 0, "attempt to use narray list operation with rank!=1");
        }

        /// <summary>
        /// compute the total number of elements in an array
        /// of the given dimensions
        /// </summary>
        /// <param name="d0">size of 0 dimension</param>
        /// <param name="d1">size of 1 dimension</param>
        /// <param name="d2">size of 2 dimension</param>
        /// <param name="d3">size of 3 dimension</param>
        /// <returns></returns>
        private index_t total_(index_t d0, index_t d1 = 0, index_t d2 = 0, index_t d3 = 0)
        {
            return d0 * (d1 > 0 ? d1 : 1) * (d2 > 0 ? d2 : 1) * (d3 > 0 ? d3 : 1);
        }

        /// <summary>
        /// change the elements of the array
        /// </summary>
        public void SetDims(index_t d0, index_t d1 = 0, index_t d2 = 0, index_t d3 = 0)
        {
            total = total_(d0, d1, d2, d3);
            dims[0] = d0; dims[1] = d1; dims[2] = d2; dims[3] = d3; dims[4] = 0;
            check(total <= allocated, "bad SetDims (internal error)");
        }

        /// <summary>
        /// allocate the elements of the array
        /// </summary>
        private void alloc_(index_t d0, index_t d1 = 0, index_t d2 = 0, index_t d3 = 0)
        {
            total = total_(d0, d1, d2, d3);
            data = new T[total];
            allocated = total;
            dims[0] = d0; dims[1] = d1; dims[2] = d2; dims[3] = d3; dims[4] = 0;
        }

        public void Dealloc()
        {
            if (data != null)
            {
                data = null;
                //GC.Collect();
            }
            dims[0] = 0;
            total = 0;
            allocated = 0;
        }
        #endregion

        #region Constructors and Destructor
        /// <summary>
        /// Constructor. Creates a rank-0, empty array.
        /// </summary>
        public Narray()
        {
            data = null;
            for (int i = 0; i < 5; i++) dims[i] = 0;
            total = 0;
            allocated = 0;
        }

        /// <summary>
        /// Constructor. Creates a rank 1 array with dimensions d0.
        /// </summary>
        /// <param name="d0">size of dimension 0</param>
        public Narray(index_t d0)
        {
            alloc_(d0);
        }

        /// <summary>
        /// Constructor. Creates a rank 2 array with dimensions d0 and d1.
        /// </summary>
        /// <param name="d0">size of dimension 0</param>
        /// <param name="d1">size of dimension 1</param>
        public Narray(index_t d0, index_t d1)
        {
            alloc_(d0, d1);
        }

        /// <summary>
        /// Constructor. Creates a rank 3 array with dimensions d0, d1, and d2.
        /// </summary>
        public Narray(index_t d0, index_t d1, index_t d2)
        {
            alloc_(d0, d1, d2);
        }

        /// <summary>
        /// Constructor. Creates a rank 4 array with dimensions d0, d1, d2 and d3.
        /// </summary>
        public Narray(index_t d0, index_t d1, index_t d2, index_t d3)
        {
            alloc_(d0, d1, d2, d3);
        }

        public void Dispose()
        {
            Dealloc();
        }

        #endregion // Constructors

        public virtual string Name
        {
            get { return "Narray<" + typeof(T).Name + ">"; }
        }

        public override string ToString()
        {
            return String.Format("{0} {1} {2} {3} {4}", this.Name, Dim(0), Dim(1), Dim(2), Dim(3));
        }

        public virtual T[] To1DArray()
        {
            T[] result = new T[this.Length1d()];
            for (int i = 0; i < this.Length1d(); i++)
                result[i] = this.data[i];
            return result;
        }

        public virtual S[] To1DArray<S>()
        {
            S[] result = new S[this.Length1d()];
            for (int i = 0; i < this.Length1d(); i++)
                result[i] = (S)Convert.ChangeType(this.data[i], typeof(S));
            return result;
        }

        /// <summary>
        /// Truncates the array.
        /// </summary>
        /// <param name="d0">smaller size for 0 dimension</param>
        /// <returns>this instance</returns>
        public Narray<T> Truncate(index_t d0)
        {
            check(d0<=dims[0] && dims[1]==0, "can only truncate 1D arrays to smaller arrays");
            SetDims(d0);
            return this;
        }

        /// <summary>
        /// Resizes the array, possibly destroying any data previously held by it.
        /// </summary>
        public Narray<T> Resize(index_t d0,index_t d1=0,index_t d2=0,index_t d3=0)
        {
            index_t ntotal = total_(d0,d1,d2,d3);
            if(ntotal > total) {
                data = null;
                alloc_(d0,d1,d2,d3);
            } else {
                SetDims(d0,d1,d2,d3);
            }
            return this;
        }

        /// <summary>
        /// Resizes the array to the given size; this is guaranteed to reallocate
        /// the storage fresh.
        /// </summary>
        public Narray<T> Renew(index_t d0, index_t d1 = 0, index_t d2 = 0, index_t d3 = 0)
        {
            Dealloc();
            Resize(d0,d1,d2,d3);
            return this;
        }

        /// <summary>
        /// Reshapes the array; the new shape must have the same number of elements as before.
        /// </summary>
        public Narray<T> Reshape(index_t d0, index_t d1 = 0, index_t d2 = 0, index_t d3 = 0)
        {
            index_t ntotal = total_(d0,d1,d2,d3);
            check(ntotal==total, "narray: bad reshape");
            dims[0] = d0; dims[1] = d1; dims[2] = d2; dims[3] = d3; dims[4] = 0;
            return this;
        }

        /// <summary>
        /// Determine the rank of the array.
        /// </summary>
        public int Rank()
        {
            for(int i=1;i<=4;i++)
                if(dims[i]==0) return i;
            return 0;
        }

        /// <summary>
        /// Determine the range of valid index for index number i.
        /// </summary>
        /// <param name="i">index dimension</param>
        /// <returns>range of valid index dimension</returns>
        public index_t Dim(int i)
        {
            check_range(i,4);
            return dims[i];
        }


        #region Indexers, operators

        /// <summary>
        /// 1D subscripting.
        /// </summary>
        public T this[index_t i0]
		{
			get
            {
                check(dims[1] == 0, "narray: bad rank");
                check_range(i0, dims[0]);
                return data[i0]; 
            }
			set
			{
                check(dims[1] == 0, "narray: bad rank");
                check_range(i0, dims[0]);
                data[i0] = value;								
			}
		}

        /// <summary>
        /// 2D subscripting.
        /// </summary>
        public T this[index_t i0, index_t i1]
        {
            get
            {
                check(dims[2] == 0, "narray: bad rank");
                check_range(i0, dims[0]);
                check_range(i1, dims[1]);
                return data[i1 + i0 * dims[1]];
            }
            set
            {
                check(dims[2] == 0, "narray: bad rank");
                check_range(i0, dims[0]);
                check_range(i1, dims[1]);
                data[i1 + i0 * dims[1]] = value;
            }
        }

        /// <summary>
        /// 3D subscripting.
        /// </summary>
        public T this[index_t i0, index_t i1, index_t i2]
        {
            get
            {
                check(dims[3] == 0, "narray: bad rank");
                check_range(i0, dims[0]);
                check_range(i1, dims[1]);
                check_range(i2, dims[2]);
                return data[(i1 + i0 * dims[1]) * dims[2] + i2];
            }
            set
            {
                check(dims[3] == 0, "narray: bad rank");
                check_range(i0, dims[0]);
                check_range(i1, dims[1]);
                check_range(i2, dims[2]);
                data[(i1 + i0 * dims[1]) * dims[2] + i2] = value;
            }
        }

        /// <summary>
        /// 4D subscripting.
        /// </summary>
        public T this[index_t i0, index_t i1, index_t i2, index_t i3]
        {
            get
            {
                check_range(i0, dims[0]);
                check_range(i1, dims[1]);
                check_range(i2, dims[2]);
                check_range(i3, dims[3]);
                return data[((i1 + i0 * dims[1]) * dims[2] + i2) * dims[3] + i3];
            }
            set
            {
                check_range(i0, dims[0]);
                check_range(i1, dims[1]);
                check_range(i2, dims[2]);
                check_range(i3, dims[3]);
                data[((i1 + i0 * dims[1]) * dims[2] + i2) * dims[3] + i3] = value;
            }
        }

        // Methods provided for easier binding to scripting languages and
        // for references with operator->  We're just doing this for the
        // most common case.

        public T At(index_t i0) { return this[i0]; }
        public T At(index_t i0, index_t i1) { return this[i0, i1]; }
        public T At(index_t i0, index_t i1, index_t i2) { return this[i0, i1, i2]; }
        public T At(index_t i0, index_t i1, index_t i2, index_t i3) { return this[i0, i1, i2, i3]; }

        public void Put(index_t i0, T value) { this[i0] = value; }
        public void Put(index_t i0, index_t i1, T value) { this[i0, i1] = value; }
        public void Put(index_t i0, index_t i1, index_t i2, T value) { this[i0, i1, i2] = value; }
        public void Put(index_t i0, index_t i1, index_t i2, index_t i3, T value) { this[i0, i1, i2, i3] = value; }

        // Unsafe subscriptings.

        public T UnsafeAt(index_t i0) { return data[i0]; }
        public void UnsafePut(index_t i0, T value) { data[i0] = value; }
        public T UnsafeAt(index_t i0, index_t i1) { return data[i1 + i0 * dims[1]]; }
        public void UnsafePut(index_t i0, index_t i1, T value) { data[i1 + i0 * dims[1]] = value; }
        public T UnsafeAt(index_t i0, index_t i1, index_t i2) { return data[(i1 + i0 * dims[1]) * dims[2] + i2]; }
        public void UnsafePut(index_t i0, index_t i1, index_t i2, T value) { data[(i1 + i0 * dims[1]) * dims[2] + i2] = value; }
        public T UnsafeAt(index_t i0, index_t i1, index_t i2, index_t i3) { return data[((i1 + i0 * dims[1]) * dims[2] + i2) * dims[3] + i3]; }
        public void UnsafePut(index_t i0, index_t i1, index_t i2, index_t i3, T value) { data[((i1 + i0 * dims[1]) * dims[2] + i2) * dims[3] + i3] = value; }

        /// <summary>
        /// 1D subscripting (works for arrays of any rank).
        /// </summary>
        public T At1d(index_t i) { check_range(i, total); return data[i]; }
        /// <summary>
        /// 1D subscripting (works for arrays of any rank).
        /// </summary>
        public void Put1d(index_t i, T value) { check_range(i, total); data[i] = value;  }

        /// <summary>
        /// Unsafe 1D subscripting (works for arrays of any rank).
        /// </summary>
        public T UnsafeAt1d(index_t i) { return data[i]; }
        public void UnsafePut1d(index_t i, T value) { data[i] = value; }
        public void UnsafePut1d<S>(index_t i, S value) { data[i] = (T)Convert.ChangeType(value, typeof(T)); }

        #endregion // Indexers, operators


        /// <summary>
        /// Length of the array, viewed as a 1D array.
        /// </summary>
        public index_t Length()
        {
            return total;
        }
        /// <summary>
        /// Length of the array, viewed as a 1D array.
        /// </summary>
        public index_t Length1d()
        {
            return total;
        }

        /// <summary>
        /// Initializing/setting the value.
        /// </summary>
        /// <param name="value">value of same type T</param>
        public void Fill(T value)
        {
            for (index_t i = 0, n = Length1d(); i < n; i++)
                UnsafePut1d(i, value);
        }

        /// <summary>
        /// Initializing/setting the value.
        /// </summary>
        /// <typeparam name="S">other type S</typeparam>
        /// <param name="value">value of other type S</param>
        public void Fill<S>(S value)
        {
            for (index_t i = 0,n = Length1d(); i < n; i++)
                UnsafePut1d(i, value);
        }

        /// <summary>
        /// Make sure that the array has allocated room for at least
        /// n more elements.  However, these additional elements may
        /// not be accessible until the dimensions are changed
        /// (e.g., through push).
        /// </summary>
        public void Reserve(index_t n)
        {
            index_t nallocated = total + n;
            if (nallocated <= allocated) return;
            nallocated = roundup_(nallocated);
            T[] ndata = new T[nallocated];
            for (index_t i = 0; i < total; i++)
            {
                //na_transfer(ref ndata[i], ref data[i]);
                ndata[i] = data[i];
            }
            data = ndata;
            allocated = nallocated;
        }

        /// <summary>
        /// Make sure that the array is a 1D array capable of holding at
        /// least n elements.  This preserves existing data.
        /// </summary>
        public void GrowTo(index_t n)
        {
            check_rank1();
            if (n > allocated) Reserve(n - total);
            total = dims[0] = n;
        }

        public void ReserveTo(index_t n)
        {
            check_rank1();
            if (n > allocated) Reserve(n - total);
        }

        /// <summary>
        /// Append an element to a rank-1 array.
        /// </summary>
        public T Push(T value)
        {
            check_rank1();
            Reserve(1);
            data[dims[0]] = value;
            dims[0] += 1;
            total = dims[0];
            return value;
        }
        public T Push()
        {
            check_rank1();
            Reserve(1);
            dims[0] += 1;
            total = dims[0];
            return data[dims[0]-1];
        }
        public void Push<S>(S value)
        {
            Push((T)Convert.ChangeType(value, typeof(T)));
        }

        /// <summary>
        /// Remove the last element of a rank-1 array (returns a reference).
        /// </summary>
        public T Pop()
        {
            check_rank1();
            check(dims[0] > 0, "pop of empty list");
            dims[0] -= 1;
            T result = data[dims[0]];
            total = dims[0];
            return result;
        }

        /// <summary>
        /// Return a reference to the last element of a rank-1 array.
        /// </summary>
        public T Last()
        {
            check_rank1();
            check(dims[0]>0, "pop of empty list");
            return data[dims[0]-1];
        }

        /// <summary>
        /// Make the array empty, but don't deallocate the storage held by it.
        /// The clear() method is more efficient if you expect you will be reusing the
        /// storage in a loop.
        /// If you want to deallocate the storage, call Dealloc().
        /// </summary>
        public void Clear()
        {
            dims[0] = 0;
            dims[1] = 0;
            dims[2] = 0;
            total = 0;
        }

        /// <summary>
        /// Set value at start position. Resize array to 1.
        /// </summary>
        public void Set(T v0)
        {
            Resize(1);
            Put(0, v0);
        }

        /// <summary>
        /// Set values at 0 and 1 positions. Resize array to 2.
        /// </summary>
        public void Set(T v0, T v1)
        {
            Resize(2);
            Put(0, v0);
            Put(1, v1);
        }
        public void Set(T v0, T v1, T v2)
        {
            Resize(3);
            Put(0, v0);
            Put(1, v1);
            Put(2, v2);
        }
        public void Set(T v0, T v1, T v2, T v3)
        {
            Resize(4);
            Put(0, v0);
            Put(1, v1);
            Put(2, v2);
            Put(3, v3);
        }


        /// <summary>
        /// Take the data held by the src array and put it into the dest array.
        /// The src array is made empty in the proceess.  This is an O(1) operation.
        /// </summary>
        public void Move(Narray<T> src)
        {
            this.data = src.data;
            for (int i = 0; i < 5; i++) this.dims[i] = src.dims[i];
            this.total = src.total;
            this.allocated = src.allocated;
            src.Dealloc();
        }

        /// <summary>
        /// Swap the contents of the two arrays.
        /// </summary>
        public void Swap(Narray<T> src)
        {
            swap_<T[]>(ref this.data, ref src.data);
            for (int i = 0; i < 5; i++) swap_(ref this.dims[i], ref src.dims[i]);
            swap_(ref this.total, ref src.total);
            swap_(ref this.allocated, ref src.allocated);
        }

        /// <summary>
        /// Copy the elements of the source array into the destination array,
        /// resizing if necessary.
        /// </summary>
        public void Copy(Narray<T> src)
        {
            this.Resize(src.Dim(0), src.Dim(1), src.Dim(2), src.Dim(3));
            index_t n = this.Length1d();
            for (index_t i = 0; i < n; i++)
                this.UnsafePut1d(i, src.UnsafeAt1d(i));
        }

        /// <summary>
        /// Copy the elements of the source array into the destination array,
        /// resizing if necessary.
        /// </summary>
        public void Copy<S>(Narray<S> src)
        {
            this.Resize(src.Dim(0), src.Dim(1), src.Dim(2), src.Dim(3));
            index_t n = this.Length1d();
            for (index_t i = 0; i < n; i++)
                this.UnsafePut1d(i, src.UnsafeAt1d(i));
        }

        /// <summary>
        /// Copy the elements of the source array into the destination array,
        /// resizing if necessary.
        /// </summary>
        public void Append(Narray<T> src)
        {
            check_rank1();
            Reserve(src.Length());
            for (index_t i = 0; i < src.Length(); i++)
            {
                //Push(src.unsafe_at1d(i));
                data[dims[0]] = src.UnsafeAt1d(i);
                dims[0] += 1;
                total = dims[0];
            }
        }
        public void Append<S>(Narray<S> src)
        {
            check_rank1();
            Reserve(src.Length());
            for (index_t i = 0; i < src.Length(); i++)
            {
                var value = src.UnsafeAt1d(i);
                data[dims[0]] = (T)Convert.ChangeType(value, typeof(T));
                dims[0] += 1;
                total = dims[0];
            }
        }

        /// <summary>
        /// Check whether two narrays have the same rank and sizes.
        /// </summary>
        public bool SameDims<S>(Narray<S> b)
        {
            if (this.Rank() != b.Rank()) return false;
            for (int i = 0; i < this.Rank(); i++)
                if (this.Dim(i) != b.Dim(i)) return false;
            return true;
        }

        /// <summary>
        /// Make the first array have the same dimensions as the second array.
        /// </summary>
        public Narray<T> MakeLike<S>(Narray<S> b)
        {
            if (SameDims(b))
                return this;
            Narray<T> a = this;
            int r = b.Rank();
            switch(r) {
            case 0:
                a.Dealloc();
                break;
            case 1:
                a.Resize(b.Dim(0));
                break;
            case 2:
                a.Resize(b.Dim(0), b.Dim(1));
                break;
            case 3:
                a.Resize(b.Dim(0), b.Dim(1), b.Dim(2));
                break;
            case 4:
                a.Resize(b.Dim(0), b.Dim(1), b.Dim(2), b.Dim(3));
                break;
            default:
                throw new Exception("bad rank");
            }
            return this;
        }
        public Narray<T> MakeLike<S>(Narray<S> b, T value)
        {
            this.MakeLike(b);
            this.Fill(value);
            return this;
        }

        /// <summary>
        /// Check whether two narrays are equal (mostly for testing).
        /// </summary>
        public bool Equal(Narray<T> b)
        {
            if (this.Rank() != b.Rank()) return false;
            for (int i = 0; i < this.Rank(); i++) if (this.Dim(i) != b.Dim(i)) return false;
            index_t n = this.Length1d();
            for (index_t i = 0; i < n; i++) if (!this.UnsafeAt1d(i).Equals(b.UnsafeAt1d(i))) return false;
            return true;
        }

        public bool IsEmpty()
        {
            return (data == null || total == 0 || dims[0] == 0);
        }

        // use the methods instead (fewer name conflicts)
        public static void Move(Narray<T> dest, Narray<T> src) { dest.Move(src); }
        public static void Swap(Narray<T> dest, Narray<T> src) { dest.Swap(src); }
        public static void Copy<S>(Narray<T> dest, Narray<S> src) { dest.Copy(src); }
        public static bool SameDims<S>(Narray<T> a, Narray<S> b) { return a.SameDims(b); }
        public static void MakeLike<S>(Narray<T> a, Narray<S> b) { a.MakeLike(b); }
        public static bool Equal(Narray<T> a, Narray<T> b) { return a.Equal(b); }
        public static void Fill<S>(Narray<T> a, S value) { a.Fill(value); }
    }

    /// <summary>
    /// Narray&lt;byte&gt;
    /// </summary>
    public class Bytearray : Narray<byte>
    {
        public Bytearray() : base() { }
        public Bytearray(index_t d0) : base(d0) { }
        public Bytearray(index_t d0, index_t d1) : base(d0, d1) { }
        public Bytearray(index_t d0, index_t d1, index_t d2) : base(d0, d1, d2) { }
        public override string Name
        {
            get { return "Bytearray"; }
        }
    }

    /// <summary>
    /// Narray&lt;short&gt;
    /// </summary>
    public class Shortarray : Narray<short>
    {
        public Shortarray() : base() { }
        public Shortarray(index_t d0) : base(d0) { }
        public override string Name
        {
            get { return "Shortarray"; }
        }
    }

    /// <summary>
    /// Narray&lt;int&gt;
    /// </summary>
    public class Intarray : Narray<int>
    {
        public Intarray() : base() { }
        public Intarray(index_t d0) : base(d0) { }
        public Intarray(index_t d0, index_t d1) : base(d0, d1) { }
        public Intarray(index_t d0, index_t d1, index_t d2) : base(d0, d1, d2) { }
        public override string Name
        {
            get { return "Intarray"; }
        }

        #region operators
        public static Intarray operator *(Intarray array, int val)
        {
            Intarray res = array;
            check_range(res.Length1d()-1, array.Length1d());
            for (int i = 0; i < array.Length1d(); i++)
                res.UnsafePut1d(i, array.UnsafeAt1d(i) * val);
            return res;
        }

        public static Intarray operator +(Intarray array, int val)
        {
            Intarray res = array;
            for (int i = 0; i < array.Length1d(); i++)
                res.UnsafePut1d(i, array.UnsafeAt1d(i) + val);
            return res;
        }

        public static Intarray operator +(Intarray outarray1, Intarray array2)
        {
            if (!SameDims(outarray1, array2))
                throw new Exception("outarray1 and array2 must be same dims!");
            Intarray res = outarray1;
            for (int i = 0; i < outarray1.Length1d(); i++)
                res.UnsafePut1d(i, outarray1.UnsafeAt1d(i) + array2.UnsafeAt1d(i));
            return res;
        }

        public static Intarray operator -(Intarray array, int val)
        {
            Intarray res = array;
            for (int i = 0; i < array.Length1d(); i++)
                res.UnsafePut1d(i, array.UnsafeAt1d(i) - val);
            return res;
        }

        public static Intarray operator -(Intarray outarray1, Intarray array2)
        {
            if (!SameDims(outarray1, array2))
                throw new Exception("outarray1 and array2 must be same dims!");
            Intarray res = outarray1;
            for (int i = 0; i < outarray1.Length1d(); i++)
                res.UnsafePut1d(i, outarray1.UnsafeAt1d(i) - array2.UnsafeAt1d(i));
            return res;
        }

        public static Intarray operator /(Intarray array, int val)
        {
            Intarray res = array;
            for (int i = 0; i < array.Length1d(); i++)
                res.UnsafePut1d(i, array.UnsafeAt1d(i) / val);
            return res;
        }
        #endregion
    }

    /// <summary>
    /// Narray&lt;float&gt;
    /// </summary>
    public class Floatarray : Narray<float>
    {
        public Floatarray() : base() { }
        public Floatarray(index_t d0) : base(d0) { }
        public override string Name
        {
            get { return "Floatarray"; }
        }

        #region operators
        public static Floatarray operator *(Floatarray array, float val)
        {
            Floatarray res = array;
            for (int i = 0; i < array.Length1d(); i++)
                res.UnsafePut1d(i, array.UnsafeAt1d(i) * val);
            return res;
        }

        public static Floatarray operator +(Floatarray array, float val)
        {
            Floatarray res = array;
            for (int i = 0; i < array.Length1d(); i++)
                res.UnsafePut1d(i, array.UnsafeAt1d(i) + val);
            return res;
        }

        public static Floatarray operator +(Floatarray outarray1, Floatarray array2)
        {
            if (!SameDims(outarray1, array2))
                throw new Exception("outarray1 and array2 must be same dims!");
            Floatarray res = outarray1;
            for (int i = 0; i < outarray1.Length1d(); i++)
                res.UnsafePut1d(i, outarray1.UnsafeAt1d(i) + array2.UnsafeAt1d(i));
            return res;
        }

        public static Floatarray operator -(Floatarray array, float val)
        {
            Floatarray res = array;
            for (int i = 0; i < array.Length1d(); i++)
                res.UnsafePut1d(i, array.UnsafeAt1d(i) - val);
            return res;
        }

        public static Floatarray operator -(Floatarray outarray1, Floatarray array2)
        {
            if (!SameDims(outarray1, array2))
                throw new Exception("outarray1 and array2 must be same dims!");
            Floatarray res = outarray1;
            for (int i = 0; i < outarray1.Length1d(); i++)
                res.UnsafePut1d(i, outarray1.UnsafeAt1d(i) - array2.UnsafeAt1d(i));
            return res;
        }

        public static Floatarray operator /(Floatarray array, float val)
        {
            Floatarray res = array;
            for (int i = 0; i < array.Length1d(); i++)
                res.UnsafePut1d(i, array.UnsafeAt1d(i) / val);
            return res;
        }
        public static Floatarray operator /(Floatarray array, double val)
        {
            Floatarray res = array;
            for (int i = 0; i < array.Length1d(); i++)
                res.UnsafePut1d(i, (float)(array.UnsafeAt1d(i) / val));
            return res;
        }
        #endregion
    }

    /// <summary>
    /// Narray&lt;double&gt;
    /// </summary>
    public class Doublearray : Narray<double>
    {
        public Doublearray() : base() { }
        public Doublearray(index_t d0) : base(d0) { }
        public override string Name
        {
            get { return "Doublearray"; }
        }

        #region operators
        public static Doublearray operator *(Doublearray array, double val)
        {
            Doublearray res = array;
            for (int i = 0; i < array.Length1d(); i++)
                res.UnsafePut1d(i, array.UnsafeAt1d(i) * val);
            return res;
        }

        public static Doublearray operator +(Doublearray array, double val)
        {
            Doublearray res = array;
            for (int i = 0; i < array.Length1d(); i++)
                res.UnsafePut1d(i, array.UnsafeAt1d(i) + val);
            return res;
        }

        public static Doublearray operator -(Doublearray array, double val)
        {
            Doublearray res = array;
            for (int i = 0; i < array.Length1d(); i++)
                res.UnsafePut1d(i, array.UnsafeAt1d(i) - val);
            return res;
        }

        public static Doublearray operator /(Doublearray array, double val)
        {
            Doublearray res = array;
            for (int i = 0; i < array.Length1d(); i++)
                res.UnsafePut1d(i, array.UnsafeAt1d(i) / val);
            return res;
        }
        #endregion
    }

    /// <summary>
    /// Narray&lt;long&gt;
    /// </summary>
    public class Longarray : Narray<long>
    {
        public Longarray() : base() { }
        public Longarray(index_t d0) : base(d0) { }
        public override string Name
        {
            get { return "Longarray"; }
        }

        #region operators
        public static Longarray operator *(Longarray array, long val)
        {
            Longarray res = array;
            for (int i = 0; i < array.Length1d(); i++)
                res.UnsafePut1d(i, array.UnsafeAt1d(i) * val);
            return res;
        }

        public static Longarray operator +(Longarray array, long val)
        {
            Longarray res = array;
            for (int i = 0; i < array.Length1d(); i++)
                res.UnsafePut1d(i, array.UnsafeAt1d(i) + val);
            return res;
        }

        public static Longarray operator -(Longarray array, long val)
        {
            Longarray res = array;
            for (int i = 0; i < array.Length1d(); i++)
                res.UnsafePut1d(i, array.UnsafeAt1d(i) - val);
            return res;
        }

        public static Longarray operator /(Longarray array, long val)
        {
            Longarray res = array;
            for (int i = 0; i < array.Length1d(); i++)
                res.UnsafePut1d(i, array.UnsafeAt1d(i) / val);
            return res;
        }
        #endregion
    }

    /// <summary>
    /// Narray&lt;Rect&gt;
    /// </summary>
    public class Rectarray : Narray<Rect>
    {
        public Rectarray() : base() { }
        public Rectarray(index_t d0) : base(d0) { }
        public override string Name
        {
            get { return "Rectarray"; }
        }
    }
}
