using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace Ocronet.Dynamic.ImgLib
{
    public static class ImgRoutine
    {
        public static unsafe Bitmap NarrayToRgbBitmap<T>(Narray<T> image)
        {
            int w = image.Dim(0);
            int h = image.Dim(1);
            // create new image
            Bitmap bitmap = new Bitmap(w, h, PixelFormat.Format24bppRgb);
            // lock destination bitmap data
            BitmapData destinationData = bitmap.LockBits(
                new Rectangle(0, 0, w, h),
                ImageLockMode.WriteOnly, bitmap.PixelFormat);
            int pixelOffset = 3, r = 2, g = 1, b = 0;
            int dstOffset = destinationData.Stride - w * pixelOffset;
            byte* dst = (byte*)destinationData.Scan0.ToPointer();

            if (image.Rank() == 3)
            {
                // for each line
                for (int y = h - 1; y >= 0; y--)
                {
                    // for each pixel
                    for (int x = 0; x < w; x++, dst += pixelOffset)
                    {
                        dst[r] = Convert.ToByte(image[x, y, 0]);
                        dst[g] = Convert.ToByte(image[x, y, 1]);
                        dst[b] = Convert.ToByte(image[x, y, 2]);
                    }
                    dst += dstOffset;
                }
            }
            else if (image.Rank() == 2)
            {
                // packed image
                if (typeof(T).Name == "Int32")
                {
                    // for each line
                    for (int y = h - 1; y >= 0; y--)
                    {
                        // for each pixel
                        for (int x = 0; x < w; x++, dst += pixelOffset)
                        {
                            dst[r] = Convert.ToByte((Convert.ToInt32(image[x, y]) >> 16) & 0xFF);
                            dst[g] = Convert.ToByte((Convert.ToInt32(image[x, y]) >> 8) & 0xFF);
                            dst[b] = Convert.ToByte((Convert.ToInt32(image[x, y])) & 0xFF);
                        }
                        dst += dstOffset;
                    }
                }
                else // gray image
                {
                    // for each line
                    for (int y = h - 1; y >= 0; y--)
                    {
                        // for each pixel
                        for (int x = 0; x < w; x++, dst += pixelOffset)
                        {
                            dst[r] = Convert.ToByte(image[x, y]);
                            dst[g] = Convert.ToByte(image[x, y]);
                            dst[b] = Convert.ToByte(image[x, y]);
                        }
                        dst += dstOffset;
                    }
                }
            }

            // unlock destination image
            bitmap.UnlockBits(destinationData);
            return bitmap;
        }

        public static unsafe Bitmap GrayNarrayToBitmap(Narray<byte> image)
        {
            if (image.Rank() != 2)
            {
                throw new Exception("Narray must be rank 2");
            }

            int w = image.Dim(0);
            int h = image.Dim(1);
            // create new image
            Bitmap bitmap = new Bitmap(w, h, PixelFormat.Format8bppIndexed);
            // lock destination bitmap data
            BitmapData destinationData = bitmap.LockBits(
                new Rectangle(0, 0, w, h),
                ImageLockMode.WriteOnly, bitmap.PixelFormat);
            int pixelOffset = 1;
            int dstOffset = destinationData.Stride - w * pixelOffset;
            byte* dst = (byte*)destinationData.Scan0.ToPointer();

            // for each line
            for (int y = h - 1; y >= 0; y--)
            {
                // for each pixel
                for (int x = 0; x < w; x++, dst += pixelOffset)
                {
                    *dst = image[x, y];
                }
                dst += dstOffset;
            }

            // unlock destination image
            bitmap.UnlockBits(destinationData);
            return bitmap;
        }

        /// <summary>
        /// Convert Bitmap image to any Narray
        /// </summary>
        public static unsafe void NarrayFromBitmap<T>(Narray<T> outarray, Bitmap bitmap, bool maxrgb = false, bool yflip = true)
        {
            if (bitmap.PixelFormat == PixelFormat.Format1bppIndexed)
                bitmap = new Bitmap(bitmap);
            int w = bitmap.Width;
            int h = bitmap.Height;
            Bytearray imgRgb = new Bytearray();
            imgRgb.Resize(w, h, 3);

            // lock source image
            BitmapData imageData = bitmap.LockBits(
                new Rectangle(0, 0, w, h),
                ImageLockMode.ReadOnly, bitmap.PixelFormat);
            int srcOffset = imageData.Stride - w;
            int pixelOffset = 1, r = 0, g = 0, b = 0;
            float cr = 0.5f, cg = 0.419f, cb = 0.081f;    // RMY convert method
            Color[] palette = null;
            // check image format
            if (bitmap.PixelFormat == PixelFormat.Format32bppArgb || bitmap.PixelFormat == PixelFormat.Format32bppRgb)
            {
                pixelOffset = 4;
                srcOffset = imageData.Stride - w * pixelOffset;
                r = 2; g = 1; b = 0;
                // get palette
                palette = new Color[256];
                // init palette
                for (int i = 0; i < 256; i++)
                {
                    palette[i] = Color.FromArgb(i, i, i);
                }
            }
            else if (bitmap.PixelFormat != PixelFormat.Format8bppIndexed)
            {
                pixelOffset = 3;
                srcOffset = imageData.Stride - w * pixelOffset;
                r = 2; g = 1; b = 0;
                // get palette
                palette = new Color[256];
                // init palette
                for (int i = 0; i < 256; i++)
                {
                    palette[i] = Color.FromArgb(i, i, i);
                }
            }
            else
                palette = bitmap.Palette.Entries;

            // process convert the image to RGB Bytearray
            byte* src = (byte*) imageData.Scan0.ToPointer( );
            int yres;
            // for each line
            //for (int y = h - 1; y >= 0; y--)
            for (int y = 0; y < h; y++)
            {
                // for each pixel
                for (int x = 0; x < w; x++, src += pixelOffset)
                {
                    yres = y;
                    if (yflip)
                        yres = h - y - 1;
                    imgRgb[x, yres, 0] = palette[src[r]].R;
                    imgRgb[x, yres, 1] = palette[src[g]].G;
                    imgRgb[x, yres, 2] = palette[src[b]].B;
                }
                src += srcOffset;
            }
            // unlock image
            bitmap.UnlockBits(imageData);

            // copy to output array
            if (outarray.Rank() == 3)
                outarray.Copy(imgRgb);
            else if (outarray.Rank() == 2)
            {
                // resize output array
                outarray.Resize(w, h);

                // packed image
                if (typeof(T).Name == "Int32")
                {
                    for (int y = 0; y < h; y++)
                    {
                        // for each RGB pixel convert to Gray
                        for (int x = 0; x < w; x++)
                        {
                            outarray[x, y] = (T)Convert.ChangeType(
                                ((imgRgb[x, y, 0] << 16) | (imgRgb[x, y, 1] << 8) | (imgRgb[x, y, 2])),
                                typeof(T));
                        }
                    }
                }
                else // gray image
                {
                    for (int x = 0; x < w; x++)
                    {
                        // for each RGB pixel convert to Gray
                        for (int y = 0; y < h; y++)
                        {
                            if (maxrgb)
                            {
                                outarray[x, y] = (T)Convert.ChangeType(
                                    Convert.ToByte(Math.Max(Math.Max(imgRgb[x, y, 0], imgRgb[x, y, 1]), imgRgb[x, y, 2])),
                                    typeof(T));
                            }
                            else
                            {
                                outarray[x, y] = (T)Convert.ChangeType(
                                    Convert.ToByte(cr * imgRgb[x, y, 0] + cg * imgRgb[x, y, 1] + cb * imgRgb[x, y, 2]),
                                    typeof(T));
                            }
                        }
                    }
                }
            }
            else
                throw new ArgumentException("NarrayFromBitmap: array rank must be 2 or 3");

        }


    }
}
