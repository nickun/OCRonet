using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ocronet.Dynamic.Recognizers;
using Ocronet.Dynamic.OcroFST;
using Ocronet.Dynamic;
using Ocronet.Dynamic.ImgLib;
using Ocronet.Dynamic.Interfaces;
using Ocronet.Dynamic.Utils;
using Ocronet.Dynamic.IOData;
using System.IO;
using Ocronet.Dynamic.Segmentation;
using Ocronet.Dynamic.Recognizers.Lenet;

namespace Ocronet.DynamicConsole
{
    public class TestLinerec
    {
        public void TestSimple()
        {
            Global.SetEnv("debug", Global.GetEnv("debug") + "");
            // image file name to recognize
            string imgFileName = "line.png";
            string imgCsegFileName = "line.cseg.png";
            string imgTranscriptFileName = "line.txt";

            // line recognizer
            Linerec lrec = (Linerec)Linerec.LoadLinerec("default.model");
            //Linerec lrec = (Linerec)Linerec.LoadLinerec("2m2-reject.cmodel");
            //Linerec lrec = (Linerec)Linerec.LoadLinerec("multi3.cmodel");
            //Linerec lrec = (Linerec)Linerec.LoadLinerec("latin-ascii.model");
            lrec.Info();

            // language model
            OcroFST default_lmodel = OcroFST.MakeOcroFst();
            default_lmodel.Load("default.fst");
            OcroFST lmodel = default_lmodel;

            // read image
            Bytearray image = new Bytearray(1, 1);
            ImgIo.read_image_gray(image, imgFileName);

            // recognize!
            OcroFST fst = OcroFST.MakeOcroFst();
            Intarray rseg = new Intarray();
            lrec.RecognizeLine(rseg, fst, image);

            // show result 1
            string resStr;
            fst.BestPath(out resStr);
            Console.WriteLine("Fst BestPath:   {0}", resStr);

            // find result 2
            Intarray v1 = new Intarray();
            Intarray v2 = new Intarray();
            Intarray inp = new Intarray();
            Intarray outp = new Intarray();
            Floatarray c = new Floatarray();

            BeamSearch.beam_search(v1, v2, inp, outp, c, fst, lmodel, 100);

            FstUtil.remove_epsilons(out resStr, outp);
            Console.WriteLine("Fst BeamSearch: {0}", resStr);

            Intarray cseg = new Intarray();
            SegmRoutine.rseg_to_cseg(cseg, rseg, inp);
            SegmRoutine.make_line_segmentation_white(cseg);
            ImgLabels.simple_recolor(cseg); // for human readable
            ImgIo.write_image_packed(imgCsegFileName, cseg);
            File.WriteAllText(imgTranscriptFileName, resStr.Replace(" ", ""));
        }


        public void TestTrainLenetCseg()
        {
            string bookPath = "data\\0000\\";
            string netFileName = "latin-lenet.model";

            Linerec.GDef("linerec", "use_reject", 1);
            Linerec.GDef("lenet", "junk", 1);
            Linerec.GDef("lenet", "epochs", 4);
            
            // create Linerec
            Linerec linerec;
            if (File.Exists(netFileName))
                linerec = Linerec.LoadLinerec(netFileName);
            else
            {
                linerec = new Linerec("lenet");
                LenetClassifier classifier = linerec.GetClassifier() as LenetClassifier;
                if (classifier != null)
                    classifier.InitNumSymbLatinAlphabet();
            }

            // temporary disable junk
            //linerec.DisableJunk = true;

            linerec.StartTraining();
            int nepochs = 10;
            LineSource lines = new LineSource();
            lines.Init(new string[] { "data2" });

            //linerec.GetClassifier().Set("epochs", 1);

            for (int epoch = 1; epoch <= nepochs; epoch++)
            {
                linerec.Epoch(epoch);

                // load cseg samples
                while (!lines.Done())
                {
                    lines.MoveNext();
                    Intarray cseg = new Intarray();
                    //Bytearray image = new Bytearray();
                    string transcript = lines.GetTranscript();
                    //lines.GetImage(image);
                    if (!lines.GetCharSegmentation(cseg) && cseg.Length() == 0)
                    {
                        Global.Debugf("warn", "skipping book {0} page {1} line {2} (no or bad cseg)",
                            lines.CurrentBook, lines.CurrentPage, lines.Current);
                        continue;
                    }
                    SegmRoutine.make_line_segmentation_black(cseg);
                    linerec.AddTrainingLine(cseg, transcript);
                }

                lines.Reset();
                lines.Shuffle();
                // do Train and clear Dataset
                linerec.FinishTraining();
                // do save
                if (epoch % 1 == 0)
                    linerec.Save(netFileName);

                // recognize test line
                bool bakDisJunk = linerec.DisableJunk;
                linerec.DisableJunk = false;
                DoTestLinerecRecognize(linerec, "data2\\", "test1.png");
                linerec.DisableJunk = bakDisJunk;
            }

            // finnaly save
            linerec.Save(netFileName);
        }


