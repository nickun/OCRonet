using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.IO;
using Ocronet.Dynamic;
using Ocronet.Dynamic.ImgLib;

namespace DynamicVizSegmenter
{
    public static class Utils
    {
        /// <summary>
        /// Convert System.Drawing.Bitmap to System.Windows.Media.ImageSource
        /// </summary>
        /// <param name="bitmap">System.Drawing.Bitmap object</param>
        /// <returns>System.Windows.Media.ImageSource object</returns>
        public static ImageSource ToImageSource(this Bitmap bitmap)
        {
            var bi = new BitmapImage();
            using (var ms = new MemoryStream())
            {
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                ms.Position = 0;

                bi.BeginInit();
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.UriSource = null;
                bi.StreamSource = ms;
                bi.EndInit();
            }
            return bi;
        }

        public static ImageSource ToImageSource(this Bytearray grayImg)
        {
            Bitmap bitmap = ImgRoutine.NarrayToRgbBitmap(grayImg);
            var bi = new BitmapImage();
            using (var ms = new MemoryStream())
            {
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                ms.Position = 0;

                bi.BeginInit();
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.UriSource = null;
                bi.StreamSource = ms;
                bi.EndInit();
            }
            return bi;
        }
    }
}
