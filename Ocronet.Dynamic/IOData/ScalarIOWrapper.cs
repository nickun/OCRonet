using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Ocronet.Dynamic.Utils;

namespace Ocronet.Dynamic.IOData
{
    public class ScalarIOWrapper<T> : IOWrapper
    {
        Object data;

        public ScalarIOWrapper(ref T scalar)
        {
            data = scalar;
        }

        public override void Clear()
        {
            data = 0;
        }

        public override void Save(BinaryWriter writer)
        {
            BinIO.scalar_write(writer, (T)data);
        }

        public override void Load(System.IO.BinaryReader reader)
        {
            T val;
            BinIO.scalar_read<T>(reader, out val);
            data = val;
        }

        public override string Info()
        {
            return "scalar " + data;
        }

        public override string ToString()
        {
            return Info();
        }
    }
}
