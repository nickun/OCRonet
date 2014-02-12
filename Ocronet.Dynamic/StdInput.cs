using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace Ocronet.Dynamic
{
    public class StdInput
    {
        private byte[] data;
        private int[] dims = new int[2];
        private int total;

        public StdInput(int height, int width)
        {
            alloc_(height, width);
        }

        public StdInput(byte[] buffer, int height, int width, bool invert = false)
        {
            alloc_(height, width);
            if (invert)
                for (int i = 0; i < Length; i++)
                    data[i] = (byte)(255 - buffer[i]);
            else
                for (int i = 0; i < Length; i++)
                    data[i] = buffer[i];
        }

        public StdInput(Narray<byte> bytearray)
        {
            alloc_(bytearray.Dim(1), bytearray.Dim(0));
            int yput;
            for (int y = 0; y < Height; y++)
            {
                yput = Height - y - 1;
                for(int x=0; x < Width; x++)
                    Put(yput, x, bytearray[x, y]);
            }
        }

        public StdInput(Narray<float> floatarray)
        {
            alloc_(floatarray.Dim(1), floatarray.Dim(0));
            int yput;
            for (int y = 0; y < Height; y++)
            {
                yput = Height - y - 1;
                for (int x = 0; x < Width; x++)
                    Put(yput, x, Convert.ToByte(floatarray[x, y] * 255));
            }
        }

        public void Put(int y, int x, byte value)
        {
            data[x + y * Width] = value;
        }

        public byte Get(int y, int x)
        {
            return data[x + y * Width];
        }

        /// <summary>
        /// Element subscripting.
        /// </summary>
        public byte this[int y, int x]
        {
            get { return Get(y, x); }
            set { Put(y, x, value); }
        }

        public int Height
        {
            get { return dims[0]; }
        }

        public int Width
        {
            get { return dims[1]; }
        }

        public int Length
        {
            get { return total; }
        }

        public byte Max
        {
            get
            {
                byte result = byte.MinValue;
                for (int i = 0; i < Length; i++)
                    if (data[i] > result)
                        result = data[i];
                return result;
            }
        }

        public byte Min
        {
            get
            {
                byte result = byte.MaxValue;
                for (int i = 0; i < Length; i++)
                    if (data[i] < result)
                        result = data[i];
                return result;
            }
        }

        public override string ToString()
        {
            return String.Format("byte[{0}] ({1}, {2})", Length, Height, Width);
        }

        public byte[] GetDataBuffer()
        {
            return data;
        }

        public Floatarray ToFloatarray()
        {
            Floatarray fa = new Floatarray();
            fa.Resize(Width, Height);
            int yput;
            for (int y = 0; y < Height; y++)
            {
                yput = Height - y - 1;
                for (int x = 0; x < Width; x++)
                    fa.Put(x, yput, Convert.ToSingle(Get(y, x) / 255.0f));
            }
            return fa;
        }

        public Bytearray ToBytearray()
        {
            Bytearray ba = new Bytearray();
            ba.Resize(Width, Height);
            int yput;
            for (int y = 0; y < Height; y++)
            {
                yput = Height - y - 1;
                for (int x = 0; x < Width; x++)
                    ba.Put(x, yput, Get(y, x));
            }
            return ba;
        }

        /// <summary>
        /// allocate the elements of the array
        /// </summary>
        private void alloc_(int h, int w)
        {
            total = h * w;
            data = new byte[total];
            dims[0] = h;
            dims[1] = w;
        }



        public static unsafe StdInput FromBitmap(Bitmap bm)
        {
            int r = 2, g = 1, b = 0;
            float cr = 0.5f, cg = 0.419f, cb = 0.081f;    // RMY convert method
            StdInput lenetInput = new StdInput(bm.Height, bm.Width);
            byte[] buffer = lenetInput.GetDataBuffer();

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
                        buffer[yres * width + x] = Convert.ToByte(255 - *src);
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
                        buffer[yres * width + x] = Convert.ToByte(255 - Convert.ToByte(cr * src[r] + cg * src[g] + cb * src[b]));
                    }
                    src += srcOffset;
                }
            }

            // unlock source image
            bm.UnlockBits(srcData);

            return lenetInput;
        }
    }
}
