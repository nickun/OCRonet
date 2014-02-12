using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ocronet.Dynamic.Interfaces
{

    /// <summary>
    /// Compute text/image probabilities
    /// 
    /// The output is in the standard RGB format
    /// for text/image segmentation (see ocropus.org)
    /// </summary>
    public abstract class ITextImageClassification : IComponent
    {
        public override string Interface
        {
            get { return "ITextImageClassification"; }
        }

        /// <summary>
        /// Compute text/image probabilities.
        /// </summary>
        public abstract void TextImageProbabilities(Intarray outarray, Bytearray inarray);
    }

}
