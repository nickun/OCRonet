using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ocronet.Dynamic.Interfaces
{
    /// <summary>
    /// Cleanup for gray scale document images.
    /// 
    /// Should work for both gray scale and binary images.
    /// </summary>
    public abstract class ICleanupGray : IComponent
    {
        public override string Interface
        {
            get { return "ICleanupGray"; }
        }

        /// <summary>
        /// Clean up a gray image.
        /// </summary>
        public abstract void Cleanup(Bytearray outarray, Bytearray inarray);
    }
}
