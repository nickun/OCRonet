using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ocronet.Dynamic.Segmentation;
using Ocronet.Dynamic.Utils;
using Ocronet.Dynamic.ImgLib;

namespace Ocronet.Dynamic.Recognizers
{
    public class StandardExtractor : IExtractor
    {
        public override string Name
        {
            get { return "StandardExtractor"; }
        }

        public StandardExtractor()
        {
            PDef("csize", 30, "taget image size");
            PDef("aa", 0, "anti-aliasing");
            PDef("noupscale", 0.5, "no upscaling for scales smaller than this");
            PDef("threshold", 0.25, "threshold for finding the largest cc");
            PDef("minsize", 0.2, "minimum size of connected components to be kept, in terms of largest");
            PDef("gradsigma", 1.0, "gaussian convolution prior to gradient computation");
            PDef("n", 10, "number of smoothing steps");
            PDef("step", 0.3, "amount of smoothing");
            PDef("pad", 1, "amount to pad bounding rectangle by");
            PDef("binsmooth", 1, "use smoothing instead of morphology");
        }

        public override void Extract(Narray<Floatarray> outarrays, Floatarray inarray)
        {
            outarrays.Clear();
            Floatarray input = new Floatarray();
            input.Copy(inarray);
            int w = input.Dim(0), h = input.Dim(1);
            Floatarray a = new Floatarray();            // working array
            int csize = PGeti("csize");

            // get rid of small components
            SegmRoutine.erase_small_components(input, PGetf("minsize"), PGetf("threshold"));

            // compute a thresholded version for morphological operations
            Bytearray thresholded = new Bytearray();
            OcrRoutine.threshold_frac(thresholded, input, PGetf("threshold"));

            // compute a smoothed version of the input for gradient computations
            float sigma = PGetf("gradsigma");
            Floatarray smoothed = new Floatarray();
            smoothed.Copy(input);
            Gauss.Gauss2d(smoothed, sigma, sigma);

            // x gradient
            a.Resize(w, h);
            for (int j = 0; j < h; j++)
            {
                for (int i = 0; i < w; i++)
                {
                    float delta;
                    if (i == 0) delta = 0f;
                    else delta = smoothed[i, j] - smoothed[i - 1, j];
                    a[i, j] = delta;
                }
            }
            Floatarray xgrad = outarrays.Push(new Floatarray());
            OcrRoutine.scale_to(xgrad, a, csize, PGetf("noupscale"), PGetf("aa"));
            for (int j = 0; j < csize; j++)
            {
                for (int i = 0; i < csize; i++)
                {
                    if (j % 2 == 0) xgrad[i, j] = Math.Max(xgrad[i, j], 0f);
                    else xgrad[i, j] = Math.Min(xgrad[i, j], 0f);
                }
            }

            // y gradient
            a.Resize(w, h);
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    float delta;
                    if (j == 0) delta = 0f;
                    else delta = smoothed[i, j] - smoothed[i, j - 1];
                    a[i, j] = delta;
                }
            }
            Floatarray ygrad = outarrays.Push(new Floatarray());
            OcrRoutine.scale_to(ygrad, a, csize, PGetf("noupscale"), PGetf("aa"));
            for (int i = 0; i < csize; i++)
            {
                for (int j = 0; j < csize; j++)
                {
                    if (i % 2 == 0) ygrad[i, j] = Math.Max(ygrad[i, j], 0f);
                    else ygrad[i, j] = Math.Min(ygrad[i, j], 0f);
                }
            }

            // junctions, endpoints, and holes
            Floatarray junctions = new Floatarray();
            Floatarray endpoints = new Floatarray();
            Floatarray holes = new Floatarray();
            Bytearray junctions1 = new Bytearray();
            Bytearray endpoints1 = new Bytearray();
            Bytearray holes1 = new Bytearray();
            Bytearray dilated = new Bytearray();
            Bytearray binary = new Bytearray();
            junctions.MakeLike(input, 0f);
            endpoints.MakeLike(input, 0f);
            holes.MakeLike(input, 0f);
            int n = PGeti("n");
            float step = PGetf("step");
            int bs = PGeti("binsmooth");
            for(int i=0; i<n; i++)
            {
                sigma = step * i;
                if(bs > 0)
                    OcrRoutine.binsmooth(binary, input, sigma);
                else
                {
                    binary.Copy(thresholded);
                    Morph.binary_dilate_circle(binary, (int)(sigma));
                }
                OcrRoutine.skeletal_features(endpoints1, junctions1, binary, 0.0f, 0.0f);
                NarrayUtil.Greater(junctions1, (byte)0, (byte)0, (byte)1);
                junctions.Copy(junctions1);
                NarrayUtil.Greater(endpoints1, (byte)0, (byte)0, (byte)1);
                endpoints.Copy(endpoints1);
                SegmRoutine.extract_holes(ref holes1, binary);
                NarrayUtil.Greater(holes1, (byte)0, (byte)0, (byte)1);
                holes.Copy(holes1);
            }
            junctions *= 1.0f / (float)n;
            endpoints *= 1.0f / (float)n;
            holes *= 1.0f / (float)n;

            OcrRoutine.scale_to(outarrays.Push(new Floatarray()), junctions, csize, PGetf("noupscale"), PGetf("aa"));
            OcrRoutine.scale_to(outarrays.Push(new Floatarray()), endpoints, csize, PGetf("noupscale"), PGetf("aa"));
            OcrRoutine.scale_to(outarrays.Push(new Floatarray()), holes, csize, PGetf("noupscale"), PGetf("aa"));
        }

    }
}
