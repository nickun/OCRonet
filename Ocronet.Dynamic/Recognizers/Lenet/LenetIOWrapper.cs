using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Ocronet.Dynamic.IOData;
using Ocronet.Dynamic.Utils;

namespace Ocronet.Dynamic.Recognizers.Lenet
{
    public class LenetIOWrapper : IOWrapper
    {
        string name;
        LenetWrapper lenetWrap;
        Doublearray lenetparam;

        public LenetIOWrapper(LenetWrapper lenet, string name)
        {
            this.lenetWrap = lenet;
            this.lenetparam = new Doublearray();
            this.name = name;
        }

        public override string Name
        {
            get { return name; }
        }

        public override void Clear()
        {
            lenetparam.Clear();
        }

        public override void Save(BinaryWriter writer)
        {
            if (lenetWrap.IsEmpty)
            {
                BinIO.string_write(writer, "<null/>");
                return;
            }

            double[] dbuffer;
            int size;
            // receive buffer from wrapped lenet
            lenetWrap.SaveNetworkToBuffer(out size, out dbuffer);
            lenetparam.Resize(size);
            for (int i = 0; i < size; i++)
                lenetparam.UnsafePut1d(i, dbuffer[i]);

            BinIO.string_write(writer, "<object>");
            //BinIO.string_write(writer, comp.Name);

            // write lenet arguments
            BinIO.scalar_write(writer, lenetWrap.Classes.Length);
            for (int i = 0; i < lenetWrap.Classes.Length; i++)
                BinIO.scalar_write(writer, lenetWrap.Classes[i]);
            BinIO.scalar_write(writer, Convert.ToByte(lenetWrap.TanhSigmoid));
            BinIO.scalar_write(writer, Convert.ToByte(lenetWrap.NetNorm));
            BinIO.scalar_write(writer, Convert.ToByte(lenetWrap.AsciiTarget));

            // save Narray to stream
            BinIO.narray_write(writer, lenetparam);

            BinIO.string_write(writer, "</object>");
        }

        public override void Load(BinaryReader reader)
        {
            string s;
            BinIO.string_read(reader, out s);
            if (s == "<object>")
            {
                // load lenet arguments
                int nclasses;
                BinIO.scalar_read(reader, out nclasses);
                lenetWrap.Classes = new int[nclasses];
                for (int i = 0; i < nclasses; i++)
                    BinIO.scalar_read(reader, out lenetWrap.Classes[i]);
                byte boolval;
                BinIO.scalar_read(reader, out boolval);
                lenetWrap.TanhSigmoid = Convert.ToBoolean(boolval);
                BinIO.scalar_read(reader, out boolval);
                lenetWrap.NetNorm = Convert.ToBoolean(boolval);
                BinIO.scalar_read(reader, out boolval);
                lenetWrap.AsciiTarget = Convert.ToBoolean(boolval);

                // load Narray from stream
                BinIO.narray_read(reader, lenetparam);
                double[] dbuffer = lenetparam.To1DArray();

                Global.Debugf("info", "loading " + Name + "..");

                // create lenet
                if (lenetWrap.IsEmpty)
                    lenetWrap.CreateLenet(lenetWrap.Classes.Length, lenetWrap.Classes, lenetWrap.TanhSigmoid, lenetWrap.NetNorm, lenetWrap.AsciiTarget);

                // send loaded buffer to created lenet
                lenetWrap.LoadNetworkFromBuffer(dbuffer, dbuffer.Length);
                BinIO.string_read(reader, out s);
                if (s != "</object>")
                    throw new Exception("Expected string: </object>");
            }
        }

        public override string Info()
        {
            return String.Format("{0} classes count: {1}", Name, lenetWrap.Classes.Length);
        }

        public override string ToString()
        {
            return Info();
        }
    }
}
