using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ocronet.Dynamic.Interfaces;
using System.Drawing;

namespace Ocronet.Dynamic.Utils
{
    public class SmartBookStore : IBookStore
    {
        IBookStore p;

        public override void SetPrefix(string prefix)
        {
            if (DirPattern.Exist(prefix, @"[0-9][0-9][0-9][0-9]", @"([0-9][0-9][0-9][0-9])\.png"))
            {
                Global.Debugf("info", "selecting OldBookStore");
                p = new OldBookStore();
            }
            else
            {
                Global.Debugf("info", "selecting (new) BookStore");
                p = new BookStore();
            }
            p.SetPrefix(prefix);
        }


        public override bool GetPage(Bytearray image, int page, string variant = null)
        {
            return p.GetPage(image, page, variant);
        }

        public override bool GetPage(Intarray image, int page, string variant = null)
        {
            return p.GetPage(image, page, variant);
        }

        public override bool HaveLine(int page, int line, string variant = null, string extension = "png")
        {
            return p.HaveLine(page, line, variant, extension);
        }

        public override bool GetLine(Bytearray image, int page, int line, string variant = null)
        {
            return p.GetLine(image, page, line, variant);
        }

        public override bool GetLine(Intarray image, int page, int line, string variant = null)
        {
            return p.GetLine(image, page, line, variant);
        }

        public override bool GetLine(out string s, int page, int line, string variant = null)
        {
            return p.GetLine(out s, page, line, variant);
        }

        public override bool GetLine(out System.Drawing.Bitmap bitmap, int page, int line, string variant = null)
        {
            return p.GetLine(out bitmap, page, line, variant);
        }


        public override void PutPage(Bytearray image, int page, string variant = null)
        {
            p.PutPage(image, page, variant);
        }

        public override void PutPage(Intarray image, int page, string variant = null)
        {
            p.PutPage(image, page, variant);
        }

        public override void PutLine(Bytearray image, int page, int line, string variant = null)
        {
            p.PutLine(image, page, line, variant);
        }

        public override void PutLine(Intarray image, int page, int line, string variant = null)
        {
            p.PutLine(image, page, line, variant);
        }

        public override void PutLine(string s, int page, int line, string variant = null)
        {
            p.PutLine(s, page, line, variant);
        }

        public override void PutLine(Bitmap bitmap, int page, int line, string variant = null)
        {
            p.PutLine(bitmap, page, line, variant);
        }


        public override string PathFile(int page, int line = -1, string variant = null, string extension = null)
        {
            return p.PathFile(page, line, variant, extension);
        }

        public override System.IO.FileStream Open(System.IO.FileMode mode, int page, int line = -1, string variant = null, string extension = null)
        {
            return p.Open(mode, page, line, variant, extension);
        }


        public override int NumberOfPages()
        {
            return p.NumberOfPages();
        }

        public override int LinesOnPage(int ipage)
        {
            return p.LinesOnPage(ipage);
        }

        public override int GetLineId(int ipage, int iline)
        {
            return p.GetLineId(ipage, iline);
        }

        public override int AddLineId(int ipage, int iline)
        {
            return p.AddLineId(ipage, iline);
        }

        public override bool GetCosts(Floatarray costs, int page, int line, string variant = null)
        {
            return p.GetCosts(costs, page, line, variant);
        }

        public override void PutCosts(Floatarray costs, int page, int line, string variant = null)
        {
            p.PutCosts(costs, page, line, variant);
        }

    }
}
