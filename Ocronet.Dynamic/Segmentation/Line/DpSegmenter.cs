using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ocronet.Dynamic.Interfaces;
using System.Drawing;
using Ocronet.Dynamic.Utils;
using Ocronet.Dynamic.ImgLib;

namespace Ocronet.Dynamic.Segmentation.Line
{
    public class DpSegmenter : IDpSegmenter
    {
        // input
        Floatarray wimage;
        int where;

        // output
        Intarray costs;
        Intarray sources;
        int direction;
        int limit;

        public Intarray bestcuts;

        public string debug;

        public Narray<Narray<Point>> cuts;
        Floatarray cutcosts;

        public DpSegmenter()
        {
            wimage = new Floatarray();
            costs = new Intarray();
            sources = new Intarray();
            bestcuts = new Intarray();
            dimage = new Intarray();
            cuts = new Narray<Narray<Point>>();
            cutcosts = new Floatarray();

            PDef("down_cost", 0, "cost of down step");
            PDef("outside_diagonal_cost", 1, "cost of outside diagonal step to the left");
            PDef("outside_diagonal_cost_r", 1, "cost of outside diagonal step to the right");
            PDef("inside_diagonal_cost", 4, "cost of inside diagonal step");
            PDef("outside_weight", 0, "cost of outside pixel");
            PDef("inside_weight", 4, "cost of inside pixel");
            PDef("cost_smooth", 2.0, "smoothing parameter for costs");
            PDef("min_range", 1, "min range value");
            PDef("min_thresh", 80.0, "min threshold value");
            PDef("component_segmentation", 0, "also perform connected component segmentation");
            PDef("fix_diacritics", 1, "group dots above characters back with those characters");
            PDef("fill_holes", 1, "fill holes prior to dp segmentation (for cases like oo)");
            PDef("debug", "none", "debug output file");
        }

        public override string Name
        {
            get { return "dpseg"; }
        }

        public override string Description
        {
            get { return "curved cut segmenter"; }
        }

        protected void setParams()
        {
            down_cost = PGetf("down_cost");
            outside_weight = PGetf("outside_weight");
            inside_weight = PGetf("inside_weight");
            outside_diagonal_cost = PGetf("outside_diagonal_cost");
            outside_diagonal_cost_r = PGetf("outside_diagonal_cost_r");
            inside_diagonal_cost = PGetf("inside_diagonal_cost");
            cost_smooth = PGetf("cost_smooth");
            min_range = PGeti("min_range");
            min_thresh = PGetf("min_thresh");
            if (PGet("debug") != "none") debug = PGet("debug");
        }

        protected void Step(int x0, int x1, int y)
        {
            int w = wimage.Dim(0), h = wimage.Dim(1);
            Queue<Point> queue = new Queue<Point>(w * h);
            for (int i = x0; i < x1; i++) queue.Enqueue(new Point(i, y));
            int low = 1;
            int high = wimage.Dim(0) - 1;

            while (queue.Count > 0)
            {
                Point p = queue.Dequeue();
                int i = p.X, j = p.Y;
                int cost = costs[i, j];
                int ncost = (int)(cost + wimage[i, j] + down_cost);
                if (costs[i, j + direction] > ncost)
                {
                    costs[i, j + direction] = ncost;
                    sources[i, j + direction] = i;
                    if (j + direction != limit) queue.Enqueue(new Point(i, j + direction));
                }
                if (i > low)
                {
                    if (wimage[i, j] == 0)
                        ncost = (int)(cost + wimage[i, j] + outside_diagonal_cost);
                    else //if(wimage[i, j] > 0)
                        ncost = (int)(cost + wimage[i, j] + inside_diagonal_cost);
                    //else if(wimage[i, j] < 0)
                    //    ncost = cost + wimage[i,j] + boundary_diagonal_cost;*/
                    if (costs[i - 1, j + direction] > ncost)
                    {
                        costs[i - 1, j + direction] = ncost;
                        sources[i - 1, j + direction] = i;
                        if (j + direction != limit) queue.Enqueue(new Point(i - 1, j + direction));
                    }
                }
                if (i < high)
                {
                    if (wimage[i, j] == 0)
                        ncost = (int)(cost + wimage[i, j] + outside_diagonal_cost_r);
                    else //if(wimage[i, j] > 0)
                        ncost = (int)(cost + wimage[i, j] + inside_diagonal_cost);
                    //else if(wimage[i, j] < 0)
                    //    ncost = cost + wimage[i, j] + boundary_diagonal_cost;
                    if (costs[i + 1, j + direction] > ncost)
                    {
                        costs[i + 1, j + direction] = ncost;
                        sources[i + 1, j + direction] = i;
                        if (j + direction != limit) queue.Enqueue(new Point(i + 1, j + direction));
                    }
                }
            }
        }

