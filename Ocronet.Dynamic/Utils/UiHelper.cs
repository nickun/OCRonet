using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Ocronet.Dynamic.Segmentation;
using Ocronet.Dynamic.ImgLib;

namespace Ocronet.Dynamic.Utils
{
    public static class UiHelper
    {
        public static Bitmap ConvertCharsegToBitmapRecolor(Intarray charseg, string trans = "")
        {
            Intarray cseg = new Intarray();
            cseg.Copy(charseg);
            Narray<Rect> bboxes = new Narray<Rect>();
            //SegmRoutine.make_line_segmentation_black(cseg);
            ImgLabels.bounding_boxes(ref bboxes, cseg);
            SegmRoutine.make_line_segmentation_white(cseg);
            ImgLabels.simple_recolor(cseg);
            return DrawSegmentTranscript(
                DrawSegmentNumbers(
                    ImgRoutine.NarrayToRgbBitmap(cseg),
                    bboxes),
                bboxes,
                trans);
        }

        public static Bitmap DrawSegmentNumbers(Bitmap bmp, Narray<Rect> bboxes)
        {
            int numAreaHeight = 10;
            Bitmap newbitmap = new Bitmap(bmp.Width, bmp.Height + numAreaHeight, bmp.PixelFormat);
            int height = bmp.Height;
            using (Graphics g = Graphics.FromImage(newbitmap))
            {
                SolidBrush bgrnBrush = new SolidBrush(Color.White);
                SolidBrush txtBrush = new SolidBrush(Color.Black);
                Pen rectPen = new Pen(Color.DarkGray);
                g.FillRectangle(bgrnBrush, new Rectangle(Point.Empty, newbitmap.Size));
                g.DrawImage(bmp, Point.Empty);
                bgrnBrush.Dispose();
                FontFamily fontFam;
                try { fontFam = new FontFamily("Tahoma"); }
                catch { fontFam = FontFamily.GenericSansSerif; }
                Font font = new Font(fontFam, 5f);
                for (int i = 1; i < bboxes.Length(); i++)
                {
                    Rect b = bboxes[i];
                    int bposY = (height - b.y1);
                    // draw bounding rects
                    g.DrawRectangle(rectPen, Math.Max(0, b.x0-1), Math.Max(0, bposY-1), b.W, b.H+1);
                    // draw numbers
                    g.DrawString(i.ToString(), font, txtBrush, b.x0, height + 1);
                }
                txtBrush.Dispose();
            }
            return newbitmap;
        }

        public static Bitmap DrawSegmentTranscript(Bitmap bmp, Narray<Rect> bboxes, string trans)
        {
            if (String.IsNullOrEmpty(trans) || bboxes.Length() - 1 != trans.Length)
                return bmp;

            int numAreaHeight = 15;
            Bitmap newbitmap = new Bitmap(bmp.Width, bmp.Height + numAreaHeight, bmp.PixelFormat);
            int height = bmp.Height;
            using (Graphics g = Graphics.FromImage(newbitmap))
            {
                SolidBrush bgrnBrush = new SolidBrush(Color.White);
                SolidBrush txtBrush = new SolidBrush(Color.Black);
                Pen rectPen = new Pen(Color.DarkGray);
                g.FillRectangle(bgrnBrush, new Rectangle(Point.Empty, newbitmap.Size));
                g.DrawImage(bmp, Point.Empty);
                bgrnBrush.Dispose();
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                FontFamily fontFam;
                try { fontFam = new FontFamily("Tahoma"); }
                catch { fontFam = FontFamily.GenericSansSerif; }
                
                Font font = new Font(fontFam, 6f);
                for (int i = 1; i < bboxes.Length(); i++)
                {
                    Rect b = bboxes[i];
                    g.DrawString(trans[i-1].ToString(), font, txtBrush, b.x0, height + 1);
                }
                txtBrush.Dispose();
            }
            return newbitmap;
        }
    }
}
