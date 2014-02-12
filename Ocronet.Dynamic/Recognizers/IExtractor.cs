using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ocronet.Dynamic.Interfaces;

namespace Ocronet.Dynamic.Recognizers
{
    public abstract class IExtractor : IComponent
    {
        public override string Name
        {
            get { return "IExtractor"; }
        }

        public override string Interface
        {
            get { return "IExtractor"; }
        }

        public abstract void Extract(Narray<Floatarray> outarrays, Floatarray inarray);

        public virtual void Extract(Floatarray outa, Floatarray ina)
        {
            outa.Clear();
            Narray<Floatarray> items = new Narray<Floatarray>();
            Extract(items, ina);
            //int num = 0;
            for (int i = 0; i < items.Length(); i++)
            {
                Floatarray a = items[i];
                outa.ReserveTo(outa.Length() + a.Length());    // optimization
                for (int j = 0; j < a.Length(); j++)
                {
                    outa.Push(a.At1d(j));
                    //outa[num++] = a.At1d(j);
                }
            }
        }

        public virtual void Extract(Bytearray outa, Bytearray ina)
        {
            Floatarray fina = new Floatarray();
            Floatarray fouta = new Floatarray();
            fina.Copy(ina);
            Extract(fouta, fina);
            outa.Copy(fouta);
        }

        /// <summary>
        /// convenience methods
        /// </summary>
        public virtual void Extract(Floatarray v)
        {
            Floatarray temp = new Floatarray();
            Extract(temp, v);
            v.Move(temp);
        }
    }
}
