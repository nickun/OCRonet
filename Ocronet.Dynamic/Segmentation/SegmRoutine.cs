using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Ocronet.Dynamic.ImgLib;
using Ocronet.Dynamic.Utils;

namespace Ocronet.Dynamic.Segmentation
{
    public class SegmRoutine
    {
        public static int cseg_pixel(int chr)
        {
            if (!(chr > 0 && chr < 4096))
                throw new Exception("ASSERT: chr > 0 && chr < 4096");
            return (1 << 12) | chr;
        }

        public static void combine_segmentations(ref Intarray dst, Intarray src)
        {
            dst.SameDims(src);
            int n = NarrayUtil.Max(dst) + 1;
            for(int i = 0; i < dst.Length1d(); i++)
                dst.Put1d(i, (dst.At1d(i) + src.At1d(i) * n));
            ImgLabels.renumber_labels(dst, 1);
        }

        public static void fix_diacritics(Intarray segmentation)
        {
            Narray<Rect> bboxes = new Narray<Rect>();
            ImgLabels.bounding_boxes(ref bboxes, segmentation);
            if (bboxes.Length() < 1) return;
            Intarray assignments = new Intarray(bboxes.Length());
            for (int i = 0; i < assignments.Length(); i++)
                assignments[i] = i;
            for (int j = 0; j < bboxes.Length(); j++)
            {
                float dist = 1e38f;
                int closest = -1;
                for (int i = 0; i < bboxes.Length(); i++)
                {
                    // j should overlap i in the x direction
                    if (bboxes.At1d(j).x1 < bboxes.At1d(i).x0) continue;
                    if (bboxes.At1d(j).x0 > bboxes.At1d(i).x1) continue;
                    // j should be above i
                    if (!(bboxes.At1d(j).y0 >= bboxes.At1d(i).y1)) continue;
#if false
                // j should be smaller than i
                if(!(bboxes.At1d(j).area()<bboxes.At1d(i).area())) continue;
#endif
                    float d = Math.Abs((bboxes[j].x0 + bboxes[j].x1) / 2 - (bboxes[i].x0 + bboxes[i].x1) / 2);
                    if (d >= dist) continue;
                    dist = d;
                    closest = i;
                }
                if (closest < 0) continue;
                assignments[j] = closest;
            }
            for (int i = 0; i < segmentation.Length(); i++)
                segmentation.Put1d(i, assignments[segmentation.At1d(i)]);
            ImgLabels.renumber_labels(segmentation, 1);
        }

        public static void local_min(ref Floatarray result, Floatarray data, int r)
        {
            int n = data.Length();
            result.Resize(n);
            for(int i=0; i<n; i++) {
                float lmin = data[i];
                for(int j=-r; j<=r; j++) {
                    int k = i+j;
                    unchecked
                    {
                        if ((uint)(k) >= (uint)(n)) continue;
                    }
                    if(data[k] >= lmin) continue;
                    lmin = data[k];
                }
                result[i] = lmin;
            }
        }

        public static void local_minima(ref Intarray result, Floatarray data, int r, float threshold)
        {
            int n = data.Length();
            result.Clear();
            Floatarray lmin = new Floatarray();
            local_min(ref lmin, data, r);
            for(int i=1; i<n-1; i++) {
                if(data[i] <= threshold && data[i] <= lmin[i] &&
                   data[i]<=data[i-1] && data[i]<data[i+1]) {
                    result.Push(i);
                }
            }
        }

        public static void line_segmentation_sort_x(Intarray segmentation)
        {
            if(NarrayUtil.Max(segmentation) > 100000)
                throw new Exception("line_segmentation_merge_small_components: to many segments");
            Narray<Rect> bboxes = new Narray<Rect>();
            ImgLabels.bounding_boxes(ref bboxes, segmentation);
            Floatarray x0s = new Floatarray();
            unchecked
            {
                x0s.Push((float)-999999);
            }
            for (int i = 1; i < bboxes.Length(); i++)
            {
                if (bboxes[i].Empty())
                {
                    x0s.Push(999999);
                }
                else
                {
                    x0s.Push(bboxes[i].x0);
                }
            }
            // dprint(x0s,1000); printf("\n");
            Narray<int> permutation = new Intarray();
            Narray<int> rpermutation = new Intarray();
            NarrayUtil.Quicksort(permutation, x0s);
            rpermutation.Resize(permutation.Length());
            for (int i = 0; i < permutation.Length(); i++)
                rpermutation[permutation[i]] = i;
            // dprint(rpermutation,1000); printf("\n");
            for (int i = 0; i < segmentation.Length1d(); i++)
            {
                if (segmentation.At1d(i) == 0) continue;
                segmentation.Put1d(i, rpermutation[segmentation.At1d(i)]);
            }
        }

