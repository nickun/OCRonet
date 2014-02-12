using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace Ocronet.Dynamic.Recognizers.Lenet
{
    public class LenetRoutines
    {
        public static unsafe T[] ToMatrix<T>(Bitmap bm, bool normalize = false, bool yflip = false)
        {
            int r = 2, g = 1, b = 0;
            float cr = 0.5f, cg = 0.419f, cb = 0.081f;    // RMY convert method
            T[] result = new T[bm.Width * bm.Height];

            // lock source bitmap data
            BitmapData srcData = bm.LockBits(
                new Rectangle(0, 0, bm.Width, bm.Height),
                ImageLockMode.ReadOnly, bm.PixelFormat);
            int pixelSize = (srcData.PixelFormat == PixelFormat.Format8bppIndexed) ? 1 : 3;
            byte* src = (byte*)srcData.Scan0.ToPointer();
            int srcOffset = srcData.Stride - bm.Width * pixelSize;
            int width = bm.Width;
            int height = bm.Height;
            int yres;

            // --- GrayScale ---
            if (srcData.PixelFormat == PixelFormat.Format8bppIndexed)
            {
                // for each line
                for (int y = 0; y < height; y++)
                {
                    // for each pixel
                    for (int x = 0; x < width; x++, src++)
                    {
                        yres = y;
                        if (yflip)
                            yres = height - y - 1;
                        if (normalize)
                            result[yres * width + x] = (T)Convert.ChangeType((1.0 - *src / 255.0), typeof(T));
                        else
                            result[yres * width + x] = (T)Convert.ChangeType((255 - *src), typeof(T));
                    }
                    src += srcOffset;
                }
            }
            else //--- RGB ---
            {
                // for each line
                for (int y = 0; y < height; y++)
                {
                    // for each pixel
                    for (int x = 0; x < width; x++, src += pixelSize)
                    {
                        yres = y;
                        if (yflip)
                            yres = height - y - 1;
                        if (normalize)
                            result[yres * width + x] = (T)Convert.ChangeType((1.0 - Convert.ToByte(cr * src[r] + cg * src[g] + cb * src[b]) / 255.0), typeof(T));
                        else
                            result[yres * width + x] = (T)Convert.ChangeType((255 - Convert.ToByte(cr * src[r] + cg * src[g] + cb * src[b])), typeof(T));
                    }
                    src += srcOffset;
                }
            }

            // unlock source image
            bm.UnlockBits(srcData);

            return result;
        }
    }
}
