using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Ocronet.Dynamic.Interfaces;

namespace Ocronet.Dynamic.IOData
{
    public abstract class IOWrapper
    {
        public virtual string Name
        {
            get { return this.GetType().Name; }
        }

        public abstract void Clear();

        public abstract void Save(BinaryWriter writer);

        public abstract void Load(BinaryReader reader);

        public abstract string Info();

        public virtual void Set(string val)
        {
            throw new NotImplementedException();
        }

        public virtual void Set(double val)
        {
            throw new NotImplementedException();
        }

        public virtual void Set(IComponent p)
        {
            throw new NotImplementedException();
        }

        public virtual string Get()
        {
            throw new NotImplementedException();
        }

        public virtual float Getf()
        {
            throw new NotImplementedException();
        }

        public virtual IComponent GetComponent()
        {
            throw new NotImplementedException();
        }
    }
}
