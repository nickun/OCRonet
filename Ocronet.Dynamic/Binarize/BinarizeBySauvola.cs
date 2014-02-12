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
    /// Best binarization algorithm
    /// </summary>
    public class BinarizeBySauvola : IBinarize
    {
        public static int MAXVAL = 256;
        protected float k;
        protected int w;
        protected int whalf; // Half of window size

        public override string Description
        {
            get { return "An efficient implementation of the Sauvola's document binarization algorithm based on integral images."; }
        }

        public override string Name
        {
            get { return "binsauvola"; }
        }

        public BinarizeBySauvola()
        {
            PDef("k", 0.3, "Weighting factor");
            PDef("w", 40, "Local window size. Should always be positive");
            PDef("debug_binarize", 0, "output the result of binarization");
        }

        public override void Binarize(Bytearray bin_image, Bytearray gray_image)
        {
            w = PGeti("w");
            k = (float)PGetf("k");
            whalf = w >> 1;
            // fprintf(stderr,"[sauvola %g %d]\n",k,w);
            if(k<0.001 || k>0.999)
                throw new Exception("Binarize: CHECK_ARG(k>=0.001 && k<=0.999)");
            if(w==0 || k>=1000)
                throw new Exception("Binarize: CHECK_ARG(w>0 && k<1000)");
            if(bin_image.Length1d() != gray_image.Length1d())
                bin_image.MakeLike(gray_image);

            if(NarrayUtil.contains_only(gray_image, (byte)0, (byte)255))
            {
                bin_image.Copy(gray_image);
                return;
            }

            int image_width  = gray_image.Dim(0);
            int image_height = gray_image.Dim(1);
            whalf = w >> 1;

            // Calculate the integral image, and integral of the squared image
            Narray<long> integral_image = new Narray<long>(), rowsum_image = new Narray<long>();
            Narray<long> integral_sqimg = new Narray<long>(), rowsum_sqimg = new Narray<long>();
            integral_image.MakeLike(gray_image);
            rowsum_image.MakeLike(gray_image);
            integral_sqimg.MakeLike(gray_image);
            rowsum_sqimg.MakeLike(gray_image);
            int xmin,ymin,xmax,ymax;
            double diagsum,idiagsum,diff,sqdiagsum,sqidiagsum,sqdiff,area;
            double mean,std,threshold;

            for (int j = 0; j < image_height; j++)
            {
                rowsum_image[0, j] = gray_image[0, j];
                rowsum_sqimg[0, j] = gray_image[0, j] * gray_image[0, j];
            }
            for (int i = 1; i < image_width; i++)
            {
                for (int j = 0; j < image_height; j++)
                {
                    rowsum_image[i, j] = rowsum_image[i - 1, j] + gray_image[i, j];
                    rowsum_sqimg[i, j] = rowsum_sqimg[i - 1, j] + gray_image[i, j] * gray_image[i, j];
                }
            }

            for (int i = 0; i < image_width; i++)
            {
                integral_image[i, 0] = rowsum_image[i, 0];
                integral_sqimg[i, 0] = rowsum_sqimg[i, 0];
            }
            for (int i = 0; i < image_width; i++)
            {
                for (int j = 1; j < image_height; j++)
                {
                    integral_image[i, j] = integral_image[i, j - 1] + rowsum_image[i, j];
                    integral_sqimg[i, j] = integral_sqimg[i, j - 1] + rowsum_sqimg[i, j];
                }
            }

            //Calculate the mean and standard deviation using the integral image

            for(int i=0; i<image_width; i++){
                for(int j=0; j<image_height; j++){
                    xmin = Math.Max(0,i-whalf);
                    ymin = Math.Max(0, j - whalf);
                    xmax = Math.Min(image_width - 1, i + whalf);
                    ymax = Math.Min(image_height - 1, j + whalf);
                    area = (xmax-xmin+1)*(ymax-ymin+1);
                    // area can't be 0 here
                    // proof (assuming whalf >= 0):
                    // we'll prove that (xmax-xmin+1) > 0,
                    // (ymax-ymin+1) is analogous
                    // It's the same as to prove: xmax >= xmin
                    // image_width - 1 >= 0         since image_width > i >= 0
                    // i + whalf >= 0               since i >= 0, whalf >= 0
                    // i + whalf >= i - whalf       since whalf >= 0
                    // image_width - 1 >= i - whalf since image_width > i
                    // --IM
                    if (area <= 0)
                        throw new Exception("Binarize: area can't be 0 here");
                    if (xmin == 0 && ymin == 0)
                    { // Point at origin
                        diff = integral_image[xmax, ymax];
                        sqdiff = integral_sqimg[xmax, ymax];
                    }
                    else if (xmin == 0 && ymin > 0)
                    { // first column
                        diff = integral_image[xmax, ymax] - integral_image[xmax, ymin - 1];
                        sqdiff = integral_sqimg[xmax, ymax] - integral_sqimg[xmax, ymin - 1];
                    }
                    else if (xmin > 0 && ymin == 0)
                    { // first row
                        diff = integral_image[xmax, ymax] - integral_image[xmin - 1, ymax];
                        sqdiff = integral_sqimg[xmax, ymax] - integral_sqimg[xmin - 1, ymax];
                    }
                    else
                    { // rest of the image
                        diagsum = integral_image[xmax, ymax] + integral_image[xmin - 1, ymin - 1];
                        idiagsum = integral_image[xmax, ymin - 1] + integral_image[xmin - 1, ymax];
                        diff = diagsum - idiagsum;
                        sqdiagsum = integral_sqimg[xmax, ymax] + integral_sqimg[xmin - 1, ymin - 1];
                        sqidiagsum = integral_sqimg[xmax, ymin - 1] + integral_sqimg[xmin - 1, ymax];
                        sqdiff = sqdiagsum - sqidiagsum;
                    }

                    mean = diff/area;
                    std  = Math.Sqrt((sqdiff - diff*diff/area)/(area-1));
                    threshold = mean*(1+k*((std/128)-1));
                    if(gray_image[i,j] < threshold)
                        bin_image[i,j] = 0;
                    else
                        bin_image[i,j] = (byte)(MAXVAL-1);
                }
            }
            if(PGeti("debug_binarize") > 0) {
                ImgIo.write_image_gray("debug_binarize.png", bin_image);
            }
        }
    }
}
