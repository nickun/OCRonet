using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ocronet.Dynamic.Interfaces
{
    /// <summary>
    /// Cleanup for binary document images.
    /// 
    /// Should throw an error when applied to grayscale.
    /// </summary>
    public abstract class ICleanupBinary : IComponent
    {
        public override string Interface
        {
            get { return "ICleanupBinary"; }
        }

        /// <summary>
        /// Clean up a binary image.
        /// </summary>
        public abstract void Cleanup(Bytearray outarray, Bytearray inarray);
    }
}
