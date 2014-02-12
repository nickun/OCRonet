using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ocronet.Dynamic.Component;
using Ocronet.Dynamic.Interfaces;

namespace Ocronet.Dynamic.Recognizers
{
    public class ComponentContainerExtractor : ComponentContainer<IComponent>
    {
        public ComponentContainerExtractor()
            : base(null)
        {
        }

        public ComponentContainerExtractor(IExtractor extractor)
            : base(extractor)
        {
        }

        public new IExtractor Object
        {
            get { return (IExtractor)_component; }
            set { _component = value; }
        }
    }
}
