using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ocronet.Dynamic.Interfaces;

namespace Ocronet.Dynamic.OcroFST
{
    public abstract class CompositionFst : ReadOnlyFst
    {
        /// <summary>
        /// Return the 1st FST, releasing the ownership.
        /// </summary>
        public abstract IGenericFst Move1();

        /// <summary>
        /// Return the 2nd FST, releasing the ownership.
        /// </summary>
        public abstract IGenericFst Move2();

        public abstract void SplitIndex(out int result1,
                                out int result2,
                                int index);

        public abstract void SplitIndices(Intarray result1,
                                  Intarray result2,
                                  Intarray indices);

        /// <summary>
        /// Fail if this FST still owns either of the objects.
        /// </summary>
        public abstract void CheckOwnsNothing();
    }
}
