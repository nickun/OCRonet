using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using Ocronet.Dynamic.Segmentation;
using Ocronet.Dynamic.Utils;

namespace Ocronet.Dynamic.Interfaces
{
    public abstract class IBookStore : IComponent
    {
        public override string Name
        {
            get { return this.GetType().Name; }
        }

        public override string Interface
        {
            get { return "IBookStore"; }
        }

        public override string Description
        {
            get { return this.GetType().FullName; }
        }

        public override void Info()
        {
            Global.Debugf("info", "{0} has {1} pages", Name, NumberOfPages());
        }

        public abstract void SetPrefix(string s);

        public abstract bool HaveLine(int page, int line, string variant = null, string extension = "png");
        public abstract bool GetPage(Bytearray image, int page, string variant = null);
        public abstract bool GetPage(Intarray image, int page, string variant = null);
        public abstract bool GetLine(Bytearray image, int page, int line, string variant = null);
        public abstract bool GetLine(Intarray image, int page, int line, string variant = null);
        public abstract bool GetLine(out string s, int page, int line, string variant = null);
        public abstract bool GetLine(out Bitmap bitmap, int page, int line, string variant = null);
        public abstract bool GetCosts(Floatarray costs, int page, int line, string variant = null);

        public abstract void PutPage(Bytearray image, int page, string variant = null);
        public abstract void PutPage(Intarray image, int page, string variant = null);
        public abstract void PutLine(Bytearray image, int page, int line, string variant = null);
        public abstract void PutLine(Intarray image, int page, int line, string variant = null);
        public abstract void PutLine(string s, int page, int line, string variant = null);
        public abstract void PutLine(Bitmap bitmap, int page, int line, string variant = null);
        public abstract void PutCosts(Floatarray costs, int page, int line, string variant = null);

        public abstract string PathFile(int page, int line = -1, string variant = null, string extension = null);
        public abstract FileStream Open(FileMode mode, int page, int line = -1, string variant = null, string extension = null);

        public abstract int NumberOfPages();
        public abstract int LinesOnPage(int ipage);
        public abstract int GetLineId(int ipage, int iline);
        public abstract int AddLineId(int ipage, int iline);

        public void GetLineBin(Bytearray image, int page, int line, string variant = null)
        {
            string v = "bin";
            if(!String.IsNullOrEmpty(variant)) { v += "."; v += variant; }
            GetLine(image, page, line, v);
        }
        public void PutLineBin(Bytearray image, int page, int line, string variant = null)
        {
            string v = "bin";
            if (!String.IsNullOrEmpty(variant)) { v += "."; v += variant; }
            PutLine(image, page, line, v);
        }

        public void GetPageSegmentation(Intarray image, int page, string variant = null)
        {
            string v = "pseg";
            if (!String.IsNullOrEmpty(variant)) { v += "."; v += variant; }
            GetPage(image, page, v);
            //SegmRoutine.check_page_segmentation(image);   // dublicate!
            SegmRoutine.make_page_segmentation_black(image);
        }

        public void PutPageSegmentation(Intarray image, int page, string variant = null)
        {
            string v = "pseg";
            if (!String.IsNullOrEmpty(variant)) { v += "."; v += variant; }
            Intarray simage = new Intarray();
            simage.Copy(image);
            SegmRoutine.check_page_segmentation(simage);
            SegmRoutine.make_page_segmentation_white(simage);
            PutPage(simage, page, v);
        }

        public void GetLineSegmentation(Intarray image, int page, int line, string variant = null)
        {
            string v = "rseg";
            if (!String.IsNullOrEmpty(variant)) { v += "."; v += variant; }
            GetLine(image, page, line, v);
            SegmRoutine.make_line_segmentation_black(image);
        }

        public void PutLineSegmentation(Intarray image, int page, int line, string variant = null)
        {
            string v = "rseg";
            if (!String.IsNullOrEmpty(variant)) { v += "."; v += variant; }
            Intarray simage = new Intarray();
            simage.Copy(image);
            SegmRoutine.make_line_segmentation_white(simage);
            PutLine(simage, page, line, v);
        }

        public void GetCharSegmentation(Intarray image, int page, int line, string variant = null)
        {
            string v = "cseg";
            if (!String.IsNullOrEmpty(variant)) { v += "."; v += variant; }
            GetLine(image, page, line, v);
            SegmRoutine.make_line_segmentation_black(image);
        }

        public void PutCharSegmentation(Intarray image, int page, int line, string variant = null)
        {
            string v = "cseg";
            if (!String.IsNullOrEmpty(variant)) { v += "."; v += variant; }
            Intarray simage = new Intarray();
            simage.Copy(image);
            SegmRoutine.make_line_segmentation_white(simage);
            PutLine(simage, page, line, v);
        }

        public void GetLattice(IGenericFst fst, int page, int line, string variant = null)
        {
            string s = PathFile(page, line, variant, "fst");
            fst.Load(s);
        }

        public void PutLattice(IGenericFst fst, int page, int line, string variant = null)
        {
            string s = PathFile(page, line, variant, "fst");
            fst.Save(s);
        }
    }
}
