using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Ocronet.Dynamic.Utils
{
    public class BookStore : OldBookStore
    {
        protected override void GetLinesOfPage(Intarray lines, int ipage)
        {
            lines.Clear();
            string dirName = String.Format("{0}{1}{2:0000}", prefix, Path.DirectorySeparatorChar, ipage);
            //DirPattern dpattern = new DirPattern(dirName, @"([0-9a-fA-F][0-9a-fA-F][0-9a-fA-F][0-9a-fA-F][0-9a-fA-F][0-9a-fA-F])\.png");
            DirPattern dpattern = new DirPattern(dirName, @"([0-9][0-9][0-9][0-9][0-9][0-9])\.png");
            if (dpattern.Length > 0)
                lines.ReserveTo(dpattern.Length);
            for (int i = 0; i < dpattern.Length; i++)
            {
                int k = int.Parse(dpattern[i]);
                lines.Push(k);
            }
        }

        public override string PathFile(int page, int line = -1, string variant = null, string extension = null)
        {
            char dirSepar = System.IO.Path.DirectorySeparatorChar;
            string file = String.Format("{0}/{1:0000}", prefix, page);
            if (line >= 0)
                file += String.Format("/{0:000000}", line);
                //file += String.Format("/{0:X6}", line);
            if (!String.IsNullOrEmpty(variant))
                file += String.Format(".{0}", variant);
            if (!String.IsNullOrEmpty(extension))
                file += String.Format(".{0}", extension);
            file = file.Replace('/', dirSepar);
            Global.Debugf("bookstore", "(path: {0})", file);

            return file;
        }
    }
}
