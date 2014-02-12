using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ocronet.Dynamic.ImgLib;

namespace Ocronet.Dynamic.Utils
{
    public class OcrRoutine
    {
        /// <summary>
        /// Check background. Abort on detecting an inverted image.
        /// </summary>
        public static bool bgcheck = false;

        public static void Invert(Bytearray a)
        {
            int n = a.Length1d();
            for (int i = 0; i < n; i++)
            {
                a.Put1d(i, (byte)(255 - a.At1d(i)));
            }
        }

        public static int average_on_border(Bytearray a)
        {
            int sum = 0;
            int right = a.Dim(0) - 1;
            int top = a.Dim(1) - 1;
            for(int x = 0; x < a.Dim(0); x++)
                sum += a[x, 0];
            for(int x = 0; x < a.Dim(0); x++)
                sum += a[x, top];
            for(int y = 1; y < top; y++)
                sum += a[0, y];
            for(int y = 1; y < top; y++)
                sum += a[right, y];
            // If average border intensity is between 127-128, inverting the
            // image does not work correctly
            float average_border_intensity = sum / ((right + top) * 2.0f);
            if (!(average_border_intensity <= 127 || average_border_intensity >= 128))
                Console.WriteLine("average border intensity is between 127-128, inverting the image does not work correctly");
            return sum / ((right + top) * 2);
        }
        
        public static bool background_seems_black(Bytearray a)
        {
            return average_on_border(a) <= (NarrayUtil.Min(a) + NarrayUtil.Max(a) / 2);
        }

        public static bool background_seems_white(Bytearray a)
        {
            return average_on_border(a) >= (NarrayUtil.Min(a) + NarrayUtil.Max(a) / 2);
        }

        public static void optional_check_background_is_darker(Bytearray a) {
            if (bgcheck)
                if (!background_seems_black(a))
                    throw new Exception("background must be black");
        }

        public static void optional_check_background_is_lighter(Bytearray a)
        {
            if (bgcheck)
                if (!background_seems_white(a))
                    throw new Exception("background must be white");
        }

        public static int binarize_simple(Bytearray result, Bytearray image)
        {
            int threshold = (NarrayUtil.Max(image)/* + NarrayUtil.Min(image)*/) / 2;
            result.MakeLike(image);
            for (int i = 0; i < image.Length1d(); i++)
                result.Put1d(i, image.At1d(i) < threshold ? (byte)0 : (byte)255);
            return threshold;
        }

        public static int binarize_simple(Bytearray image)
        {
            return binarize_simple(image, image);
        }

        public static void binarize_with_threshold(Bytearray result, Bytearray image, int threshold)
        {
            result.MakeLike(image);
            for (int i = 0; i < image.Length1d(); i++)
                result.Put1d(i, image.At1d(i) < threshold ? (byte)0 : (byte)255);
        }

        public static void binarize_with_threshold(Bytearray result, Floatarray image, float threshold)
        {
            result.MakeLike(image);
            for (int i = 0; i < image.Length1d(); i++)
                result.Put1d(i, image.At1d(i) < threshold ? (byte)0 : (byte)255);
        }

        public static void binarize_with_threshold(Bytearray image, int threshold)
        {
            binarize_with_threshold(image, image, threshold);
        }

        public static void threshold_frac(Bytearray thresholded, Floatarray input, float frac)
        {
            float minofinput = NarrayUtil.Min(input);
            float theta = frac * (NarrayUtil.Max(input) - minofinput) + minofinput;
            binarize_with_threshold(thresholded, input, theta);
        }

        public static void binsmooth(Bytearray binary, Floatarray input, float sigma)
        {
            Floatarray smoothed = new Floatarray();
            smoothed.Copy(input);
            smoothed -= NarrayUtil.Min(smoothed);
            smoothed /= NarrayUtil.Max(smoothed);
            if (sigma > 0)
                Gauss.Gauss2d(smoothed, sigma, sigma);
            binarize_with_threshold(binary, smoothed, 0.5f);
        }

        public static void scale_to(Floatarray v, Floatarray sub, int csize, float noupscale=1.0f, float aa=1.0f)
        {
            // compute the scale factor
            float s = Math.Max(sub.Dim(0), sub.Dim(1))/(float)csize;

            // don't upscale if that's prohibited
            if(s < noupscale)
                s = 1.0f;

            // compute the offset to keep the input centered in the output
            float dx = (csize*s-sub.Dim(0))/2;
            float dy = (csize*s-sub.Dim(1))/2;

            // antialiasing via Gaussian convolution
            float sig = s * aa;
            if(sig > 1e-3f)
                Gauss.Gauss2d(sub, sig, sig);

            // now compute the output image via bilinear interpolation
            v.Resize(csize, csize);
            v.Fill(0f);
            for (int i = 0; i < csize; i++)
            {
                for (int j = 0; j < csize; j++)
                {
                    float x = i * s - dx;
                    float y = j * s - dy;
                    if (x < 0 || x >= sub.Dim(0)) continue;
                    if (y < 0 || y >= sub.Dim(1)) continue;
                    float value = ImgOps.bilin(sub, x, y);
                    v[i, j] = value;
                }
            }
        }

        public static void skeletal_features(Bytearray endpoints, Bytearray junctions,
                           Bytearray image, float presmooth, float skelsmooth)
        {
            Bytearray temp = new Bytearray();
            temp.Copy(image);
            NarrayUtil.Greater(temp, (byte)128, (byte)0, (byte)255);
            if (presmooth > 0f)
            {
                Gauss.Gauss2d(temp, presmooth, presmooth);
                NarrayUtil.Greater(temp, (byte)128, (byte)0, (byte)255);
            }
        }

    }
}
