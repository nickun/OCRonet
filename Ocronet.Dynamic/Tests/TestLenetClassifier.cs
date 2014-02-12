using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using Ocronet.Dynamic.Interfaces;
using Ocronet.Dynamic.Component;
using Ocronet.Dynamic.ImgLib;
using Ocronet.Dynamic.Debug;
using Ocronet.Dynamic.Utils;
using Ocronet.Dynamic.IOData;
using Ocronet.Dynamic.Segmentation;
using Ocronet.Dynamic.Recognizers;
using Ocronet.Dynamic.Recognizers.Lenet;

namespace Ocronet.Dynamic.Tests
{
    public class TestLenetClassifier
    {
        string strClassesNums = "0123456789";
        string strClassesAlphabet = "0123456789";
        int[] classesNums;
        int[] classesAlphabet;
        string oldformatFileName = "netp-ascii";
        string networkFileName = "netp-ascii.cmodel";
        string trainNetworkFileName = "netp-ascii-train.cmodel";
        string trainDatasetFileName = "t10k.dsr8";
        string testPngFileName = "test-c3.png";

        public TestLenetClassifier()
        {
            // create int array of ascii codes
            classesNums = new int[strClassesNums.Length];
            for (int i = 0; i < strClassesNums.Length; i++)
                classesNums[i] = (int)strClassesNums[i];
        }

        public void TestRecognize()
        {
            LenetClassifier classifier = new LenetClassifier();
            classifier.Load(networkFileName);

            StringBuilder sbout;
            classifier.GetStdout(out sbout);
            Console.Write(sbout);

            DoTestRecognize(classifier);
        }

        private void DoTestRecognize(LenetClassifier classifier)
        {
            OutputVector ov = new OutputVector();
            Floatarray v = new Floatarray();
            Bytearray ba = new Bytearray(1, 1);
            ImgIo.read_image_gray(ba, testPngFileName);
            NarrayUtil.Sub(255, ba);
            v.Copy(ba);
            v /= 255.0;
            classifier.XOutputs(ov, v);
            Console.WriteLine("Featured output class '{0}', score '{1}'", (char)ov.Key(ov.BestIndex), ov.Value(ov.BestIndex));
        }



        public LenetClassifier TestLoadNetwork()
        {
            // load network from new format
            LenetClassifier classifier = new LenetClassifier();
            classifier.Load(networkFileName);

            return classifier;
        }


        public void TestSaveNetwork()
        {
            // create lenet
            LenetClassifier classifier = new LenetClassifier();
            classifier.CharClass.TanhSigmoid = false;
            classifier.CharClass.NetNorm = false;
            classifier.CharClass.AsciiTarget = true;
            classifier.JunkClass.TanhSigmoid = false;
            classifier.Set("junk", 0);      // disable junk
            classifier.SetExtractor("scaledfe");
            classifier.Initialize(classesNums);

            // load char lenet from old file format
            LenetWrapper.LoadNetwork(classifier.CharClass.HrLenet, oldformatFileName);

            // save network to new format
            classifier.Save(networkFileName);
        }


        public void TestTrainSimple()
        {
            // create lenet
            LenetClassifier classifier = new LenetClassifier();
            classifier.Set("junk", 0);      // disable junk
            classifier.SetExtractor("scaledfe");
            classifier.Initialize(classesNums);

            StringBuilder sbout;
            classifier.GetStdout(out sbout);
            Console.Write(sbout);

            // load RowDataset8 from file
            RowDataset8 ds = new RowDataset8();
            ds.Load(trainDatasetFileName);

            // do train
            classifier.Set("epochs", 3);
            classifier.XTrain(ds);

            // save classifier to file
            classifier.Save(trainNetworkFileName);

            // test recognize
            DoTestRecognize(classifier);
        }


        public void TestArrays()
        {
            string imgfn = "test-c3.png";

            // load Bytearray
            Bytearray ba = new Bytearray(1, 1);
            ImgIo.read_image_gray(ba, imgfn);
            OcrRoutine.Invert(ba);
            //NarrayUtil.Sub((byte)255, image);
            byte[] bytes1 = ba.To1DArray();
            NarrayShow.ShowConsole(ba);
            StdInput linput1 = new StdInput(ba);
            Console.WriteLine();

            // load StdInput
            Bitmap bitmap = ImgIo.LoadBitmapFromFile(imgfn);
            StdInput linput2 = StdInput.FromBitmap(bitmap);
            //NarrayShow.ShowConsole(linput2);

            // test convert
            Floatarray fa = linput2.ToFloatarray();
            StdInput linput3 = new StdInput(fa);

            Console.WriteLine("Arrays is identical? {0}", Equals(linput1.GetDataBuffer(), linput2.GetDataBuffer()));
            Console.WriteLine("Arrays is identical? {0}", Equals(linput2.GetDataBuffer(), linput3.GetDataBuffer()));
        }


        private bool Equals(byte[] b1, byte[] b2)
        {
            if (b1.Length != b2.Length)
                return false;
            for (int i = 0; i < b1.Length; i++)
                if (b1[i] != b2[i])
                    return false;
            return true;
        }

    }
}
