using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Ocronet.Dynamic.Interfaces;
using Ocronet.Dynamic.IOData;
using Ocronet.Dynamic.Utils;

namespace Ocronet.Dynamic.Component
{
    public static class ComponentIO
    {
        public static int level = 0;

        public static void save_component(BinaryWriter writer, IComponent comp)
        {
            if (comp == null)
            {
                Global.Debugf("iodetail", "{0}[writing OBJ:NULL]", level);
                BinIO.string_write(writer, "<null/>");
            }
            else
            {
                Global.Debugf("iodetail", "{0}[writing OBJ:{1}]", level, comp.Name);
                level++;
                BinIO.string_write(writer, "<object>");
                BinIO.string_write(writer, comp.Name);
                comp.Save(writer);
                BinIO.string_write(writer, "</object>");
                level--;
                Global.Debugf("iodetail", "{0}[done]", level);
            }
        }

        public static IComponent load_component(BinaryReader reader)
        {
            IComponent result = null;
            string s;
            BinIO.string_read(reader, out s);
            Global.Debugf("iodetail", "{0}[got {1}]", level, s);
            if (s == "<object>")
            {
                level++;
                BinIO.string_read(reader, out s);
                if (level <= 2)
                    Global.Debugf("info", "{0," + (level-1) + "}loading component {1}", "", s);

                Global.Debugf("iodetail", "{0}[constructing {1}]", level, s);
                result = ComponentCreator.MakeComponent(s);
                result.Load(reader);
                BinIO.string_read(reader, out s);
                if (s != "</object>")
                    throw new Exception("Expected string: </object>");
                level--;
            }
            else if (s.StartsWith("OBJ:"))
            {
                s = s.Substring(4);
                level++;
                Global.Debugf("iodetail", "{0}[constructing {1}]", level, s);
                result = ComponentCreator.MakeComponent(s);
                result.Load(reader);
                BinIO.string_read(reader, out s);
                if (s != "OBJ:END")
                    throw new Exception("Expected string: </object>");
                level--;
            }
            Global.Debugf("iodetail", "{0}[done]", level);
            return result;
        }
    }
}
