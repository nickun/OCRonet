using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ocronet.Dynamic.IOData
{
    /// <summary>
    /// Класс еще не дописан!!!
    /// </summary>
    public class BitDataset : IExtDataset
    {
        int nfeat;
        Intarray classes;

        public BitDataset()
        {
            nfeat = -1;
            classes = null;
        }

        public override int nSamples()
        {
            return classes.Length();
        }

        public override int nClasses()
        {
            return NarrayUtil.Max(classes) + 1;
        }

        public override int nFeatures()
        {
            return nfeat;
        }

        public override void Input(Floatarray v, int i)
        {
            throw new NotImplementedException();
        }

        public override void Add(Floatarray v, int c)
        {
            throw new NotImplementedException();
        }

        public override void Add(Floatarray ds, Intarray cs)
        {
            throw new NotImplementedException();
        }

        public override void Clear()
        {
            throw new NotImplementedException();
        }

        public override int Cls(int i)
        {
            throw new NotImplementedException();
        }

        public override int Id(int i)
        {
            throw new NotImplementedException();
        }
    }
}
