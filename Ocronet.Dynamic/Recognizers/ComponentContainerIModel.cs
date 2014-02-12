using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ocronet.Dynamic.Component;
using Ocronet.Dynamic.Interfaces;

namespace Ocronet.Dynamic.Recognizers
{
    public class ComponentContainerIModel : ComponentContainer<IComponent>
    {
        public ComponentContainerIModel()
            : base(null)
        {
        }

        public ComponentContainerIModel(IModel model)
            : base(model)
        {
        }

        public new IModel Object
        {
            get { return (IModel)_component; }
            set { _component = value; }
        }
    }
}