        /// <summary>
        /// A valid line segmentation may contain 0 or 0xffffff as the
        /// background, and otherwise numbers components starting at 1.
        /// The segmentation consists of segmented background pixels
        /// (0x80xxxx) and segmented foreground pixels (0x00xxxx).  The
        /// segmented foreground pixels should constitute a usable
        /// binarization of the original image.
        /// </summary>
        public static void check_line_segmentation(Intarray cseg)
        {
            if (cseg.Length1d() == 0) return;
            if (cseg.Rank() != 2)
                throw new Exception("check_line_segmentation: rank must be 2");
            for (int i = 0; i < cseg.Length1d(); i++)
            {
                int value = cseg.At1d(i);
                if (value == 0) continue;
                if (value == 0xffffff) continue;
                if ((value & 0x800000) > 0)
                {
                    if((value & ~0x800000) > 100000)
                        throw new Exception("check_line_segmentation: (value & ~0x800000) > 100000");
                }
                else
                    if(value > 100000)
                        throw new Exception("check_line_segmentation: value > 100000");
            }
        }

        public static void make_line_segmentation_black(Intarray a)
        {
            check_line_segmentation(a);
            ImgMisc.replace_values(a, 0xFFFFFF, 0);
            for (int i = 0; i < a.Length1d(); i++)
                a.Put1d(i, a.At1d(i) & 0xFFF);
        }

        public static void make_line_segmentation_white(Intarray a)
        {
            ImgMisc.replace_values(a, 0, 0xFFFFFF);
            check_line_segmentation(a);
        }

        public static void check_page_segmentation(Intarray pseg)
        {
            bool allow_zero = true;
            Narray<bool> used = new Narray<bool>(5000);
            used.Fill(false);
            int nused = 0;
            int mused = 0;
            for (int i = 0; i < pseg.Length1d(); i++)
            {
                uint pixel = (uint)pseg.At1d(i);
                if (!(allow_zero || pixel != 0))
                    throw new Exception("CHECK_ARG: (allow_zero || pixel != 0)");
                if (pixel == 0 || pixel == 0xffffff) continue;
                int column = (int)(0xff & (pixel >> 16));
                int paragraph = (int)(0xff & (pixel >> 8));
                int line = (int)(0xff & pixel);
                if(!((column > 0 && column < 32) || column == 254 || column == 255))
                    throw new Exception("CHECK_ARG: ((column > 0 && column < 32) || column == 254 || column == 255)");
                if(!((paragraph >= 0 && paragraph < 64) || (paragraph >= 250 && paragraph <= 255)))
                    throw new Exception("CHECK_ARG: ((paragraph >= 0 && paragraph < 64) || (paragraph >= 250 && paragraph <= 255))");
                if (column < 32)
                {
                    if (!used[line])
                        nused++;
                    used[line] = true;
                    if (line > mused)
                        mused = line;
                }
            }
            // character segments need to be numbered sequentially (no gaps)
            // (gaps usually happen when someone passes a binary image instead of a segmentation)
            if (!(nused == mused || nused == mused + 1))
                Global.Debugf("warn", "check_page_segmentation found non-sequentially numbered segments");
        }

        public static void make_page_segmentation_black(Intarray a)
        {
            check_page_segmentation(a);
            ImgMisc.replace_values(a, 0xFFFFFF, 0);
        }

        public static void make_page_segmentation_white(Intarray a)
        {
            ImgMisc.replace_values(a, 0, 0xFFFFFF);
            check_page_segmentation(a);
        }

        public static void remove_small_components(Intarray segmentation, int r = 5)
        {
            remove_small_components(segmentation, r, r);
        }

