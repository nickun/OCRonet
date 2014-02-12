using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;

namespace Ocronet.Dynamic.ImgLib
{
    public static class ImgIo
    {
        /// <summary>
        /// Возвращает MemoryStream с содержимым файла
        /// </summary>
        public static Stream LoadStreamFromFile(string fileName)
        {
            MemoryStream stream = new MemoryStream();
            using (FileStream fstream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                CopyStream(fstream, stream);
                stream.Flush();
                stream.Position = 0L;
                fstream.Close();
            }
            return stream;
        }

        /// <summary>
        /// Загружает изображение из файла
        /// </summary>
        public static Bitmap LoadBitmapFromFile(string fileName)
        {
            Stream stream = LoadStreamFromFile(fileName);
            return (Bitmap)Image.FromStream(stream);
        }

        /// <summary>
        /// Копирует данные из одного стрима в другой
        /// </summary>
        public static long CopyStream(Stream source, Stream destination)
        {
            long count = source.Length;
            long num = count;
            byte[] buffer = new byte[0xf000];
            int length = (int)count;
            if (length > buffer.Length)
            {
                length = buffer.Length;
            }
            while (count != 0L)
            {
                int num3 = (int)count;
                if (count > length)
                {
                    num3 = length;
                }
                source.Read(buffer, 0, num3);
                destination.Write(buffer, 0, num3);
                count -= num3;
            }
            destination.Flush();
            return num;
        }

        public static Bitmap read_image_packed(Intarray image, string path)
        {
            Bitmap bitmap = LoadBitmapFromFile(path);
            image.Resize(bitmap.Width, bitmap.Height);
            ImgRoutine.NarrayFromBitmap(image, bitmap);
            return bitmap;
        }

        public static Bitmap read_image_rgb(Bytearray image, string path)
        {
            Bitmap bitmap = LoadBitmapFromFile(path);
            image.Resize(bitmap.Width, bitmap.Height, 3);
            ImgRoutine.NarrayFromBitmap(image, bitmap);
            return bitmap;
        }

        public static Bitmap read_image_gray(Bytearray image, string path)
        {
            Bitmap bitmap = LoadBitmapFromFile(path);
            image.Resize(bitmap.Width, bitmap.Height);
            ImgRoutine.NarrayFromBitmap(image, bitmap);
            return bitmap;
        }

        public static Bitmap read_image_binary(Bytearray image, string path)
        {
            Bitmap bitmap = LoadBitmapFromFile(path);
            image.Resize(bitmap.Width, bitmap.Height);
            ImgRoutine.NarrayFromBitmap(image, bitmap);
            double threshold = (NarrayUtil.Min(image) + NarrayUtil.Max(image)) / 2.0;
            for (int i = 0; i < image.Length1d(); i++)
                image.Put1d(i, (byte)((image.At1d(i) < threshold) ? 0 : 255));
            return bitmap;
        }


        public static void write_image_packed(string path, Intarray image)
        {
            Bitmap bitmap = ImgRoutine.NarrayToRgbBitmap(image);
            bitmap.Save(path);
            bitmap.Dispose();
        }

        public static void write_image_packed(Stream stream, Intarray image, System.Drawing.Imaging.ImageFormat fmt)
        {
            Bitmap bitmap = ImgRoutine.NarrayToRgbBitmap(image);
            bitmap.Save(stream, fmt);
        }

        public static void write_image_rgb(string path, Bytearray image)
        {
            Bitmap bitmap = ImgRoutine.NarrayToRgbBitmap(image);
            bitmap.Save(path);
            bitmap.Dispose();
        }

        public static void write_image_gray(string path, Bytearray image)
        {
            Bitmap bitmap = ImgRoutine.NarrayToRgbBitmap(image);
            bitmap.Save(path);
            bitmap.Dispose();
        }

        public static void write_image_gray(Stream stream, Bytearray image, System.Drawing.Imaging.ImageFormat fmt)
        {
            Bitmap bitmap = ImgRoutine.NarrayToRgbBitmap(image);
            bitmap.Save(stream, fmt);
        }

        public static void write_image_gray(string path, Floatarray image)
        {
            Bitmap bitmap = ImgRoutine.NarrayToRgbBitmap(image);
            bitmap.Save(path);
            bitmap.Dispose();
        }
    }
}