        public void TestTrainLatinCseg()
        {
            string bookPath = "data\\0000\\";
            string netFileName = "latin-amlp.model";

            Linerec.GDef("linerec", "use_reject", 1);
            Linerec.GDef("lenet", "junk", 1);

            // create Linerec
            Linerec linerec;
            if (File.Exists(netFileName))
                linerec = Linerec.LoadLinerec(netFileName);
            else
            {
                linerec = new Linerec("latin");
            }

            // temporary disable junk
            //linerec.DisableJunk = true;

            linerec.StartTraining();
            int nepochs = 1;
            LineSource lines = new LineSource();
            lines.Init(new string[] { "data2" });

            for (int epoch = 1; epoch <= nepochs; epoch++)
            {
                linerec.Epoch(epoch);

                // load cseg samples
                while (lines.MoveNext())
                {
                    Intarray cseg = new Intarray();
                    //Bytearray image = new Bytearray();
                    string transcript = lines.GetTranscript();
                    //lines.GetImage(image);
                    if (!lines.GetCharSegmentation(cseg) && cseg.Length() == 0)
                    {
                        Global.Debugf("warn", "skipping book {0} page {1} line {2} (no or bad cseg)",
                            lines.CurrentBook, lines.CurrentPage, lines.CurrentLine);
                        continue;
                    }
                    SegmRoutine.make_line_segmentation_black(cseg);
                    linerec.AddTrainingLine(cseg, transcript);
                }

                lines.Reset();
                lines.Shuffle();
                // do Train and clear Dataset
                linerec.FinishTraining();
                // do save
                if (epoch % 1 == 0)
                    linerec.Save(netFileName);

                // recognize test line
                bool bakDisJunk = linerec.DisableJunk;
                linerec.DisableJunk = false;
                DoTestLinerecRecognize(linerec, bookPath, "000010.png");
                linerec.DisableJunk = bakDisJunk;
            }

            // finnaly save
            linerec.Save(netFileName);
        }


        private void DoTestLinerecRecognize(Linerec linerec, string bookPath, string filename)
        {
            Bytearray image = new Bytearray();
            ImgIo.read_image_gray(image, bookPath + filename);
            // recognize!
            OcroFST fst = OcroFST.MakeOcroFst();
            linerec.RecognizeLine(fst, image);
            // show result
            string resStr;
            fst.BestPath(out resStr);
            Console.WriteLine("Fst BestPath:   {0}", resStr);
        }

        public void TestRecognizeCseg()
        {
            string book1Path = "data2\\0001\\";
            string book2Path = "data\\0000\\";

            //Linerec.GDef("linerec", "use_reject", 0);
            //Linerec.GDef("lenet", "junk", 0);
            Linerec linerec = Linerec.LoadLinerec("latin-amlp.model");
            //Linerec linerec = Linerec.LoadLinerec("latin-lenet.model");
            //Linerec linerec = Linerec.LoadLinerec("latin-ascii2.model");
            //Linerec linerec = Linerec.LoadLinerec("default.model");
            //linerec.Set("maxcost", 20);
            DoTestLinerecRecognize(linerec, book1Path, "0010.png");
            DoTestLinerecRecognize(linerec, book1Path, "0001.png");
            DoTestLinerecRecognize(linerec, book1Path, "0089.png");
            DoTestLinerecRecognize(linerec, book1Path, "0026.png");
            DoTestLinerecRecognize(linerec, book2Path, "000001.png");
        }