        public void FindAllCuts()
        {
            int w = wimage.Dim(0), h = wimage.Dim(1);
            // initialize dimensions of cuts, costs etc
            cuts.Resize(w);
            cutcosts.Resize(w);
            costs.Resize(w, h);
            sources.Resize(w, h);

            costs.Fill(1000000000);
            for (int i = 0; i < w; i++) costs[i, 0] = 0;
            sources.Fill(-1);
            limit = where;
            direction = 1;
            Step(0, w, 0);

            for (int x = 0; x < w; x++)
            {
                cutcosts[x] = costs[x, where];
                cuts[x] = new Narray<Point>();
                cuts[x].Clear();
                // bottom should probably be initialized with 2*where instead of
                // h, because where cannot be assumed to be h/2. In the most extreme
                // case, the cut could go through 2 pixels in each row
                Narray<Point> bottom = new Narray<Point>();
                int i = x, j = where;
                while (j >= 0)
                {
                    bottom.Push(new Point(i, j));
                    i = sources[i, j];
                    j--;
                }
                //cuts(x).resize(h);
                for (i = bottom.Length() - 1; i >= 0; i--) cuts[x].Push(bottom[i]);
            }

            costs.Fill(1000000000);
            for (int i = 0; i < w; i++) costs[i, h - 1] = 0;
            sources.Fill(-1);
            limit = where;
            direction = -1;
            Step(0, w, h - 1);

            for (int x = 0; x < w; x++)
            {
                cutcosts[x] += costs[x, where];
                // top should probably be initialized with 2*(h-where) instead of
                // h, because where cannot be assumed to be h/2. In the most extreme
                // case, the cut could go through 2 pixels in each row
                Narray<Point> top = new Narray<Point>();
                int i = x, j = where;
                while (j < h)
                {
                    if (j > where) top.Push(new Point(i, j));
                    i = sources[i, j];
                    j++;
                }
                for (i = 0; i < top.Length(); i++) cuts[x].Push(top[i]);
            }

            // add costs for line "where"
            for (int x = 0; x < w; x++)
            {
                cutcosts[x] += wimage[x, where];
            }
        }

        public void FindBestCuts()
        {
            /*Intarray segm = new Intarray();
            segm.Copy(dimage);
            ImgLabels.simple_recolor(segm);
            ImgIo.write_image_packed("debug1.png", segm);*/
            unchecked
            {
                for (int i = 0; i < cutcosts.Length(); i++) NarrayUtil.ExtPut(dimage, i, (int)(cutcosts[i] + 10), 0xff0000);
                for (int i = 0; i < cutcosts.Length(); i++) NarrayUtil.ExtPut(dimage, i, (int)(min_thresh + 10), 0x800000);
            }
            Floatarray temp = new Floatarray();
            Gauss.Gauss1d(temp, cutcosts, cost_smooth);
            cutcosts.Move(temp);
            SegmRoutine.local_minima(ref bestcuts, cutcosts, min_range, min_thresh);
            for (int i = 0; i < bestcuts.Length(); i++)
            {
                Narray<Point> cut = cuts[bestcuts[i]];
                for (int j = 0; j < cut.Length(); j++)
                {
                    Point p = cut[j];
                    NarrayUtil.ExtPut(dimage, p.X, p.Y, 0x00ff00);
                }
            }
            /*segm.Copy(dimage);
            ImgLabels.simple_recolor(segm);
            ImgIo.write_image_packed("debug2.png", segm);*/
        }

        public void SetImage(Bytearray image_)
        {
            Bytearray image = new Bytearray();
            //image = image_;
            image.Copy(image_);
            dimage.Copy(image);
            if (PGeti("fill_holes") > 0)
            {
                Bytearray holes = new Bytearray();
                SegmRoutine.extract_holes(ref holes, image);
                for (int i = 0; i < image.Length(); i++)
                    if (holes.At1d(i) > 0) image.Put1d(i, 255);
            }
            int w = image.Dim(0), h = image.Dim(1);
            wimage.Resize(w, h);
            wimage.Fill(0);
            float s1 = 0.0f, sy = 0.0f;
            for (int i = 1; i < w; i++)
                for (int j = 0; j < h; j++)
                {
                    if (image[i, j] > 0) { s1++; sy += j; }
                    if (image[i, j] > 0) wimage[i, j] = inside_weight;
                    else wimage[i, j] = outside_weight;
                }
            if(s1==0) where = image.Dim(1)/2;
            else where = (int)(sy / s1);
            for (int i = 0; i < dimage.Dim(0); i++) dimage[i, where] = 0x008000;
        }


        public override void Charseg(ref Intarray segmentation, Bytearray inraw)
        {
            setParams();
            //Logger.Default.Image("segmenting", inraw);

            int PADDING = 3;
            OcrRoutine.optional_check_background_is_lighter(inraw);
            Bytearray image = new Bytearray();
            image.Copy(inraw);
            OcrRoutine.binarize_simple(image);
            OcrRoutine.Invert(image);

            SetImage(image);
            FindAllCuts();
            FindBestCuts();

            Intarray seg = new Intarray();
            seg.MakeLike(image);
            seg.Fill(255);

            for (int r = 0; r < bestcuts.Length(); r++)
            {
                int w = seg.Dim(0);
                int c = bestcuts[r];
                Narray<Point> cut = cuts[c];
                for (int y = 0; y < image.Dim(1); y++)
                {
                    for (int i = -1; i <= 1; i++)
                    {
                        int x = cut[y].X;
                        if (x < 1 || x >= w - 1) continue;
                        seg[x + i, y] = 0;
                    }
                }
            }
            ImgLabels.label_components(ref seg);
            // dshowr(seg,"YY"); dwait();
            segmentation.Copy(image);

            for (int i = 0; i < seg.Length1d(); i++)
                if (segmentation.At1d(i) == 0) seg.Put1d(i, 0);

            ImgLabels.propagate_labels_to(ref segmentation, seg);

            if (PGeti("component_segmentation") > 0)
            {
                Intarray ccseg = new Intarray();
                ccseg.Copy(image);
                ImgLabels.label_components(ref ccseg);
                SegmRoutine.combine_segmentations(ref segmentation, ccseg);
                if (PGeti("fix_diacritics") > 0)
                {
                    SegmRoutine.fix_diacritics(segmentation);
                }
            }
#if false
            SegmRoutine.line_segmentation_merge_small_components(ref segmentation, small_merge_threshold);
            SegmRoutine.line_segmentation_sort_x(segmentation);
#endif

            SegmRoutine.make_line_segmentation_white(segmentation);
            // set_line_number(segmentation, 1);
            //Logger.Default.Image("resulting segmentation", segmentation);
        }
    }
}
