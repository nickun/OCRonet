using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ocronet.Dynamic.Interfaces;

namespace Ocronet.Dynamic.Component
{
    public class ComponentContainer<T> where T : IComponent
    {
        protected T _component;

        public ComponentContainer(T component)
        {
            _component = component;
        }

        public T Object
        {
            get { return _component; }
            set { _component = value; }
        }

        public bool IsEmpty
        {
            get { return _component == null; }
        }

        public IComponent GetComponent()
        {
            if (_component == null)
            {
                return null;
            }
            /*Type type = typeof(T);
            if (!(_component is IComponent))
                throw new Exception(String.Format("class {0} does not IComponent", type.Name));*/
            return _component;
        }

        public void SetComponent(IComponent component)
        {
            if (component == null)
            {
                _component = default(T);
                return;
            }
            /*Type type = typeof(T);
            if (!(component is T))
                throw new Exception(String.Format("class {0} does not IComponent", type.Name));
            _component = (T)((Object)component);*/
            Type type = typeof(T);
            if (!(component is T))
                throw new Exception(String.Format("class {0} does not IComponent", type.Name));
            _component = (T)component;
        }

        public T CreateComponent(string name)
        {
            _component = ComponentCreator.MakeComponent<T>(name);
            return _component;
        }
    }
}
