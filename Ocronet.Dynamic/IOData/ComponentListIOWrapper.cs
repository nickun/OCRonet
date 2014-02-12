using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ocronet.Dynamic.Interfaces;
using Ocronet.Dynamic.Utils;
using Ocronet.Dynamic.Component;
using System.IO;

namespace Ocronet.Dynamic.IOData
{
    public class ComponentListIOWrapper : IOWrapper
    {
        Narray<IComponent> data;

        public ComponentListIOWrapper(Narray<IComponent> complist)
        {
            data = complist;
        }

        public override void Clear()
        {
            data.Dealloc();
        }

        public override void Save(BinaryWriter writer)
        {
            Global.Debugf("iodetail", "<componentlist>");
            // write start tag
            BinIO.string_write(writer, "<componentlist>");
            // write array length
            BinIO.string_write(writer, data.Length().ToString());
            // write components
            for (int i = 0; i < data.Length(); i++)
            {
                if (data[i] != null)
                    Global.Debugf("iodetail", "   {0}", data[i].Name);
                ComponentIO.save_component(writer, data[i]);
            }
            // write end tag
            BinIO.string_write(writer, "</componentlist>");
            Global.Debugf("iodetail", "</componentlist>");
        }

        public override void Load(BinaryReader reader)
        {
            Global.Debugf("iodetail", "<componentlist>");
            string s;
            BinIO.string_read(reader, out s);
            if (s != "<componentlist>")
                throw new Exception("Expected string: <componentlist>");
            BinIO.string_read(reader, out s);
            int n;
            if (!int.TryParse(s, out n))
                throw new Exception("Incorrect number format (count of component).");
            data.Resize(n);
            for (int i = 0; i < n; i++)
            {
                data[i] = ComponentIO.load_component(reader);
                if (data[i] != null)
                    Global.Debugf("iodetail", "   {0}", data[i].Name);
            }
            BinIO.string_read(reader, out s);
            if (s != "</componentlist>")
                throw new Exception("Expected string: </componentlist>");
            Global.Debugf("iodetail", "</componentlist>");
        }

        public override string Info()
        {
            return "componentlist";
        }

        public override string ToString()
        {
            return Info();
        }
    }
}
