using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ocronet.Dynamic.Interfaces
{
    /// <summary>
    /// Compute page segmentation into columns, lines, etc.
    /// 
    /// The output is in the standard RGB format
    /// for page segmentation (see ocropus.org)
    /// </summary>
    public abstract class ISegmentPage : IComponent
    {
        public override string Interface
        {
            get { return "ISegmentPage"; }
        }

        /// <summary>
        /// Segment the page.
        /// </summary>
        public abstract void Segment(out Intarray outarray, Bytearray inarray);

        public virtual void Segment(out Intarray outarray, Bytearray inarray, Rectarray obstacles)
        {
            throw new NotImplementedException("ISegmentPage:Segment: unimplemented"); 
        }
    }
}
