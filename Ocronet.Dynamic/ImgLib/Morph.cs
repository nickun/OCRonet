using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ocronet.Dynamic.ImgLib
{
    public class Morph
    {
        public static byte bc(int c)
        {
            if (c < 0)
                return 0;
            if (c > 255)
                return 255;
            return Convert.ToByte(c);
        }

        public static void make_binary(Bytearray image)
        {
            for (int i = 0; i < image.Length1d(); i++)
                image.Put1d(i, (byte)(image.At1d(i) > 0 ? 255 : 0));
        }

        public static void check_binary(Bytearray image)
        {
            for (int i = 0; i < image.Length1d(); i++)
            {
                int value = image.At1d(i);
                if (!(value == 0 || value == 255))
                    throw new Exception("check_binary: value must be 0 or 255");
            }
        }

        public static void binary_invert(Bytearray image)
        {
            check_binary(image);
            for (int i = 0; i < image.Length1d(); i++)
                image.Put1d(i, (byte)(255 - image.At1d(i)));
        }

        public static void binary_autoinvert(Bytearray image)
        {
            check_binary(image);
            int count = 0;
            for (int i = 0; i < image.Length1d(); i++)
                if (image.At1d(i) > 0) count++;
            if (count > image.Length1d() / 2)
                binary_invert(image);
        }

        public static void binary_and(Bytearray image, Bytearray image2, int dx, int dy)
        {
            int w = image.Dim(0);
            int h = image.Dim(1);
            for (int i = 0; i < w; i++)
                for (int j = 0; j < h; j++)
                    image[i, j] = Math.Min(image[i, j], NarrayUtil.Ext(image2, i - dx, j - dy));
        }

        public static void binary_or(Bytearray image, Bytearray image2, int dx, int dy)
        {
            int w = image.Dim(0);
            int h = image.Dim(1);
            for (int i = 0; i < w; i++)
                for (int j = 0; j < h; j++)
                    image[i, j] = Math.Max(image[i, j], NarrayUtil.Ext(image2, i - dx, j - dy));
        }

        public static void binary_erode_circle(Bytearray image, int r)
        {
            if (r == 0)
                return;
            Bytearray outa = new Bytearray();
            outa.Copy(image);
            for (int i = -r; i <= r; i++)
                for (int j = -r; j <= r; j++)
                {
                    if (i * i + j * j <= r * r)
                        binary_and(outa, image, i, j);
                }
            image.Move(outa);
        }

        public static void binary_dilate_circle(Bytearray image, int r)
        {
            if (r == 0)
                return;
            Bytearray outa = new Bytearray();
            outa.Copy(image);
            for (int i = -r; i <= r; i++)
                for (int j = -r; j <= r; j++)
                {
                    if (i * i + j * j <= r * r)
                        binary_or(outa, image, i, j);
                }
            image.Move(outa);
        }

        public static void binary_open_circle(Bytearray image, int r)
        {
            if (r == 0)
                return;
            binary_erode_circle(image, r);
            binary_dilate_circle(image, r);
        }

        public static void binary_close_circle(Bytearray image, int r)
        {
            if (r == 0)
                return;
            binary_dilate_circle(image, r);
            binary_erode_circle(image, r);
        }

        public static void binary_erode_rect(Bytearray image, int rw, int rh)
        {
            if(rw==0 && rh==0)
                return;
            Bytearray outa = new Bytearray();;
            outa.Copy(image);
            for(int i=0; i<rw; i++)
                binary_and(outa, image, i-rw/2, 0);
            for(int j=0; j<rh; j++)
                binary_and(image, outa, 0, j-rh/2);
        }

        public static void binary_dilate_rect(Bytearray image, int rw, int rh)
        {
            if(rw==0 && rh==0)
                return;
            Bytearray outa = new Bytearray();
            outa.Copy(image);
            // note that we handle the even cases complementary
            // to erode_rect; this makes open_rect and close_rect
            // do the right thing
            for(int i=0; i<rw; i++)
                binary_or(outa, image, i-(rw-1)/2, 0);
            for(int j=0; j<rh; j++)
                binary_or(image, outa, 0, j-(rh-1)/2);
        }

        public static void binary_open_rect(Bytearray image, int rw, int rh)
        {
            if (rw == 0 && rh == 0)
                return;
            binary_erode_rect(image, rw, rh);
            binary_dilate_rect(image, rw, rh);
        }

        public static void binary_close_rect(Bytearray image, int rw, int rh)
        {
            if (rw == 0 && rh == 0)
                return;
            binary_dilate_rect(image, rw, rh);
            binary_erode_rect(image, rw, rh);
        }
    }
}
