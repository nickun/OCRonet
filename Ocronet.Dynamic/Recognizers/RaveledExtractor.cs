using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ocronet.Dynamic.Recognizers
{
    public class RaveledExtractor : IExtractor
    {
        public override string Name
        {
            get { return "raveledfe"; }
        }

        public override void Extract(Narray<Floatarray> outarrays, Floatarray inarray)
        {
            outarrays.Clear();
            Floatarray image = outarrays.Push(new Floatarray());
            image.Copy(inarray);
        }
    }
}
