using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ocronet.Dynamic.IOData
{
    public class RaggedDatasetFloat8 : RowDataset8
    {
        public RaggedDatasetFloat8()
        {
        }

        public RaggedDatasetFloat8(Narray<byte> ds, Intarray cs)
            : base(ds, cs)
        {
        }

        public override string Name
        {
            get { return "raggeddataset8"; }
        }

        public override int nClasses()
        {
            return -1;
        }

        public override int nFeatures()
        {
            return -1;
        }

        public override void Save(System.IO.BinaryWriter writer)
        {
            // TODO: реализовать RaggedDatasetFloat8.Save
            throw new NotImplementedException();
        }

        public override void Load(System.IO.BinaryReader reader)
        {
            // TODO: реализовать RaggedDatasetFloat8.Load
            throw new NotImplementedException();
        }
    }
}
