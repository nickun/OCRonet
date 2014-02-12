using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ocronet.Dynamic.Interfaces
{
    /// <summary>
    /// Perform binarization of grayscale images.
    /// </summary>
    public abstract class IBinarize : IComponent
    {
        public override string Interface
        {
            get { return "IBinarize"; }
        }

        /// <summary>
        /// Binarize an image stored in a floatarray. Override this.
        /// </summary>
        public abstract void Binarize(Bytearray outarray, Bytearray inarray);

        /// <summary>
        /// Binarize an image stored in a bytearray.
        /// Override this if you want to provide a more efficient implementation.
        /// </summary>
        public virtual void Binarize(Bytearray outarray, Bytearray gray, Bytearray inarray)
        {
            Binarize(outarray, inarray);
            gray.Copy(inarray); // copy from inarray
        }
    }
}
