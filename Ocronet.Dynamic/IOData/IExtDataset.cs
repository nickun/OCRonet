using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ocronet.Dynamic.IOData
{
    public abstract class IExtDataset : IDataset
    {
        public override string Interface
        {
            get { return "IExtDataset"; }
        }

        public virtual string FileExt
        {
            get { return ".ds"; }
        }

        public abstract void Add(Floatarray v, int c);

        public abstract void Add(Floatarray ds, Intarray cs);

        public abstract void Clear();
    }
}