        public static void remove_small_components(Intarray segmentation, int w = 5, int h = 4)
        {
            if (NarrayUtil.Max(segmentation) > 100000)
                throw new Exception("remove_small_components: to many segments");
            Narray<Rect> bboxes = new Narray<Rect>();
            ImgLabels.bounding_boxes(ref bboxes, segmentation);
            for (int i = 1; i < bboxes.Length(); i++)
            {
                Rect b = bboxes[i];
                if (b.Width() < w && b.Height() < h)
                {
                    for (int x = b.x0; x < b.x1; x++)
                        for (int y = b.y0; y < b.y1; y++)
                            if (segmentation[x, y] == i)
                                segmentation[x, y] = 0;
                }
            }
        }

        public static void remove_small_components<T>(Narray<T> bimage, int mw, int mh)
        {
            Intarray image = new Intarray();
            image.Copy(bimage);
            ImgLabels.label_components(ref image);
            Narray<Rect> rects = new Narray<Rect>();
            ImgLabels.bounding_boxes(ref rects, image);
            Bytearray good = new Bytearray(rects.Length());
            for(int i=0; i<good.Length(); i++)
                good[i] = 1;
            for (int i = 0; i < rects.Length(); i++)
            {
                if (rects[i].Width() < mw && rects[i].Height() < mh)
                {
                    // printf("*** %d %d %d\n",i,rects[i].width(),rects[i].height());
                    good[i] = 0;
                }
            }
            for (int i = 0; i < image.Length1d(); i++)
            {
                if (good[image.At1d(i)] == 0)
                    image.Put1d(i, 0);
            }
            for (int i = 0; i < image.Length1d(); i++)
                if (image.At1d(i) == 0) bimage.Put1d(i, default(T)); // default(T) - 0
        }

        public static void erase_small_components(Floatarray input, float mins=0.2f, float thresh=0.25f)
        {
            // compute a thresholded image for component labeling
            float threshold = thresh * NarrayUtil.Max(input);
            Intarray components = new Intarray();
            components.MakeLike(input);
            components.Fill(0);
            for (int i = 0; i < components.Length(); i++)
                components[i] = (input[i] > threshold ? 1 : 0);

            // compute the number of pixels in each component
            int n = ImgLabels.label_components(ref components);
            Intarray totals = new Intarray(n + 1);
            totals.Fill(0);
            for (int i = 0; i < components.Length(); i++)
                totals[components[i]] = totals[components[i]] + 1;
            totals[0] = 0;
            int biggest = NarrayUtil.ArgMax(totals);

            // erase small components
            float minsize = mins * totals[biggest];
            Bytearray keep = new Bytearray(n+1);
            float background = NarrayUtil.Min(input);
            for (int i = 0; i < keep.Length(); i++)
                keep[i] = (byte)(totals[i] > minsize ? 1 : 0);
            for (int i = 0; i < input.Length(); i++)
                if (keep[components[i]] == 0)
                    input[i] = background;
        }

        public static void extract_holes(ref Bytearray holes, Bytearray binarized)
        {
            Intarray temp = new Intarray();
            temp.Copy(binarized);
            NarrayUtil.Sub(255, temp);
            ImgLabels.label_components(ref temp);
            int background = -1;
            for (int i = 0; i < temp.Dim(0); i++)
            {
                if (temp[i, 0] != 0)
                {
                    background = temp[i, 0];
                    break;
                }
            }
            holes.MakeLike(temp);
            holes.Fill((byte)0);
            if(background <= 0)
                throw new Exception("extract_holes: background must be more 0");
            for (int i = 0; i < temp.Dim(0); i++)
            {
                for (int j = 0; j < temp.Dim(1); j++)
                {
                    if (temp[i, j] > 0 && temp[i, j] != background)
                        holes[i, j] = 255;
                }
            }
            /*fprintf(stderr, "segholes\n");
            dsection("segholes");
            dshow(holes, "y");*/
        }


