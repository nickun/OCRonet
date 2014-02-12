using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Ocronet.Dynamic.IOData
{
    public class RowDataset8 : IExtDataset
    {
        protected ObjList<Narray<byte>> data;
        protected Intarray classes;
        protected int nc;
        protected int nf;

        public RowDataset8()
        {
            DatatypeSize = sizeof(byte);   // one byte
            data = new ObjList<Narray<byte>>();
            classes = new Intarray();
            nc = -1;
            nf = -1;
        }

        public RowDataset8(int nsamples)
        {
            DatatypeSize = sizeof(byte);   // one byte
            data = new ObjList<Narray<byte>>();
            data.ReserveTo(nsamples);
            classes = new Intarray();
            classes.ReserveTo(nsamples);
            nc = -1;
            nf = -1;
        }

        public RowDataset8(Narray<byte> ds, Intarray cs)
            : this()
        {
            for (int i = 0; i < ds.Dim(0); i++)
            {
                RowGet(data.Push(new Narray<byte>()), ds, i);
                classes.Push(cs[i]);
            }
            Recompute();
        }

        public override string Name
        {
            get { return "rowdataset8"; }
        }

        public override string FileExt
        {
            get { return ".dsr8"; }
        }

        public override void Save(BinaryWriter writer)
        {
            BinIO.magic_write(writer, "dataset");
            BinIO.scalar_write(writer, DatatypeSize);
            BinIO.scalar_write(writer, data.Dim(0));
            BinIO.scalar_write(writer, nc);
            BinIO.scalar_write(writer, nf);
            for (int i = 0; i < data.Dim(0); i++)
                BinIO.narray_write(writer, data[i]);
            BinIO.narray_write(writer, classes);
        }

        public override void Load(BinaryReader reader)
        {
            BinIO.magic_read(reader, "dataset");
            int t, nsamples;
            BinIO.scalar_read(reader, out t);
            CHECK_ARG(t == DatatypeSize, "t == sizeof(byte)");
            BinIO.scalar_read(reader, out nsamples);
            BinIO.scalar_read(reader, out nc);
            BinIO.scalar_read(reader, out nf);
            data.Clear();
            for (int i = 0; i < nsamples; i++)
                BinIO.narray_read(reader, data.Push(new Narray<byte>()));
            BinIO.narray_read(reader, classes);
            CHECK_ARG(nf > 0 && nf < 1000000, "nf > 0 && nf < 1000000");
            CHECK_ARG(nc > 0 && nc < 1000000, "nc > 0 && nc < 1000000");
        }

        public override void Add(Floatarray v, int c)
        {
            CHECK_ARG(NarrayUtil.Min(v) >= 0.0f && NarrayUtil.Max(v) <= 1.0f, "float8: value out of range (0..1)");
            CHECK_ARG(c >= -1, "c>=-1");
            if (c >= nc) nc = c + 1;
            if (nf < 0) nf = v.Length();
            Narray<byte> newDataItem = data.Push(new Narray<byte>());
            Copy(newDataItem, v);
            classes.Push(c);
            CHECK_ARG(nc > 0, "nc>0");
            CHECK_ARG(nf > 0, "nf>0");
        }

        public override void Add(Floatarray ds, Intarray cs)
        {
            for (int i = 0; i < ds.Dim(0); i++)
            {
                RowGet(data.Push(new Narray<byte>()), ds, i);
                classes.Push(cs[i]);
            }
            Recompute();
        }

        public override void Clear()
        {
            data.Clear();
            classes.Clear();
            nc = 0;
            nf = -1;
        }

        protected void Recompute()
        {
            nc = NarrayUtil.Max(classes) + 1;
            nf = data[0].Dim(0);
            CHECK_ARG(DataMin(data) >= 0 && DataMax(data) < 256, "min(data) >= 0 && max(data) < 256");
            CHECK_ARG(NarrayUtil.Min(classes) >= -1 && NarrayUtil.Max(classes) < 10000,
                "min(classes)>=-1 && max(classes)<10000");
            CHECK_ARG(nc > 0, "nc > 0");
            CHECK_ARG(nf > 0, "nf > 0");
        }


        public override int nSamples()
        {
            return data.Dim(0);
        }

        public override int nClasses()
        {
            return nc;
        }

        public override int nFeatures()
        {
            return nf;
        }

        public override int Cls(int i)
        {
            return classes[i];
        }

        public override void Input(Floatarray v, int i)
        {
            Copy(v, data[i]);
        }

        public override int Id(int i)
        {
            return i;
        }


        #region Helper methods

        public static void Copy(Narray<float> dst, Narray<byte> src)
        {
            dst.Resize(src.Dim(0), src.Dim(1), src.Dim(2), src.Dim(3));
            int n = dst.Length1d();
            for (int i = 0; i < n; i++)
                dst.UnsafePut1d(i, src.UnsafeAt1d(i) / 255.0f);
        }

        public static void Copy(Narray<byte> dst, Narray<float> src)
        {
            dst.Resize(src.Dim(0), src.Dim(1), src.Dim(2), src.Dim(3));
            int n = dst.Length1d();
            for (int i = 0; i < n; i++)
                dst.UnsafePut1d(i, Convert.ToByte(src.UnsafeAt1d(i) * 255));
        }

        public static void RowGet(Narray<byte> outv, Narray<float> data, int row)
        {
            outv.Resize(data.Dim(1));
            for (int i = 0; i < outv.Length(); i++)
                outv[i] = Convert.ToByte(data[row, i] * 255);
        }

        public static void RowGet(Narray<byte> outv, Narray<byte> data, int row)
        {
            outv.Resize(data.Dim(1));
            for (int i = 0; i < outv.Length(); i++)
                outv[i] = data[row, i];
        }

        private int DataMax(ObjList<Narray<byte>> data)
        {
            int max = byte.MinValue;
            for (int io = 0; io < data.Length(); io++)
            {
                for (int n = 0; n < data[io].Length1d(); n++)
                {
                    byte val = data[io].At1d(n);
                    if (val <= max) continue;
                    max = val;
                }
            }
            return max;
        }

        private int DataMin(ObjList<Narray<byte>> data)
        {
            int min = byte.MaxValue;
            for (int io = 0; io < data.Length(); io++)
            {
                for (int n = 0; n < data[io].Length1d(); n++)
                {
                    byte val = data[io].At1d(n);
                    if (val >= min) continue;
                    min = val;
                }
            }
            return min;
        }
        #endregion // Helper methods

    }
}
