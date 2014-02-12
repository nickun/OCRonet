using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Ocronet.Dynamic.Interfaces;
using Ocronet.Dynamic.Component;
using System.Drawing;

namespace Ocronet.Dynamic.Utils
{
    public class LineSource : IComponent, IEnumerator<BookLine>, IEnumerable<BookLine>
    {
        Narray<IBookStore> bookstores;
        int index;
        string cseg_variant;
        string text_variant;
        Intarray all_lines;
        int bookno;
        int pageno;
        int lineno_;
        int lineno;


        public LineSource()
        {
            PDef("retrain", 0, "set parameters for retraining");
            PDef("randomize", 1, "randomize lines");
            PDef("cbookstore", "SmartBookStore", "bookstore using for reading pages");
            bookstores = new Narray<IBookStore>();
            all_lines = new Intarray();
        }

        public override string Name
        {
            get { return "linesource"; }
        }


        public void Init(params string[] books)
        {
            bool retrain = PGetb("retrain");
            bool randomize = PGetb("randomize");
            string cbookstore = PGet("cbookstore");
            bookstores.Clear();
            all_lines.Clear();
            cseg_variant = "cseg.gt";
            text_variant = "gt";
            if (retrain)
            {
                cseg_variant = "cseg";
                text_variant = "";
            }

            int nbooks = books.Length;
            bookstores.Resize(nbooks);
            int totalNumberOfPages = 0;
            for (int i = 0; i < nbooks; i++)
            {
                bookstores[i] = ComponentCreator.MakeComponent<IBookStore>(cbookstore);
                bookstores[i].SetPrefix(books[i].Trim());
                Global.Debugf("info", "{0}: {1} pages", books[i], bookstores[i].NumberOfPages());
                totalNumberOfPages += bookstores[i].NumberOfPages();
            }
            //CHECK_ARG(totalNumberOfPages > 0, "totalNumberOfPages > 0");

            // compute a list of all lines
            Intarray triple = new Intarray(3);
            for (int i = 0; i < nbooks; i++)
            {
                for (int j = 0; j < bookstores[i].NumberOfPages(); j++)
                {
                    for (int k = 0; k < bookstores[i].LinesOnPage(j); k++)
                    {
                        triple[0] = i;
                        triple[1] = j;
                        triple[2] = k;
                        NarrayRowUtil.RowPush(all_lines, triple);
                    }
                }
            }
            Global.Debugf("info", "got {0} lines", all_lines.Dim(0));

            // randomly permute it so that we train in random order
            if (randomize)
                Shuffle();

            index = 0;
        }

        /// <summary>
        /// Randomly permute it so that we train in random order
        /// </summary>
        public void Shuffle()
        {
            bool randomize = PGetb("randomize");
            if (randomize)
            {
                Intarray permutation = new Intarray(all_lines.Dim(0));
                for (int i = 0; i < all_lines.Dim(0); i++) permutation[i] = i;
                NarrayUtil.RandomlyPermute(permutation);
                NarrayRowUtil.RowPermute(all_lines, permutation);
            }
        }

        /// <summary>
        /// Total books count
        /// </summary>
        public int BooksCount
        {
            get { return bookstores.Length(); }
        }

        /// <summary>
        /// Total lines count
        /// </summary>
        public int Length
        {
            get { return all_lines.Dim(0); }
        }

        /// <summary>
        /// Возвращает набор всех символов базы и их количество
        /// </summary>
        /// <param name="charCounts">количество каждого символа</param>
        /// <returns>набор символов</returns>
        public string GetCharset(out int[] charCounts)
        {
            // содержит набор символов и их количество
            SortedDictionary<char, int> chardict = new SortedDictionary<char, int>();
            // соберем базу символов
            for (int i = 0; i < all_lines.Dim(0); i++)
            {
                int b = all_lines[i, 0];
                int p = all_lines[i, 1];
                int l_ = all_lines[i, 2];
                int l = bookstores[b].GetLineId(p, l_);
                string tr = GetTranscript(b, p, l);
                if (!String.IsNullOrEmpty(tr))
                {
                    foreach (char c in tr)
                    {
                        if (!chardict.ContainsKey(c))
                            chardict.Add(c, 1);
                        else
                            chardict[c]++;
                    }
                }
            }
            // подготовим результат
            StringBuilder sb = new StringBuilder(Math.Max(1, chardict.Count));
            int[] lCharCounts = new int[chardict.Count];
            int ikey = 0;
            foreach (char key in chardict.Keys)
            {
                sb.Append(key);
                lCharCounts[ikey++] = chardict[key];
            }
            // результат
            charCounts = lCharCounts;
            return sb.ToString();
        }

        public IEnumerator<BookLine> GetEnumerator()
        {
            return this;
        }

        public void Reset()
        {
            index = 0;
        }

        public BookLine Current
        {
            get { return new BookLine(bookno, pageno, lineno); }
        }

        public int CurrentBook
        {
            get { return bookno; }
        }

        public int CurrentPage
        {
            get { return pageno; }
        }

        public int CurrentLine
        {
            get { return lineno; }
        }

        public bool Done()
        {
            return index >= all_lines.Dim(0);
        }

