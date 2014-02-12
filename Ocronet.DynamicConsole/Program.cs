using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using Ocronet.Dynamic;
using Ocronet.Dynamic.ImgLib;
using Ocronet.Dynamic.Segmentation.Line;
using Ocronet.Dynamic.Interfaces;
using Ocronet.Dynamic.Binarize;
using Ocronet.Dynamic.IOData;
using Ocronet.Dynamic.OcroFST;
using Ocronet.Dynamic.Recognizers.Lenet;
using Ocronet.Dynamic.Utils;
using Ocronet.Dynamic.Tests;

namespace Ocronet.DynamicConsole
{

    class Program
    {
        static void Main(string[] args)
        {
            //MlpClassifier mlp = new MlpClassifier();
            /*OcroFST fst = OcroFST.MakeFst("OcroFST");
            fst.Load("ocr-dict-case.fst");

            Console.WriteLine("{0} ({1})", fst.Name, fst.Interface);
            Console.WriteLine("{0}..{1}", fst.GetStart(), fst.nStates());
            fst.Save("test.fst");*/

            //TestDataSet ds = new TestDataSet();
            /*Console.WriteLine("Interface: {0}", ds.Interface);
            Console.WriteLine("Name: {0}", ds.Name);
            Console.WriteLine("Description: {0}", ds.Description);*/

            /*Console.WriteLine(String.Format("{0:X6}", 15));
            DirPattern dpatt1 = new DirPattern("data2", @"[0-9][0-9][0-9][0-9]");
            Console.WriteLine("Count: {0}", dpatt1.Length);
            DirPattern dpatt2 = new DirPattern("data2", @"([0-9][0-9][0-9][0-9])\.png");
            Console.WriteLine("Count: {0}", dpatt2.Length);

            Console.WriteLine("Exist: {0}",
                DirPattern.Exist("data2", @"[0-9][0-9][0-9][0-9]", @"([0-9][0-9][0-9][0-9][0-9][0-9])\.png"));*/

            //new TestBookStore().RunTest();


            Ocronet.Dynamic.Utils.Logger.Default.verbose = true;

            //new TestLenetWrapper().RunTest();
            //TestLenetClassifier testLenet = new TestLenetClassifier();
            //testLenet.TestArrays();
            //testLenet.TestSaveNetwork();
            //testLenet.TestRecognize();
            
            //new TestDataset().TestRowDataset();

            TestLinerec testLinerec = new TestLinerec();
            //testLinerec.TestSimple();
            //testLinerec.TestTrainLatinCseg();
            testLinerec.TestTrainLenetCseg();
            //testLinerec.TestRecognizeCseg();
            //testLinerec.TestComputeMissingCseg();
            //testLinerec.TestSimple();

            if (args.Length == 0)
            {
                Console.WriteLine("Usage: DynamicConsole.exe imagefile");
                return;
            }

            //args[0] = "scan1.png";
            //args[0] = "lenna.jpg";
            string fileNameWoExt = Path.GetFileNameWithoutExtension(args[0]);

            Bytearray image = new Bytearray(1, 1);
            //Intarray image = new Intarray(1, 1);
            ImgIo.read_image_gray(image, args[0]);
            Bytearray binimage = new Bytearray(1, 1);
            Intarray segmimage = new Intarray(1, 1);

            IBinarize binarizer = new BinarizeByOtsu();
            //binarizer.Set("k", 0.05);
            //binarizer.Set("w", 5);
            ISegmentLine segmenter = new DpSegmenter();

            binarizer.Binarize(binimage, image);    
            ImgIo.write_image_gray(fileNameWoExt + ".bin.png", binimage);

            //new BinarizeByOtsu().Binarize(binimage, image);
            segmenter.Charseg(ref segmimage, binimage);
            ImgLabels.simple_recolor(segmimage);
            ImgIo.write_image_packed(fileNameWoExt + ".rseg.png", segmimage);
        }
    }
}
