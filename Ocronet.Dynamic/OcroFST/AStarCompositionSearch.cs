using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ocronet.Dynamic.OcroFST
{
    public class AStarCompositionSearch : AStarSearch
    {
        Floatarray g1, g2; // well, that's against our convention,
        CompositionFst c;   // but I let it go since it's so local.

        public AStarCompositionSearch(Floatarray g1, Floatarray g2, CompositionFst c) : base(c)
        {
            this.g1 = g1;
            this.g2 = g2;
            this.c = c;
        }

        public override double Heuristic(int index)
        {
            int i1, i2;
            c.SplitIndex(out i1, out i2, index);
            return g1[i1] + g2[i2];
        }
    }
}
