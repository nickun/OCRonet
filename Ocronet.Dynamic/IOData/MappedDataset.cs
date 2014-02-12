using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ocronet.Dynamic.IOData
{
    public class MappedDataset : IDataset
    {
        IDataset _ds;
        Intarray _classes;

        public MappedDataset(IDataset ds, Intarray classes)
        {
            this._ds = ds;
            this._classes = classes;
        }

        public override string Name
        {
            get { return "mappedds"; }
        }

        public override int nSamples()
        {
            return _ds.nSamples();
        }

        public override int nClasses()
        {
            return NarrayUtil.Max(_classes) + 1;
        }

        public override int nFeatures()
        {
            return _ds.nFeatures();
        }

        public override int Cls(int i)
        {
            return _classes[i];
        }

        public override void Input(Floatarray v, int i)
        {
            _ds.Input(v, i);
        }

        public override int Id(int i)
        {
            return _ds.Id(i);
        }
    }
}
