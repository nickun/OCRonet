using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ocronet.Dynamic.IOData;

namespace Ocronet.Dynamic.Recognizers
{
    public class ExtractedDataset : IDataset
    {
        private IDataset _ds;
        private IExtractor _ex;

        public ExtractedDataset(IDataset ds, IExtractor ex)
        {
            _ds = ds;
            _ex = ex;
        }

        public override int nSamples()
        {
            return _ds.nSamples();
        }

        public override int nClasses()
        {
            return _ds.nClasses();
        }

        public override int nFeatures()
        {
            return _ds.nFeatures();
        }

        public override void Input(Floatarray v, int i)
        {
            Floatarray temp = new Floatarray();
            _ds.Input(temp, i);
            _ex.Extract(v, temp);
        }

        public override int Cls(int i)
        {
            return _ds.Cls(i);
        }

        public override int Id(int i)
        {
            return _ds.Id(i);
        }
    }
}
