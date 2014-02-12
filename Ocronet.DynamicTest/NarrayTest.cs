using Ocronet.Dynamic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ocronet.DynamicTest
{
    
    
    /// <summary>
    ///This is a test class for NarrayTest and is intended
    ///to contain all NarrayTest Unit Tests
    ///</summary>
    [TestClass]
    public class NarrayTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for Narray`1 Constructor for Empty array.
        ///</summary>
        public void NarrayConstructorTestHelper<T>()
        {
            var target = new Narray<T>();
            Assert.AreEqual(1, target.Rank(), "Rank must be 1");
            Assert.AreEqual(0, target.Length(), "Length must be 0");
            Assert.AreEqual(0, target.Length1d(), "Length1d must be 0");
            Assert.AreEqual(0, target.Dim(0));
            Assert.AreEqual(0, target.Dim(1));
            Assert.AreEqual(0, target.Dim(2));
            Assert.AreEqual(0, target.Dim(3));
            Assert.IsTrue(target.IsEmpty());
        }

        [TestMethod()]
        public void NarrayConstructorTest()
        {
            NarrayConstructorTestHelper<byte>();
            NarrayConstructorTestHelper<int>();
            NarrayConstructorTestHelper<float>();
        }

        /// <summary>
        ///A test for Narray`1 Constructor for 1d array.
        ///</summary>
        public void NarrayConstructorTest1Helper<T>()
        {
            const int d0 = 121;
            var value = (T)Convert.ChangeType(99, typeof(T));
            var target = new Narray<T>(d0);
            Assert.AreEqual(1, target.Rank(), "Rank must be 1");
            Assert.AreEqual(d0, target.Length(), "Length must be " + d0);
            Assert.AreEqual(d0, target.Length1d(), "Length1d must be " + d0);
            Assert.AreEqual(d0, target.Dim(0));
            Assert.AreEqual(0, target.Dim(1));
            Assert.AreEqual(0, target.Dim(2));
            Assert.AreEqual(0, target.Dim(3));
            Assert.AreEqual(default(T), target.At(0));
            target.Put(10, value);
            Assert.AreEqual(value, target.At(10));
        }

        [TestMethod()]
        public void NarrayConstructorTest1()
        {
            NarrayConstructorTest1Helper<byte>();
            NarrayConstructorTest1Helper<int>();
            NarrayConstructorTest1Helper<float>();
        }

        /// <summary>
        ///A test for Narray`1 Constructor
        ///</summary>
        public void NarrayConstructorTest2Helper<T>()
        {
            const int d0 = 10;
            const int d1 = 12;
            var target = new Narray<T>(d0, d1);
            Assert.AreEqual(2, target.Rank(), "Rank must be 2");
            Assert.AreEqual((d0 * d1), target.Length(), "Length must be " + (d0 * d1));
            Assert.AreEqual((d0 * d1), target.Length1d(), "Length1d must be " + (d0 * d1));
            Assert.AreEqual(d0, target.Dim(0));
            Assert.AreEqual(d1, target.Dim(1));
            Assert.AreEqual(0, target.Dim(2));
            Assert.AreEqual(0, target.Dim(3));
        }

        [TestMethod()]
        public void NarrayConstructorTest2()
        {
            NarrayConstructorTest2Helper<byte>();
            NarrayConstructorTest2Helper<int>();
            NarrayConstructorTest2Helper<float>();
        }

        /// <summary>
        ///A test for Narray`1 Constructor
        ///</summary>
        public void NarrayConstructorTest3Helper<T>()
        {
            const int d0 = 11;
            const int d1 = 23;
            const int d2 = 10;
            var target = new Narray<T>(d0, d1, d2);
            Assert.AreEqual(3, target.Rank(), "Rank must be 3");
            Assert.AreEqual((d0 * d1 * d2), target.Length(), "Length must be " + (d0 * d1 * d2));
            Assert.AreEqual((d0 * d1 * d2), target.Length1d(), "Length1d must be " + (d0 * d1 * d2));
            Assert.AreEqual(d0, target.Dim(0));
            Assert.AreEqual(d1, target.Dim(1));
            Assert.AreEqual(d2, target.Dim(2));
            Assert.AreEqual(0, target.Dim(3));
        }

        [TestMethod()]
        public void NarrayConstructorTest3()
        {
            NarrayConstructorTest3Helper<byte>();
            NarrayConstructorTest3Helper<int>();
            NarrayConstructorTest3Helper<float>();
        }

        /// <summary>
        ///A test for Narray`1 Constructor
        ///</summary>
        public void NarrayConstructorTest4Helper<T>()
        {
            const int d0 = 10;
            const int d1 = 20;
            const int d2 = 30;
            const int d3 = 11;
            var target = new Narray<T>(d0, d1, d2, d3);
            Assert.AreEqual(4, target.Rank(), "Rank must be 4");
            Assert.AreEqual((d0 * d1 * d2 * d3), target.Length(), "Length must be " + (d0 * d1 * d2 * d3));
            Assert.AreEqual((d0 * d1 * d2 * d3), target.Length1d(), "Length1d must be " + (d0 * d1 * d2 * d3));
            Assert.AreEqual(d0, target.Dim(0));
            Assert.AreEqual(d1, target.Dim(1));
            Assert.AreEqual(d2, target.Dim(2));
            Assert.AreEqual(d3, target.Dim(3));
        }

        [TestMethod()]
        public void NarrayConstructorTest4()
        {
            NarrayConstructorTest4Helper<byte>();
            NarrayConstructorTest4Helper<int>();
            NarrayConstructorTest4Helper<float>();
        }


        /// <summary>
        ///A test for Append
        ///</summary>
        public void AppendTestHelper<T1, T2>()
        {
            var target = new Narray<T1>(10);
            var src = new Narray<T2>(10);
            var valueT = (T1)Convert.ChangeType(99, typeof(T1));
            var valueS = (T2)Convert.ChangeType(99, typeof(T2));
            src.Put(0, valueS);  // put at position 0
            src.Put(9, valueS);  // put at position 9
            target.Append(src);
            Assert.AreEqual(20, target.Length());   // 10 + 10
            Assert.AreEqual(10, src.Length());      // 10
            Assert.AreEqual(valueS, src.At(0));     // first value
            Assert.AreEqual(valueS, src.At(9));     // last value
            Assert.AreEqual(valueT, target.At(10)); // next after 9
            Assert.AreEqual(valueT, target.At(target.Length() - 1)); // last value
        }

        [TestMethod()]
        public void AppendTest()
        {
            AppendTestHelper<byte, byte>();
            AppendTestHelper<int, byte>();
            AppendTestHelper<byte, int>();
        }

        /// <summary>
        ///A test for Append
        ///</summary>
        public void AppendTest1Helper<T>()
        {
            var target = new Narray<T>(10);
            var src = new Narray<T>(20);
            var valueT1 = (T)Convert.ChangeType(99, typeof(T));
            var valueT2 = (T)Convert.ChangeType(99, typeof(T));
            src.Put(0, valueT2);                 // put at position 0
            src.Put(src.Length() - 1, valueT2);  // put at last position
            target.Append(src);
            Assert.AreEqual(30, target.Length());    // 10 + 20
            Assert.AreEqual(20, src.Length());       // 20
            Assert.AreEqual(valueT2, src.At(0));                        // first value
            Assert.AreEqual(valueT2, src.At(src.Length() - 1));         // last value
            Assert.AreEqual(valueT1, target.At(10));                    // next after 9
            Assert.AreEqual(valueT1, target.At(target.Length() - 1));   // last value
        }

        [TestMethod()]
        public void AppendTest1()
        {
            AppendTest1Helper<byte>();
            AppendTest1Helper<int>();
            AppendTest1Helper<float>();
        }

        /// <summary>
        ///A test for At
        ///</summary>
        public void AtTestHelper<T>()
        {
            var target = new Narray<T>(10, 20, 5, 2);
            int i0 = 2;
            int i1 = 1;
            int i2 = 3;
            int i3 = 1;
            T expected = default(T);
            T value = (T)Convert.ChangeType(109, typeof(T));
            Assert.AreEqual(expected, target.At(i0, i1, i2, i3));
            target.Put(i0, i1, i2, i3, value);
            Assert.AreEqual(value, target.At(i0, i1, i2, i3));
        }

        [TestMethod()]
        public void AtTest()
        {
            AtTestHelper<byte>();
            AtTestHelper<int>();
            AtTestHelper<float>();
        }


        /// <summary>
        ///A test for At1d
        ///</summary>
        public void At1dTestHelper<T>()
        {
            var target = new Narray<T>(120);
            int i = 0;
            T expected = default(T);
            Assert.AreEqual(expected, target.At1d(i));
            Assert.AreEqual(expected, target.At1d(10));
            Assert.AreEqual(expected, target.At1d(100));
        }

        [TestMethod()]
        public void At1dTest()
        {
            At1dTestHelper<GenericParameterHelper>();
        }

        /// <summary>
        ///A test for Clear
        ///</summary>
        public void ClearTestHelper<T>()
        {
            const int sizeD0 = 10;
            var target = new Narray<T>(sizeD0);
            Assert.AreEqual(sizeD0, target.Length());
            Assert.AreEqual(sizeD0, target.Dim(0));
            target.Clear();
            Assert.AreEqual(0, target.Length());
            Assert.AreEqual(0, target.Dim(0));
        }

        [TestMethod()]
        public void ClearTest()
        {
            ClearTestHelper<GenericParameterHelper>();
        }

        /// <summary>
        ///A test for Copy
        ///</summary>
        public void CopyTestHelper<T1, T2>()
        {
            var target = new Narray<T1>(10);
            var src = new Narray<T2>(10);
            T1 valueT = (T1)Convert.ChangeType(99, typeof(T1));
            T2 valueS = (T2)Convert.ChangeType(99, typeof(T2));
            src.Put(0, valueS);  // put at position 0
            src.Put(9, valueS);  // put at position 9
            target.Copy(src);
            Assert.AreEqual(10, target.Length());   // 10
            Assert.AreEqual(10, src.Length());      // 10
            Assert.AreEqual(valueS, src.At(0));     // first value
            Assert.AreEqual(valueS, src.At(9));     // last value
            Assert.AreEqual(valueT, target.At(0));  // first value
            Assert.AreEqual(valueT, target.At(9));  // last value
        }

        [TestMethod()]
        public void CopyTest()
        {
            CopyTestHelper<byte, byte>();
            CopyTestHelper<int, byte>();
            CopyTestHelper<byte, int>();
        }


        /// <summary>
        ///A test for Dim
        ///</summary>
        public void DimTestHelper<T>()
        {
            var target = new Narray<T>(10, 11, 22);
            Assert.AreEqual(10, target.Dim(0));
            Assert.AreEqual(11, target.Dim(1));
            Assert.AreEqual(22, target.Dim(2));
        }

        [TestMethod()]
        public void DimTest()
        {
            DimTestHelper<GenericParameterHelper>();
        }


        /// <summary>
        ///A test for Equal
        ///</summary>
        public void EqualTestHelper<T>()
        {
            var target = new Narray<T>(11);
            var b = new Narray<T>(11);
            T value = (T)Convert.ChangeType(99, typeof(T));
            Assert.AreEqual(true, target.Equal(b));

            b.Put1d(5, value);
            Assert.AreEqual(false, target.Equal(b));
            target.Put1d(5, value);
            Assert.AreEqual(true, target.Equal(b));
        }

        [TestMethod()]
        public void EqualTest()
        {
            EqualTestHelper<byte>();
            EqualTestHelper<int>();
            EqualTestHelper<float>();
        }

        /// <summary>
        ///A test for Fill
        ///</summary>
        public void FillTestHelper<T1, T2>()
        {
            var target = new Narray<T1>(5, 6);
            T1 valueT = (T1)Convert.ChangeType(99, typeof(T1));
            T2 valueS = (T2)Convert.ChangeType(99, typeof(T2));
            target.Fill(valueT);
            Assert.AreEqual(valueT, target.At1d(10));
            Assert.AreEqual(valueT, target.At1d(11));
            target.Fill(valueS);
            Assert.AreEqual(valueT, target.At1d(0));
            Assert.AreEqual(valueT, target.At1d(1));
        }

        [TestMethod()]
        public void FillTest()
        {
            FillTestHelper<int, byte>();
        }

        /// <summary>
        ///A test for GrowTo
        ///</summary>
        public void GrowToTestHelper<T>()
        {
            var target = new Narray<T>(12);
            Assert.AreEqual(1, target.Rank());
            Assert.AreEqual(12, target.Length1d());
            target.GrowTo(45);
            Assert.AreEqual(1, target.Rank());
            Assert.AreEqual(45, target.Length1d());
        }

        [TestMethod()]
        public void GrowToTest()
        {
            GrowToTestHelper<GenericParameterHelper>();
        }

        /// <summary>
        ///A test for Last
        ///</summary>
        public void LastTestHelper<T>()
        {
            var target = new Narray<T>(460);
            var expected = (T)Convert.ChangeType(99, typeof(T));
            Assert.AreNotEqual(expected, target.Last());
            target.Put1d(target.Length1d() - 1, expected);
            Assert.AreEqual(expected, target.Last());
        }

        [TestMethod()]
        public void LastTest()
        {
            LastTestHelper<byte>();
            LastTestHelper<int>();
        }

        /// <summary>
        ///A test for MakeLike
        ///</summary>
        public void MakeLikeTestHelper<T1, T2>()
        {
            var target = new Narray<T1>(1);
            var b = new Narray<T2>(12, 1, 1);
            Assert.AreNotEqual(target.Rank(), b.Rank());
            Assert.AreNotEqual(target.Length(), b.Length());
            var actual = target.MakeLike(b);
            Assert.AreEqual(target.Rank(), b.Rank());
            Assert.AreEqual(target.Length(), b.Length());
            Assert.AreEqual(actual.Rank(), b.Rank());
            Assert.AreEqual(actual.Length(), b.Length());
            Assert.AreSame(target, actual);
        }

        [TestMethod()]
        public void MakeLikeTest()
        {
            MakeLikeTestHelper<int, byte>();
        }

        /// <summary>
        ///A test for Move
        ///</summary>
        public void MoveTestHelper<T>()
        {
            var target = new Narray<T>(11, 2);
            var src = new Narray<T>(14);
            Assert.AreEqual(2, target.Rank());
            Assert.AreEqual(22, target.Length());
            target.Move(src);
            Assert.AreEqual(1, target.Rank());
            Assert.AreEqual(14, target.Length());
            Assert.IsTrue(src.IsEmpty());
        }

        [TestMethod()]
        public void MoveTest()
        {
            MoveTestHelper<GenericParameterHelper>();
        }

        /// <summary>
        ///A test for Pop
        ///</summary>
        public void PopTestHelper<T>()
        {
            int size = 12;
            Narray<T> target = new Narray<T>(size);
            T expected = default(T);
            Assert.AreEqual(expected, target.Pop());
            Assert.AreEqual(size - 1, target.Length1d());
            expected = (T)Convert.ChangeType(99, typeof(T));
            target.Put1d(target.Length1d() - 1, expected);
            Assert.AreEqual(expected, target.Pop());
            Assert.AreEqual(size - 2, target.Length1d());
            while (!target.IsEmpty())
                target.Pop();
            Assert.IsTrue(target.IsEmpty());
        }

        [TestMethod()]
        public void PopTest()
        {
            PopTestHelper<int>();
        }

        /// <summary>
        ///A test for Push
        ///</summary>
        public void PushTestHelper<T>()
        {
            int size = 12;
            Narray<T> target = new Narray<T>(size);
            T value = (T)Convert.ChangeType(99, typeof(T));
            target.Push(value);
            Assert.AreEqual(size + 1, target.Length1d());
            Assert.AreEqual(value, target.At1d(target.Length1d() - 1));
        }

        [TestMethod()]
        public void PushTest()
        {
            PushTestHelper<int>();
        }

        /// <summary>
        ///A test for Push
        ///</summary>
        public void PushTest1Helper<T, S>()
        {
            int size = 12;
            Narray<T> target = new Narray<T>(size);
            S value = (S)Convert.ChangeType(99, typeof(S));
            T valueT = (T)Convert.ChangeType(99, typeof(T));
            target.Push<S>(value);
            Assert.AreEqual(size + 1, target.Length1d());
            Assert.AreEqual(valueT, target.At1d(target.Length1d() - 1));
        }

        [TestMethod()]
        public void PushTest1()
        {
            PushTest1Helper<int, byte>();
        }

        /// <summary>
        ///A test for Put
        ///</summary>
        public void PutTestHelper<T>()
        {
            // 1d
            Narray<T> target = new Narray<T>(10);
            int i0 = 2;
            int i1 = 4;
            T value = (T)Convert.ChangeType(99, typeof(T));
            Assert.AreEqual(default(T), target.At(i0));
            target.Put(i0, value);
            Assert.AreEqual(value, target.At(i0));

            // 2d
            target = new Narray<T>(10, 11);
            Assert.AreEqual(default(T), target.At(i0, i1));
            target.Put(i0, i1, value);
            Assert.AreEqual(value, target.At(i0, i1));
        }

        [TestMethod()]
        public void PutTest()
        {
            PutTestHelper<int>();
        }

        /// <summary>
        ///A test for Put1d
        ///</summary>
        public void Put1dTestHelper<T>()
        {
            Narray<T> target = new Narray<T>(15, 10);
            int i0 = 50;
            T value = (T)Convert.ChangeType(99, typeof(T));
            Assert.AreEqual(default(T), target.At1d(i0));
            target.Put1d(i0, value);
            Assert.AreEqual(value, target.At1d(i0));
        }

        [TestMethod()]
        public void Put1dTest()
        {
            Put1dTestHelper<byte>();
            Put1dTestHelper<int>();
        }

        /// <summary>
        ///A test for Rank
        ///</summary>
        public void RankTestHelper<T>()
        {
            Narray<T> target = new Narray<T>(3, 2, 4);
            int expected = 3;
            Assert.AreEqual(expected, target.Rank());
        }

        [TestMethod()]
        public void RankTest()
        {
            RankTestHelper<GenericParameterHelper>();
        }

        /// <summary>
        ///A test for Renew
        ///</summary>
        public void RenewTestHelper<T>()
        {
            Narray<T> target = new Narray<T>(2, 5, 3);
            int i0 = 10;
            T value = (T)Convert.ChangeType(99, typeof(T));
            target.Put1d(i0, value);
            Assert.AreEqual(3, target.Rank());
            Assert.AreEqual(value, target.At1d(i0));
            target.Renew(2, 7);
            Assert.AreEqual(2, target.Rank());
            Assert.AreNotEqual(value, target.At1d(i0));
            Assert.AreEqual(default(T), target.At1d(i0));
        }

        [TestMethod()]
        public void RenewTest()
        {
            RenewTestHelper<int>();
        }

        /// <summary>
        ///A test for Reshape
        ///</summary>
        public void ReshapeTestHelper<T>()
        {
            int len = 2000;
            Narray<T> target = new Narray<T>(len);
            int i0 = 10;
            T value = (T)Convert.ChangeType(99, typeof(T));
            target.Put1d(i0, value);
            Assert.AreEqual(len, target.Length());
            target.Reshape(200, 10);
            Assert.AreEqual(len, target.Length());
            target.Reshape(100, 20, 0, 0);
            Assert.AreEqual(len, target.Length());
            target.Reshape(10, 20, 10);
            Assert.AreEqual(len, target.Length());
            // check exist value
            Assert.AreEqual(value, target.At1d(i0));
        }

        [TestMethod()]
        public void ReshapeTest()
        {
            ReshapeTestHelper<int>();
        }

        /// <summary>
        ///A test for Resize
        ///</summary>
        public void ResizeTestHelper<T>()
        {
            int len = 2000;
            Narray<T> target = new Narray<T>(len);
            int i0 = 10;
            T value = (T)Convert.ChangeType(99, typeof(T));
            target.Put1d(i0, value);
            Assert.AreEqual(len, target.Length());
            target.Resize(101, 20);
            Assert.AreNotEqual(len, target.Length());
            // check exist value
            Assert.AreNotEqual(value, target.At1d(i0));
        }

        [TestMethod()]
        public void ResizeTest()
        {
            ResizeTestHelper<int>();
        }

        /// <summary>
        ///A test for SameDims
        ///</summary>
        public void SameDimsTestHelper<T, S>()
        {
            Narray<T> target = new Narray<T>(10, 12);
            Narray<S> b = new Narray<S>(12, 10);
            Assert.IsFalse(target.SameDims<S>(b));
        }

        [TestMethod()]
        public void SameDimsTest()
        {
            SameDimsTestHelper<int, byte>();
        }

        /// <summary>
        ///A test for Set
        ///</summary>
        public void SetTestHelper<T>()
        {
            Narray<T> target = new Narray<T>(2, 5);
            Assert.AreEqual(2, target.Rank());
            Assert.AreEqual(10, target.Length());
            T v0 = default(T);
            T v1 = default(T);
            T v2 = default(T);
            target.Set(v0, v1, v2);
            Assert.AreEqual(1, target.Rank());
            Assert.AreEqual(3, target.Length());
            target.Set(v0, v1);
            Assert.AreEqual(1, target.Rank());
            Assert.AreEqual(2, target.Length());
            target.Set(v0);
            Assert.AreEqual(1, target.Rank());
            Assert.AreEqual(1, target.Length());
        }

        [TestMethod()]
        public void SetTest()
        {
            SetTestHelper<int>();
        }

        /// <summary>
        ///A test for Swap
        ///</summary>
        public void SwapTestHelper<T>()
        {
            Narray<T> target = new Narray<T>(10, 5);
            Narray<T> src = new Narray<T>(5, 10, 2);
            target.Swap(src);
            Assert.AreEqual(3, target.Rank());
            Assert.AreEqual(2, src.Rank());
            Assert.AreEqual(100, target.Length());
            Assert.AreEqual(50, src.Length());
        }

        [TestMethod()]
        public void SwapTest()
        {
            SwapTestHelper<GenericParameterHelper>();
        }

        /// <summary>
        ///A test for Truncate
        ///</summary>
        public void TruncateTestHelper<T>()
        {
            Narray<T> target = new Narray<T>(20);
            target.Truncate(5);
            Assert.AreEqual(5, target.Length());
        }

        [TestMethod()]
        public void TruncateTest()
        {
            TruncateTestHelper<GenericParameterHelper>();
        }

        /// <summary>
        ///A test for alloc_
        ///</summary>
        public void alloc_TestHelper<T>()
        {
            Narray_Accessor<T> target = new Narray_Accessor<T>(10);
            Assert.AreEqual(10, target.Length());
            int d0 = 10;
            int d1 = 20;
            int d2 = 30;
            int d3 = 0;
            target.alloc_(d0, d1, d2, d3);
            Assert.AreEqual(6000, target.Length());
            Assert.AreEqual(d0, target.Dim(0));
            Assert.AreEqual(d1, target.Dim(1));
            Assert.AreEqual(d2, target.Dim(2));
            Assert.AreEqual(d3, target.Dim(3));
        }

        [TestMethod()]
        [DeploymentItem("Ocronet.Dynamic.dll")]
        public void alloc_Test()
        {
            alloc_TestHelper<GenericParameterHelper>();
        }


        /// <summary>
        ///A test for check_range
        ///</summary>
        public void check_rangeTestHelper<T>()
        {
            int i = 0;
            int n = 4;
            Narray_Accessor<T>.check_range(i, n);
            Narray_Accessor<T>.check_range(1, n);
            Narray_Accessor<T>.check_range(2, n);
            Narray_Accessor<T>.check_range(3, n);
        }

        [TestMethod()]
        [DeploymentItem("Ocronet.Dynamic.dll")]
        public void check_rangeTest()
        {
            check_rangeTestHelper<GenericParameterHelper>();
        }

        /// <summary>
        ///A test for na_transfer
        ///</summary>
        public void na_transferTestHelper<T>(T srcVal)
        {
            T dst = default(T);
            T dstExpected = srcVal;
            T src = srcVal;
            T srcExpected = srcVal;
            Narray<T>.na_transfer(ref dst, ref src);
            Assert.AreEqual(dstExpected, dst);
            Assert.AreEqual(srcExpected, src);
        }

        [TestMethod()]
        [DeploymentItem("Ocronet.Dynamic.dll")]
        public void na_transferTest()
        {
            na_transferTestHelper<byte>((byte)101);
            na_transferTestHelper<int>(101);
        }

        /// <summary>
        ///A test for roundup_
        ///</summary>
        public void roundup_TestHelper<T>()
        {
            int i = 140;
            int expected = 209;
            int actual;
            actual = Narray_Accessor<T>.roundup_(i);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        [DeploymentItem("Ocronet.Dynamic.dll")]
        public void roundup_Test()
        {
            roundup_TestHelper<int>();
        }

        /// <summary>
        ///A test for reserve
        ///</summary>
        public void reserveTestHelper<T>()
        {
            Narray_Accessor<T> target = new Narray_Accessor<T>(10);
            int n = 5;
            target.Reserve(n);
            Assert.AreEqual(10, target.total);
            Assert.IsTrue(15 < target.allocated);
        }

        [TestMethod()]
        [DeploymentItem("Ocronet.Dynamic.dll")]
        public void reserveTest()
        {
            reserveTestHelper<GenericParameterHelper>();
        }

        /// <summary>
        ///A test for setdims_
        ///</summary>
        public void setdims_TestHelper<T>()
        {
            int len = 2000;
            Narray_Accessor<T> target = new Narray_Accessor<T>(len);
            target.SetDims(len, 0, 0, 0);
            Assert.AreEqual(len, target.Length());
            target.SetDims(100, 20, 0, 0);
            Assert.AreEqual(len, target.Length());
            target.SetDims(10, 20, 10, 0);
            Assert.AreEqual(len, target.Length());
        }

        [TestMethod()]
        [DeploymentItem("Ocronet.Dynamic.dll")]
        public void setdims_Test()
        {
            setdims_TestHelper<GenericParameterHelper>();
        }

        /// <summary>
        ///A test for unsafe_at
        ///</summary>
        public void unsafe_atTestHelper<T>()
        {
            Narray_Accessor<T> target = new Narray_Accessor<T>(100);
            T expected = default(T);
            // 1d
            for (int i = 0, n = target.Dim(0); i < n; i++)
                Assert.AreEqual(expected, target.UnsafeAt(i));
            // 2d
            target = new Narray_Accessor<T>(10, 10);
            for (int i = 0, n = target.Dim(0); i < n; i++)
                for (int j = 0, m = target.Dim(1); j < m; j++)
                    Assert.AreEqual(expected, target.UnsafeAt(i, j));
        }

        [TestMethod()]
        [DeploymentItem("Ocronet.Dynamic.dll")]
        public void unsafe_atTest()
        {
            unsafe_atTestHelper<int>();
        }

        /// <summary>
        ///A test for unsafe_at1d
        ///</summary>
        public void unsafe_at1dTestHelper<T>()
        {
            Narray_Accessor<T> target = new Narray_Accessor<T>(10, 10);
            T expected = default(T);
            // 1d
            for (int i = 0, n = target.Length1d(); i < n; i++)
                Assert.AreEqual(expected, target.UnsafeAt1d(i));
        }

        [TestMethod()]
        [DeploymentItem("Ocronet.Dynamic.dll")]
        public void unsafe_at1dTest()
        {
            unsafe_at1dTestHelper<int>();
        }

        /// <summary>
        ///A test for unsafe_put1d
        ///</summary>
        public void unsafe_put1dTestHelper<T, S>()
        {
            Narray_Accessor<T> target = new Narray_Accessor<T>(10, 10);
            int i = 15;
            T value = (T)Convert.ChangeType(99, typeof(T));
            target.UnsafePut1d(i, value);
            Assert.AreEqual(value, target.UnsafeAt1d(i));
        }

        [TestMethod()]
        [DeploymentItem("Ocronet.Dynamic.dll")]
        public void unsafe_put1dTest()
        {
            unsafe_put1dTestHelper<int, byte>();
        }

        /// <summary>
        ///A test for Item
        ///</summary>
        public void ItemTestHelper<T>()
        {
            Narray<T> target = new Narray<T>(10, 11, 12);
            int i0 = 3;
            int i1 = 5;
            int i2 = 7;
            T expected = default(T);
            T value = (T)Convert.ChangeType(99, typeof(T));
            Assert.AreEqual(expected, target[i0, i1, i2]);
            target[i0, i1, i2] = value;
            Assert.AreEqual(value, target[i0, i1, i2]);
        }

        [TestMethod()]
        public void ItemTest()
        {
            ItemTestHelper<int>();
        }
    }
}
