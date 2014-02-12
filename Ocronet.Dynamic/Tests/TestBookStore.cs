using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ocronet.Dynamic.Interfaces;
using Ocronet.Dynamic.Utils;

namespace Ocronet.Dynamic.Tests
{
    public class TestBookStore
    {
        public void RunTest()
        {
            IBookStore bstore = new SmartBookStore();
            bstore.SetPrefix(@"data2");

            Console.WriteLine("Pages in bookstore: {0}", bstore.NumberOfPages());
            Console.WriteLine("List pages..");
            for (int i = 0; i < bstore.NumberOfPages(); i++)
            {
                Console.WriteLine("page {0:0000}\t->\t{1,6} lines", i, bstore.LinesOnPage(i));
            }
            Bytearray line = new Bytearray();
            bstore.GetLine(line, 1, 5);
            Console.WriteLine("line{0}      [{1},{2}]", 5, line.Dim(0), line.Dim(1));
            Intarray cline = new Intarray();
            bstore.GetCharSegmentation(cline, 1, 5);
            Console.WriteLine("line{0}.cseg [{1},{2}]", 5, cline.Dim(0), cline.Dim(1));
        }
    }
}
