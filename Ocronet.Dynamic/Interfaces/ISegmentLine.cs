using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ocronet.Dynamic.Component;

namespace Ocronet.Dynamic.Interfaces
{
    /// <summary>
    /// Compute line segmentation into character hypotheses.
    /// 
    /// The output is in the standard RGB format
    /// for page segmentation (see ocropus.org)
    /// </summary>
    public abstract class ISegmentLine : IComponent
    {
        public override string Interface
        {
            get { return "ISegmentLine"; }
        }

        /// <summary>
        /// Создает экземпляр класса, наследованного от ISegmentLine
        /// </summary>
        /// <param name="name">имя класа</param>
        public static ISegmentLine MakeSegmentLine(string name)
        {
            if (name.ToLower() == "none")
                return null;
            return ComponentCreator.MakeComponent<ISegmentLine>(name);
        }

        /// <summary>
        /// Segment a line.
        /// </summary>
        public abstract void Charseg(ref Intarray outarray, Bytearray inarray);
    }
}
