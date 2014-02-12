using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;
using Ocronet.Dynamic.ImgLib;
using Ocronet.Dynamic.Recognizers.Lenet;

namespace Ocronet.Dynamic.Tests
{
    public class TestLenetWrapper
    {
        string strClasses = "0123456789";
        string networkFileName = "netp-ascii";
        string testPngFileName = "test-c3.png";
        string testPpmFileName = "test-c3.ppm";

        public void RunTest()
        {
            // create int array of ascii codes
            int[] classes = new int[strClasses.Length];
            for(int i=0; i<strClasses.Length; i++)
                classes[i] = (int)strClasses[i];
            // init lenet
            IntPtr lenetptr;
            LenetWrapper.CreateLenet(out lenetptr, classes.Length, classes, false, false, true);
            // load network from file
            LenetWrapper.LoadNetwork(lenetptr, networkFileName);

            // read image variant 2
            Bitmap bitmap = ImgIo.LoadBitmapFromFile(testPngFileName);
            int w = bitmap.Width;
            int h = bitmap.Height;
            byte[] buffer = LenetRoutines.ToMatrix<byte>(bitmap, false);

            // run recognize from buffer
            int answer;
            double rate;
            LenetWrapper.RecognizeRawData(lenetptr, buffer, buffer.Length, h, w, out answer, out rate);
            Console.WriteLine("BUFFER: Result index '{0}', rate '{1}'", answer, rate);

            // run recognize from file
            LenetWrapper.RecognizeImageFile(lenetptr, testPpmFileName, out answer, out rate);
            Console.WriteLine("FILE:   Result index '{0}', rate '{1}'", answer, rate);

            // compute outputs
            int[] outclasses = new int[strClasses.Length];
            double[] outenergies = new double[strClasses.Length];
            int getsize;
            LenetWrapper.ComputeOutputs(lenetptr, buffer, buffer.Length, h, w, outclasses, outenergies, out getsize);
            PrintOutputs(classes, outenergies, getsize);
            
            // test CoTaskMemAlloc
            IntPtr ptrArray;
            int ptrSize;
            LenetWrapper.TestDoubleCoTaskMemAlloc(out ptrSize, out ptrArray);
            IntPtr curr = ptrArray;
            double[] valArray = new double[ptrSize];
            Marshal.Copy(ptrArray, valArray, 0, ptrSize);
            Marshal.FreeCoTaskMem(ptrArray);
        }

        private void PrintOutputs(int[] classes, double[] energies, int size)
        {
            Console.WriteLine("OUTPUTS:");
            for (int i = 0; i < size; i++)
            {
                Console.WriteLine("{0}({1,10:0.000}) ", (char)classes[i], energies[i]);
            }
        }
    }
}
