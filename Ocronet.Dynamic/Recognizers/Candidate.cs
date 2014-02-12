using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ocronet.Dynamic.Recognizers
{
    /// <summary>
    /// Candidate record of recognition
    /// </summary>
    public class Candidate
    {
        public int Index;
        public Floatarray Image;
        public OutputVector Outputs;
        public Rect BBox;
    }
}