        public void TestComputeMissingCseg()
        {
            //ComputeMissingCsegForBookStore("data", "default.model", "");
            //ComputeMissingCsegForBookStore("data2", "latin-ascii.model", "gt");
            ComputeMissingCsegForBookStore("data", "latin-lenet.model", "");
        }

        /// <summary>
        /// Create char segmentation (cseg) files if missing
        /// </summary>
        /// <param name="bookPath">path to bookstore</param>
        /// <param name="modelFilename">Linerec model file</param>
        /// <param name="langModel">language model file</param>
        /// <param name="suffix">e.g., 'gt'</param>
        public void ComputeMissingCsegForBookStore(string bookPath, string model = "default.model",
            string suffix = "", bool saveRseg = false, string langModel = "default.fst")
        {
            // create line recognizer
            Linerec linerec = Linerec.LoadLinerec(model);

            // create IBookStore
            IBookStore bookstore = new SmartBookStore();
            bookstore.SetPrefix(bookPath);
            bookstore.Info();

            // language model
            OcroFST lmodel = OcroFST.MakeOcroFst();
            lmodel.Load(langModel);

            // iterate lines of pages
            for (int page = 0; page < bookstore.NumberOfPages(); page++)
            {
                int nlines = bookstore.LinesOnPage(page);
                Console.WriteLine("Page {0} has {1} lines", page, nlines);
                for (int j = 0; j < nlines; j++)
                {
                    int line = bookstore.GetLineId(page, j);

                    Bytearray image = new Bytearray();
                    bookstore.GetLine(image, page, line);
                    Intarray cseg = new Intarray();
                    bookstore.GetCharSegmentation(cseg, page, line, suffix);
                    // check missing cseg file
                    if (cseg.Length() <= 0 && image.Length() > 0)
                    {
                        // recognize line
                        OcroFST fst = OcroFST.MakeOcroFst();
                        Intarray rseg = new Intarray();
                        linerec.RecognizeLine(rseg, fst, image);
                        // find best results
                        string resText;
                        Intarray inp = new Intarray();
                        Floatarray costs = new Floatarray();
                        double totalCost = BeamSearch.beam_search(out resText, inp, costs, fst, lmodel, 100);
                        Console.WriteLine(bookstore.PathFile(page, line, suffix));
                        Console.Write("  beam_search score: {0}", totalCost);
                        /*string resText2;
                        fst.BestPath(out resText2);*/

                        // write cseg to bookstore
                        string trans;
                        bookstore.GetLine(out trans, page, line, suffix);
                        resText = resText.Replace(" ", "");
                        if (String.IsNullOrEmpty(trans))
                        {
                            bookstore.PutLine(resText, page, line, suffix);
                            Console.Write("; transcript saved");
                        }
                        else if (trans == resText)
                        {
                            // convert inputs and rseg to cseg
                            SegmRoutine.rseg_to_cseg(cseg, rseg, inp);
                            bookstore.PutCharSegmentation(cseg, page, line, suffix);
                            Console.Write("; cseg saved");
                        }
                        else if (saveRseg)
                        {
                            // convert inputs and rseg to cseg
                            SegmRoutine.rseg_to_cseg(cseg, rseg, inp);
                            //SegmRoutine.remove_small_components(cseg, 4);
                            /*bookstore.PutCharSegmentation(cseg, page, line, suffix);
                            Console.Write("; cseg saved");*/
                            SegmRoutine.make_line_segmentation_white(cseg);
                            ImgLabels.simple_recolor(cseg);
                            string v = "rseg";
                            if (!String.IsNullOrEmpty(suffix)) { v += "."; v += suffix; }
                            string rsegpath = bookstore.PathFile(page, line, v, "png");
                            ImgIo.write_image_packed(rsegpath, cseg);
                            Console.Write("; rseg saved");
                        }
                        Console.WriteLine();
                        
                    }
                }
            }

        }


    }
}
