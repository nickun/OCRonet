using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ocronet.Dynamic.IOData
{
    public class Datasubset : IDataset
    {
        IDataset _ds;
        Intarray _samples;

        public Datasubset(IDataset ds, Intarray samples)
        {
            this._ds = ds;
            this._samples = samples;
        }

        public override string Name
        {
            get { return "datasubset"; }
        }

        public override int nSamples()
        {
            return _samples.Length();
        }

        public override int nClasses()
        {
            return _ds.nClasses();
        }

        public override int nFeatures()
        {
            return _ds.nFeatures();
        }

        public override int Cls(int i)
        {
            return _ds.Cls(_samples[i]);
        }

        public override void Input(Floatarray v, int i)
        {
            _ds.Input(v, _samples[i]);
        }

        public override int Id(int i)
        {
            return _ds.Id(_samples[i]);
        }
    }
}
