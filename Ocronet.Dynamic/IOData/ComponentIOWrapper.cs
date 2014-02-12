using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Ocronet.Dynamic.Interfaces;
using Ocronet.Dynamic.Component;

namespace Ocronet.Dynamic.IOData
{
    public class ComponentIOWrapper : IOWrapper
    {
        ComponentContainer<IComponent> data;

        public ComponentIOWrapper(ComponentContainer<IComponent> comp)
        {
            data = comp;
        }

        public override void Set(IComponent p)
        {
            data.SetComponent(p);
        }

        public override IComponent GetComponent()
        {
            return data.GetComponent();
        }

        public override void Clear()
        {
            data.SetComponent(null);
        }

        public override void Save(BinaryWriter writer)
        {
            ComponentIO.save_component(writer, data.GetComponent());
        }

        public override void Load(BinaryReader reader)
        {
            data.SetComponent(ComponentIO.load_component(reader));
        }

        public override string Info()
        {
            if (data != null)
                return String.Format("{0} {1}", data.GetComponent().Name, data.GetComponent().Description);
            else
                return "NULL";
        }

        public override string ToString()
        {
            return Info();
        }
    }
}
