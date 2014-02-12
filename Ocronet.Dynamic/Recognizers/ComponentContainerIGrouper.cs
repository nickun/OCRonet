using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ocronet.Dynamic.Component;
using Ocronet.Dynamic.Interfaces;

namespace Ocronet.Dynamic.Recognizers
{
    public class ComponentContainerIGrouper : ComponentContainer<IComponent>
    {
        public ComponentContainerIGrouper()
            : base(null)
        {
        }

        public ComponentContainerIGrouper(IGrouper grouper)
            : base(grouper)
        {
        }

        public new IGrouper Object
        {
            get { return (IGrouper)_component; }
            set { _component = value; }
        }
    }
}
