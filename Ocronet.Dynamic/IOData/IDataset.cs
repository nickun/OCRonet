using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ocronet.Dynamic.Interfaces;

namespace Ocronet.Dynamic.IOData
{
    public abstract class IDataset : IComponent
    {
        protected int DatatypeSize;
        protected float limit;

        public IDataset()
        {
            DatatypeSize = 0;
            limit = 0.0f;
        }

        public override string Interface
        {
            get { return "IDataset"; }
        }

        public abstract int nSamples();

        public abstract int nClasses();

        public abstract int nFeatures();

        public abstract void Input(Floatarray v, int i);

        public abstract int Cls(int i);

        public abstract int Id(int i);

        public virtual void Output(Floatarray outv, int i)
        {
            outv.Resize(nClasses());
            outv.Fill(limit);
            outv.Put1d(Cls(i), 1 - limit);
        }

        public void Input1d(Floatarray v, int i)
        {
            Input(v, i);
            v.Reshape(v.Length());
        }

    }
}