        public static void line_segmentation_merge_small_components(ref Intarray segmentation, int r = 10)
        {
            if (NarrayUtil.Max(segmentation) > 100000)
                throw new Exception("line_segmentation_merge_small_components: to many segments");
            make_line_segmentation_black(segmentation);
            Narray<Rect> bboxes = new Narray<Rect>();
            ImgLabels.bounding_boxes(ref bboxes, segmentation);
            bboxes[0] = Rect.CreateEmpty();
            bool changed;
            do {
                changed = false;
                for(int i=1; i<bboxes.Length(); i++)
                {
                    Rect b = bboxes[i];
                    if(b.Empty()) continue;
                    if(b.Width()>=r || b.Height()>=r) continue;
                    // merge small components only with touching components
                    int closest = 0;
                    Rect b1 = b.Grow(1);
                    b1.Intersect(new Rect(0, 0, segmentation.Dim(0), segmentation.Dim(1)));
                    for(int x=b1.x0; x<b1.x1; x++)
                    {
                        for(int y=b1.y0; y<b1.y1; y++)
                        {
                            int value = segmentation[x,y];
                            if(value==0) continue;
                            if(value==i) continue;
                            closest = value;
                            break;
                        }
                    }
                    if(closest==0) continue;
                    for (int x = b.x0; x < b.x1; x++)
                        for (int y = b.y0; y < b.y1; y++)
                            if (segmentation[x, y] == i)
                                segmentation[x, y] = closest;
                    bboxes[i] = Rect.CreateEmpty();
                    changed = true;
                }
            } while (changed);
        }


        public static void rseg_to_cseg(Intarray cseg, Intarray rseg, Intarray ids)
        {
            Intarray map = new Intarray(NarrayUtil.Max(rseg) + 1);
            map.Fill(0);
            int color = 0;
            for (int i = 0; i < ids.Length(); i++)
            {
                if (ids[i] == 0) continue;
                color++;
                int start = ids[i] >> 16;
                int end = ids[i] & 0xFFFF;
                if (start > end)
                    throw new Exception("segmentation encoded in IDs looks seriously broken!");
                if (start >= map.Length() || end >= map.Length())
                    throw new Exception("segmentation encoded in IDs doesn't fit!");
                for (int j = start; j <= end; j++)
                    map[j] = color;
            }
            cseg.MakeLike(rseg);
            for (int i = 0; i < cseg.Length1d(); i++)
                cseg.Put1d(i, map[rseg.At1d(i)]);
        }

        /// <summary>
        /// Merge segments from start to end.
        /// </summary>
        /// <param name="cseg">Output</param>
        /// <param name="rseg">Input</param>
        /// <param name="start">start merge position</param>
        /// <param name="end">end merge position</param>
        public static void rseg_to_cseg(Intarray cseg, Intarray rseg, int start, int end)
        {
            int maxSegNum = NarrayUtil.Max(rseg);
            if (start > end)
                throw new Exception("segmentation encoded in IDs looks seriously broken!");
            if (start > maxSegNum || end > maxSegNum)
                throw new Exception("segmentation encoded in IDs doesn't fit!");
            Intarray map = new Intarray(maxSegNum + 1);
            map.Fill(0);

            int color = 1;
            for (int i = 1; i <= maxSegNum; i++)
            {
                map[i] = color;
                if (i < start || i >= end)
                    color++;
            }
            cseg.MakeLike(rseg);
            for (int i = 0; i < cseg.Length1d(); i++)
                cseg.Put1d(i, map[rseg.At1d(i)]);
        }

        /// <summary>
        /// Remove segments from start to end.
        /// </summary>
        /// <param name="cseg">Output</param>
        /// <param name="rseg">Input</param>
        /// <param name="start">start remove position</param>
        /// <param name="end">end remove position</param>
        public static void rseg_to_cseg_remove(Intarray cseg, Intarray rseg,
            Bytearray outimg, Bytearray img, int start, int end)
        {
            int maxSegNum = NarrayUtil.Max(rseg);
            if (start > end)
                throw new Exception("segmentation encoded in IDs looks seriously broken!");
            if (start > maxSegNum || end > maxSegNum)
                throw new Exception("segmentation encoded in IDs doesn't fit!");
            if (rseg.Length1d() != img.Length1d())
                throw new Exception("rseg and img must have same a dimension!");
            Intarray map = new Intarray(maxSegNum + 1);
            map.Fill(0);

            int color = 1;
            for (int i = 1; i <= maxSegNum; i++)
            {
                map[i] = color;
                if (i < start || i > end)
                    color++;
                else
                    map[i] = 0;
            }
            cseg.MakeLike(rseg);
            outimg.Copy(img);
            for (int i = 0; i < cseg.Length1d(); i++)
            {
                int val = rseg.At1d(i);
                cseg.Put1d(i, map[val]);
                if (val > 0 && map[val] == 0)
                    outimg.Put1d(i, 255);
            }
        }

    }
}
