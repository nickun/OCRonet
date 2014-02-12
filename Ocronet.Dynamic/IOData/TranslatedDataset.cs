using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ocronet.Dynamic.IOData
{
    public class TranslatedDataset : IDataset
    {
        IDataset _ds;
        Intarray _c2i;
        int _nc;

        public override string Name
        {
            get { return "mappeddataset"; }
        }

        public TranslatedDataset(IDataset ds, Intarray c2i)
        {
            _ds = ds;
            _c2i = c2i;
            _nc = NarrayUtil.Max(c2i) + 1;
        }

        public override int nClasses() { return _nc; }
        public override int nFeatures() { return _ds.nFeatures(); }
        public override int nSamples() { return _ds.nSamples(); }
        public override void Input(Floatarray v, int i) { _ds.Input(v, i); }
        public override int Cls(int i) { return _c2i[_ds.Cls(i)]; }
        public override int Id(int i) { return _ds.Id(i); }
    }
}
