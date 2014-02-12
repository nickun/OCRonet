using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ocronet.Dynamic.Component;
using Ocronet.Dynamic.Interfaces;

namespace Ocronet.Dynamic.Recognizers
{
    public class ComponentContainerISegmentLine : ComponentContainer<IComponent>
    {
        public ComponentContainerISegmentLine()
            : base(null)
        {
        }

        public ComponentContainerISegmentLine(ISegmentLine sline)
            : base(sline)
        {
        }

        public new ISegmentLine Object
        {
            get { return (ISegmentLine)_component; }
            set { _component = value; }
        }
    }
}
