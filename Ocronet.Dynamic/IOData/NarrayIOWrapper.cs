using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Ocronet.Dynamic.IOData
{
    public class NarrayIOWrapper<T> : IOWrapper
    {
        Narray<T> data;

        public NarrayIOWrapper(Narray<T> array)
        {
            data = array;
        }

        public override void Clear()
        {
            data.Clear();
        }

        public override void Save(BinaryWriter writer)
        {
            BinIO.narray_write(writer, data);
        }

        public override void Load(BinaryReader reader)
        {
            BinIO.narray_read(reader, data);
        }

        public override string Info()
        {
            return data.ToString(); // String.Format("Narray {0} {1} {2} {3}", data.Dim(0), data.Dim(1), data.Dim(2), data.Dim(3));
        }

        public override string ToString()
        {
            return Info();
        }
    }
}
