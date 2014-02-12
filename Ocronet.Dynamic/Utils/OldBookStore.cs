using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Ocronet.Dynamic.Interfaces;
using Ocronet.Dynamic.ImgLib;
using System.Drawing;

namespace Ocronet.Dynamic.Utils
{
    public class OldBookStore : IBookStore
    {
        protected string prefix;
        protected Narray<Intarray> lines;

        public OldBookStore()
        {
            lines = new Narray<Intarray>();
        }

        public override int LinesOnPage(int ipage)
        {
            return lines[ipage].Length();
        }

        public override int GetLineId(int ipage, int iline)
        {
            return lines[ipage][iline];
        }

        public override int AddLineId(int ipage, int iline)
        {
            if (lines.Length() <= ipage)
            {
                lines.Resize(ipage + 1);
                for (int i = 0; i < lines.Length(); i++)
                {
                    if (lines[i] == null)
                        lines[i] = new Intarray();
                }
            }
            lines[ipage].Push(iline);
            return lines[ipage].Length() - 1;
        }

        public override int NumberOfPages()
        {
            return lines.Length();
        }

        public override string PathFile(int page, int line = -1, string variant = null, string extension = null)
        {
            char dirSepar = System.IO.Path.DirectorySeparatorChar;
            string file = String.Format("{0}/{1:0000}", prefix, page);
            if (line >= 0)
                file += String.Format("/{0:0000}", line);
            if (!String.IsNullOrEmpty(variant))
                file += String.Format(".{0}", variant);
            if (!String.IsNullOrEmpty(extension))
                file += String.Format(".{0}", extension);
            file = file.Replace('/', dirSepar);
            Global.Debugf("bookstore", "(path: {0})", file);

            return file;
        }

        public override FileStream Open(FileMode mode, int page, int line = -1, string variant = null, string extension = null)
        {
            string s = PathFile(page, line, variant, extension);
            FileAccess faccess = FileAccess.Read;
            if (mode == FileMode.Create || mode == FileMode.CreateNew)
                faccess = FileAccess.Write;
            return new FileStream(s, mode, faccess);
        }

        public override void SetPrefix(string s)
        {
            prefix = s;
            int ndirs = GetMaxPage("[0-9][0-9][0-9][0-9]");
            int npngs = GetMaxPage("([0-9][0-9][0-9][0-9]).png");
            if (!(ndirs < 10000 && npngs < 10000))
                throw new Exception("CHECK: ndirs < 10000 && npngs < 10000");
            int npages = Math.Max(ndirs, npngs);
            lines.Resize(npages);
            for (int i = 0; i < npages; i++)
            {
                lines[i] = new Intarray();
                GetLinesOfPage(lines[i], i);
                Global.Debugf("bookstore", "page {0} #lines {1}", i, lines[i].Length());
            }
        }

        public virtual int GetMaxPage(string fpattern)
        {
            int npages = -1;
            DirPattern dpattern = new DirPattern(prefix, fpattern);
            for (int i = 0; i < dpattern.Length; i++)
            {
                int p = -1;
                int.TryParse(dpattern[i], out p);
                if (p > npages)
                    npages = p;
            }
            npages++;
            return npages;
        }

        protected virtual void GetLinesOfPage(Intarray lines, int ipage)
        {
            lines.Clear();
            string dirName = String.Format("{0}{1}{2:0000}", prefix, Path.DirectorySeparatorChar, ipage);
            DirPattern dpattern = new DirPattern(dirName, @"([0-9][0-9][0-9][0-9])\.png");
            if (dpattern.Length > 0)
                lines.ReserveTo(dpattern.Length);
            List<int> llist = new List<int>(dpattern.Length);
            for (int i = 0; i < dpattern.Length; i++)
            {
                int k = int.Parse(dpattern[i]);
                llist.Add(k);
                //lines.Push(k);
            }
            IEnumerable<int> query = llist.OrderBy(i => i);
            foreach (int iline in query)
                lines.Push(iline);
        }

        protected void MaybeMakeDirectory(int page)
        {
            string s = String.Format("{0}{1}{2:0000}", prefix, Path.DirectorySeparatorChar, page);
            if (!Directory.Exists(s))
                Directory.CreateDirectory(s);
        }

        public override bool GetPage(Bytearray image, int page, string variant = null)
        {
            string s = PathFile(page, -1, variant, "png");
            if (!File.Exists(s)) return false;
            ImgIo.read_image_gray(image, s);
            return true;
        }

        public override bool GetPage(Intarray image, int page, string variant = null)
        {
            string s = PathFile(page, -1, variant, "png");
            if (!File.Exists(s)) return false;
            ImgIo.read_image_packed(image, s);
            return true;
        }

