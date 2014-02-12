using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;

namespace Ocronet.DynamicConsole
{
    public class RoutineTools
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
    }
}
