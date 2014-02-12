using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ocronet.Dynamic.Interfaces;
using Ocronet.Dynamic.Utils;
using Ocronet.Dynamic.ImgLib;

namespace Ocronet.Dynamic.Binarize
{
    /// <summary>
    /// An implementation of Otsu's global thresholding method
    /// </summary>
    public class BinarizeByOtsu : IBinarize
    {
        public static int MAXVAL = 256;

        public override string Description
        {
            get { return "An implementation of Otsu's binarization algorithm."; }
        }

        public override string Name
        {
            get { return "binotsu"; }
        }

        public BinarizeByOtsu()
        {
            PDef("debug_otsu", 0, "output the result of binarization");
        }

        public void Binarize(Bytearray outarray, Floatarray inarray)
        {
            Bytearray image = new Bytearray();
            image.Copy(inarray);
            Binarize(outarray, image);
        }

        public override void Binarize(Bytearray bin_image, Bytearray gray_image)
        {
            if(bin_image.Length1d() != gray_image.Length1d())
                bin_image.MakeLike(gray_image);

            if(NarrayUtil.contains_only(gray_image, (byte)0, (byte)255))
            {
                bin_image.Copy(gray_image);
                return;
            }

            int image_width  = gray_image.Dim(0);
            int image_height = gray_image.Dim(1);
            int[]    hist = new int[MAXVAL];
            double[] pdf = new double[MAXVAL]; //probability distribution
            double[] cdf = new double[MAXVAL]; //cumulative probability distribution
            double[] myu = new double[MAXVAL];   // mean value for separation
            double max_sigma;
            double[] sigma = new double[MAXVAL]; // inter-class variance

            /* Histogram generation */
            for(int i=0; i<MAXVAL; i++){
                hist[i] = 0;
            }
            for(int x=0; x<image_width; x++){
                for(int y=0; y<image_height; y++){
                    hist[gray_image[x,y]]++;
                }
            }

            /* calculation of probability density */
            for(int i=0; i<MAXVAL; i++){
                pdf[i] = (double)hist[i] / (image_width * image_height);
            }

            /* cdf & myu generation */
            cdf[0] = pdf[0];
            myu[0] = 0.0;       /* 0.0 times prob[0] equals zero */
            for(int i=1; i<MAXVAL; i++){
                cdf[i] = cdf[i-1] + pdf[i];
                myu[i] = myu[i-1] + i*pdf[i];
            }

            /* sigma maximization
               sigma stands for inter-class variance
               and determines optimal threshold value */
            int threshold = 0;
            max_sigma = 0.0;
            for(int i=0; i<MAXVAL-1; i++){
                if(cdf[i] != 0.0 && cdf[i] != 1.0){
                    double p1p2 = cdf[i]*(1.0 - cdf[i]);
                    double mu1mu2diff = myu[MAXVAL-1]*cdf[i]-myu[i];
                    sigma[i] = mu1mu2diff * mu1mu2diff / p1p2;
                }
                else
                    sigma[i] = 0.0;
                if(sigma[i] > max_sigma){
                    max_sigma = sigma[i];
                    threshold = i;
                }
            }


            for(int x=0; x<image_width; x++){
                for(int y=0; y<image_height; y++){
                     if (gray_image[x,y] > threshold)
                        bin_image[x,y] = (byte)(MAXVAL-1);
                    else
                        bin_image[x,y] = 0;
                }
            }

            if(PGeti("debug_otsu") > 0) {
                Logger.Default.Format("Otsu threshold value = {0}\n", threshold);
                //ImgIo.write_image_gray("debug_otsu.png", bin_image);
            }
        }
    }
}