        public bool MoveNext()
        {
            if (index >= all_lines.Dim(0))
                return false;
            bookno = all_lines[index, 0];
            pageno = all_lines[index, 1];
            lineno_ = all_lines[index, 2];
            lineno = bookstores[bookno].GetLineId(pageno, lineno_);
            index++;
            return index <= all_lines.Dim(0);
        }

        public string GetPath()
        {
            return bookstores[bookno].PathFile(pageno, lineno);
        }
        public string GetPath(int book, int page, int line)
        {
            return bookstores[book].PathFile(page, line);
        }

        public bool HaveImage()
        {
            return bookstores[bookno].HaveLine(pageno, lineno);
        }

        public bool HaveImage(int book, int page, int line)
        {
            return bookstores[book].HaveLine(page, line);
        }

        public bool HaveCharSegmentation()
        {
            bool have = bookstores[bookno].HaveLine(pageno, lineno, "cseg.gt");
            if (have) return true;
            return bookstores[bookno].HaveLine(pageno, lineno, "cseg");
        }

        public bool HaveCharSegmentation(int book, int page, int line)
        {
            bool have = bookstores[book].HaveLine(page, line, "cseg.gt");
            if (have) return true;
            return bookstores[book].HaveLine(page, line, "cseg");
        }

        public bool HaveTranscript()
        {
            bool have = bookstores[bookno].HaveLine(pageno, lineno, "gt", "txt");
            if (have) return true;
            return bookstores[bookno].HaveLine(pageno, lineno, "", "txt");
        }

        public bool HaveTranscript(int book, int page, int line)
        {
            bool have = bookstores[book].HaveLine(page, line, "gt", "txt");
            if (have) return true;
            return bookstores[book].HaveLine(page, line, "", "txt");
        }

        public string GetTranscript()
        {
            string str;
            if (bookstores[bookno].GetLine(out str, pageno, lineno, "gt"))
                return str;
            if (bookstores[bookno].GetLine(out str, pageno, lineno))
                return str;
            return null;
        }

        public string GetTranscript(int book, int page, int line)
        {
            string str;
            if (bookstores[book].GetLine(out str, page, line, "gt"))
                return str;
            if (bookstores[book].GetLine(out str, page, line))
                return str;
            return null;
        }

        public void PutTranscript(string str, int book, int page, int line)
        {
            bookstores[book].PutLine(str, page, line, "gt");
        }

        public bool GetImage(Bytearray image)
        {
            image.Clear();
            return bookstores[bookno].GetLine(image, pageno, lineno);
        }

        public bool GetImage(Bytearray image, int book, int page, int line)
        {
            image.Clear();
            return bookstores[book].GetLine(image, page, line);
        }

        public Bitmap GetImage()
        {
            Bitmap result;
            bookstores[bookno].GetLine(out result, pageno, lineno);
            return result;
        }
        public Bitmap GetImage(int book, int page, int line)
        {
            Bitmap result;
            bookstores[book].GetLine(out result, page, line);
            return result;
        }
        public void PutImage(Bitmap bitmap, int book, int page, int line)
        {
            bookstores[book].PutLine(bitmap, page, line);
        }

        /// <summary>
        /// Put image to last book and last page.
        /// Return new line number.
        /// </summary>
        public int PutNewImage(Bitmap bitmap)
        {
            int ibook = bookstores.Length() - 1;
            bool emptyBook = bookstores[ibook].NumberOfPages() <= 0;
            int ipage = emptyBook ? 0 : bookstores[ibook].NumberOfPages() - 1;
            int iline = emptyBook ? 0 : bookstores[ibook].LinesOnPage(ipage); // next line pos
            int lineid = iline == 0 ? 0 : bookstores[ibook].GetLineId(ipage, iline - 1) + 1; // next line id
            bookstores[ibook].AddLineId(ipage, lineid);                 // reserve position for line id
            bookstores[ibook].PutLine(bitmap, ipage, lineid);           // put image to reserved position

            // update collection
            Intarray triple = new Intarray(3);
            triple[0] = ibook;
            triple[1] = ipage;
            triple[2] = iline;
            NarrayRowUtil.RowPush(all_lines, triple);
            // set current position
            bookno = ibook;
            pageno = ipage;
            lineno = lineid;
            return lineid;
        }

        public bool GetCharSegmentation(Intarray image)
        {
            image.Clear();
            bookstores[bookno].GetCharSegmentation(image, pageno, lineno, "gt");
            if (image.Length() > 0) return true;
            bookstores[bookno].GetCharSegmentation(image, pageno, lineno, "");
            if (image.Length() > 0) return true;
            return false;
        }

        public bool GetCharSegmentation(Intarray image, int book, int page, int line)
        {
            image.Clear();
            bookstores[book].GetCharSegmentation(image, page, line, "gt");
            if (image.Length() > 0) return true;
            bookstores[book].GetCharSegmentation(image, page, line, "");
            if (image.Length() > 0) return true;
            return false;
        }

        public void PutCharSegmentation(Intarray image, int book, int page, int line)
        {
            bookstores[book].PutCharSegmentation(image, page, line, "gt");
        }

        public bool GetCosts(Floatarray costs)
        {
            return bookstores[bookno].GetCosts(costs, pageno, lineno);
        }



        public void Dispose()
        {
        }

        object IEnumerator.Current
        {
            get { return lineno; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }
    }
}
