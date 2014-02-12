using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Ocronet.Dynamic.IOData
{
    public class Dataset8 : IExtDataset
    {
        protected Narray<sbyte> data;
        protected Intarray classes;
        protected int nc;
        protected int nf;

        public Dataset8()
        {
            DatatypeSize = sizeof(sbyte);   // one byte
            data = new Narray<sbyte>();
            classes = new Intarray();
            nc = -1;
            nf = -1;
        }

        public Dataset8(Narray<sbyte> data, Intarray classes)
            : this()
        {
            data.Copy(data);
            classes.Copy(classes);
            if (classes.Length() > 0)
            {
                nc = NarrayUtil.Max(classes) + 1;
                nf = data.Dim(1);
                //CHECK_ARG(NarrayUtil.Min(data) > -100 && NarrayUtil.Max(data) < 100, "min(data)>-100 && max(data)<100");
                CHECK_ARG(NarrayUtil.Min(classes) >= -1 && NarrayUtil.Max(classes) < 10000, "min(classes)>=-1 && max(classes)<10000");
            }
            else
            {
                nc = 0;
                nf = -1;
            }
        }

        public override string Name
        {
            get { return "dataset8"; }
        }

        public override string FileExt
        {
            get { return ".ds8"; }
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

        public override int Id(int i)
        {
            return i;
        }

        public override void Input(Floatarray v, int i)
        {
            v.Resize(data.Dim(1));
            for (int j = 0; j < v.Dim(0); j++)
                v.UnsafePut1d(j, data[i, j]);
        }

        public override void Save(BinaryWriter writer)
        {
            BinIO.magic_write(writer, "dataset");
            BinIO.scalar_write(writer, DatatypeSize);
            BinIO.narray_write(writer, data);
            BinIO.narray_write(writer, classes);
            BinIO.scalar_write(writer, nc);
            BinIO.scalar_write(writer, nf);
        }

        public override void Load(BinaryReader reader)
        {
            BinIO.magic_read(reader, "dataset");
            int t;
            BinIO.scalar_read(reader, out t);
            CHECK_ARG(t == DatatypeSize, "t == sizeof(sbyte)");
            BinIO.narray_read(reader, data);
            BinIO.narray_read(reader, classes);
            BinIO.scalar_read(reader, out nc);
            BinIO.scalar_read(reader, out nf);
            CHECK_ARG(nf > 0 && nf < 1000000, "nf > 0 && nf < 1000000");
            CHECK_ARG(nc > 0 && nc < 1000000, "nc > 0 && nc < 1000000");
        }


        public override void Add(Floatarray v, int c)
        {
            CHECK_ARG(NarrayUtil.Min(v) > -1.2f && NarrayUtil.Max(v) < 1.2f, "float8: value out of range (-1.2..1.2)");
            CHECK_ARG(c >= -1, "c>=-1");
            if (c >= nc) nc = c + 1;
            if (nf < 0) nf = v.Length();
            RowPush(data, v);
            classes.Push(c);
        }

        public override void Add(Floatarray ds, Intarray cs)
        {
            RowsPush(data, ds);
            classes.Append(cs);
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
            nf = data.Dim(1);
            CHECK_ARG(Min(data) > -100 && Max(data) < 100, "min(data) > -100 && max(data) < 100");
            CHECK_ARG(NarrayUtil.Min(classes) >= -1 && NarrayUtil.Max(classes) < 10000,
                "min(classes)>=-1 && max(classes)<10000");
            CHECK_ARG(nc > 0, "nc > 0");
            CHECK_ARG(nf > 0, "nf > 0");
        }

        #region Helper methods

        public static void RowPush(Narray<sbyte> table, Narray<float> data)
        {
            if (table.Length1d() == 0)
            {
                Copy(table, data);
                table.Reshape(1, table.Length());
                return;
            }
            CHECK_ARG(table.Dim(1) == data.Length(), "table.Dim(1) == data.Length()");
            table.Reserve(table.Length1d() + data.Length());
            table.SetDims(table.Dim(0) + 1, table.Dim(1), 0, 0);
            int irow = table.Dim(0) - 1;
            for (int k = 0; k < table.Dim(1); k++)
                table[irow, k] = Convert.ToSByte(data.UnsafeAt1d(k) * 100);
        }

        public static void RowsPush(Narray<sbyte> table, Narray<float> ftable)
        {
            if (table.Length1d() == 0)
            {
                Copy(table, ftable);
                return;
            }
            CHECK_ARG(table.Dim(1) == ftable.Dim(1), "table.Dim(1) == ftable.Dim(1)");
            table.Reserve(ftable.Length());
            int irow = table.Dim(0);
            table.SetDims(table.Dim(0) + ftable.Dim(0), table.Dim(1), 0, 0);
            for (int i = 0; i < ftable.Dim(0); i++)
                for (int k = 0; k < table.Dim(1); k++)
                    table[irow + i, k] = Convert.ToSByte(ftable[i, k] * 100);
        }

        public static void Copy(Narray<sbyte> dst, Narray<float> src)
        {
            dst.Resize(src.Dim(0), src.Dim(1), src.Dim(2), src.Dim(3));
            int n = dst.Length1d();
            for (int i = 0; i < n; i++)
                dst[i] = Convert.ToSByte(src.UnsafeAt1d(i) * 100);
        }

        public static int Min(Narray<sbyte> a)
        {
            sbyte value = a.At1d(0);
            for (int i = 1; i < a.Length1d(); i++)
            {
                sbyte nvalue = a.At1d(i);
                if (nvalue >= value) continue;
                value = nvalue;
            }
            return value;
        }

        public static int Max(Narray<sbyte> a)
        {
            sbyte value = a.At1d(0);
            for (int i = 1; i < a.Length1d(); i++)
            {
                sbyte nvalue = a.At1d(i);
                if (nvalue <= value) continue;
                value = nvalue;
            }
            return value;
        }
        #endregion // Helper methods

    }
}