        public override void PutPage(Bytearray image, int page, string variant = null)
        {
            ImgIo.write_image_gray(PathFile(page, -1, variant, "png"), image);
        }

        public override void PutPage(Intarray image, int page, string variant = null)
        {
            ImgIo.write_image_packed(PathFile(page, -1, variant, "png"), image);
        }

        public override bool HaveLine(int page, int line, string variant = null, string extension = "png")
        {
            string s = PathFile(page, line, variant, extension);
            if (!File.Exists(s)) return false;
            return true;
        }

        public override bool GetLine(Bytearray image, int page, int line, string variant = null)
        {
            string s = PathFile(page, line, variant, "png");
            if (!File.Exists(s)) return false;
            ImgIo.read_image_gray(image, s);
            return true;
        }

        public override bool GetLine(Intarray image, int page, int line, string variant = null)
        {
            string s = PathFile(page, line, variant, "png");
            if (!File.Exists(s)) return false;
            ImgIo.read_image_packed(image, s);
            return true;
        }

        public override void PutLine(Bytearray image, int page, int line, string variant = null)
        {
            MaybeMakeDirectory(page);
            FileStream fs = Open(FileMode.Create, page, line, variant, "png");
            ImgIo.write_image_gray(fs, image, System.Drawing.Imaging.ImageFormat.Png);
            fs.Flush();
            fs.Close();
        }

        public override void PutLine(Intarray image, int page, int line, string variant = null)
        {
            MaybeMakeDirectory(page);
            FileStream fs = Open(FileMode.Create, page, line, variant, "png");
            ImgIo.write_image_packed(fs, image, System.Drawing.Imaging.ImageFormat.Png);
            fs.Flush();
            fs.Close();
        }

        public override bool GetLine(out string s, int page, int line, string variant = null)
        {
            s = "";
            if (!File.Exists(PathFile(page, line, variant, "txt")))
                return false;
            FileStream fs = null;
            try
            {
                fs = Open(FileMode.Open, page, line, variant, "txt");
                StreamReader reader = new StreamReader(fs);
                s = reader.ReadLine();
                reader.Close();
                fs.Close();
            }
            catch (FileNotFoundException) { return false; }
            catch (Exception)
            {
                if (fs != null) fs.Close();
                return false;
            }
            return true;
        }

        public override void PutLine(string s, int page, int line, string variant = null)
        {
            MaybeMakeDirectory(page);
            FileStream fs = null;
            try
            {
                fs = Open(FileMode.Create, page, line, variant, "txt");
                StreamWriter writer = new StreamWriter(fs);
                writer.Write(s);
                writer.Close();
                fs.Close();
            }
            catch (Exception e)
            {
                if (fs != null) fs.Close();
                throw e;
            }
        }

        public override bool GetLine(out Bitmap bitmap, int page, int line, string variant = null)
        {
            bitmap = null;
            string s = PathFile(page, line, variant, "png");
            if (!File.Exists(s)) return false;
            bitmap = ImgIo.LoadBitmapFromFile(s);
            return true;
        }
        public override void PutLine(Bitmap bitmap, int page, int line, string variant = null)
        {
            MaybeMakeDirectory(page);
            string s = PathFile(page, line, variant, "png");
            bitmap.Save(s, System.Drawing.Imaging.ImageFormat.Png);
        }


        public override bool GetCosts(Floatarray costs, int page, int line, string variant = null)
        {
            costs.Clear();
            costs.Resize(10000);
            costs.Fill(1e38f);

            if (!File.Exists(PathFile(page, line, variant, "costs")))
                return false;
            FileStream fs = null;
            try
            {
                fs = Open(FileMode.Open, page, line, variant, "costs");
                StreamReader reader = new StreamReader(fs);
                int index;
                float cost;
                while (!reader.EndOfStream)
                {
                    string sline = reader.ReadLine();
                    string[] parts = sline.Split(new char[] { ' ' }, 2);
                    if (parts.Length == 2 && int.TryParse(parts[0], out index) && float.TryParse(parts[1], out cost))
                        costs[index] = cost;
                }
                reader.Close();
                fs.Close();
            }
            catch (FileNotFoundException e) { return false; }
            catch (Exception e)
            {
                if (fs != null) fs.Close();
                return false;
            }
            return true;
        }

        public override void PutCosts(Floatarray costs, int page, int line, string variant = null)
        {
            using(var fs = Open(FileMode.Create, page, line, variant, "costs"))
            using (var writer = new StreamWriter(fs))
            {
                for (int i = 0; i < costs.Length(); i++)
                {
                    writer.WriteLine("{0} {1}", i, costs[i]);
                }
            }
        }


    }
}
