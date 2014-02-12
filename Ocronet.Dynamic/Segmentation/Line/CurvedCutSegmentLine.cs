using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Ocronet.Dynamic.Interfaces;
using Ocronet.Dynamic.Utils;
using Ocronet.Dynamic.ImgLib;

namespace Ocronet.Dynamic.Segmentation.Line
{
    /// <summary>
    /// old CurvedCutSegmenter
    /// </summary>
    public class CurvedCutSegmentLine : ISegmentLine
    {
        protected CurvedCutSegmenterImpl segmenter;
        protected int small_merge_threshold;

        public CurvedCutSegmentLine()
        {
            small_merge_threshold = 1;
            segmenter = new CurvedCutSegmenterImpl();
        }

        public override string Name
        {
            get { return "curvedcut"; }
        }

        public override string Description
        {
            get { return "curved cut segmenter"; }
        }

        public override void Set(string key, string value)
        {
            Logger.Default.Format("set parameter {0} to {1}", key, value);
            if (key == "debug")
                segmenter.debug = value;
            else
                throw new Exception("CurvedCutSegmenter:Set: unknown key");
        }

        public override void Set(string key, int value)
        {
            Logger.Default.Format("set parameter {0} to {1}", key, value);
            if (key == "down_cost")
                segmenter.down_cost = value;
            else if (key == "small_merge_threshold")
                small_merge_threshold = value;
            else if (key == "outside_diagonal_cost")
                segmenter.outside_diagonal_cost = value;
            else if (key == "inside_diagonal_cost")
                segmenter.inside_diagonal_cost = value;
            else if (key == "boundary_diagonal_cost")
                segmenter.boundary_diagonal_cost = value;
            else if (key == "outside_weight")
                segmenter.outside_weight = value;
            else if (key == "boundary_weight")
                segmenter.boundary_weight = value;
            else if (key == "inside_weight")
                segmenter.inside_weight = value;
            else if (key == "min_range")
                segmenter.min_range = value;
            else
                throw new Exception("CurvedCutSegmenter:Set: unknown key");
        }

        public override void Set(string key, double value)
        {
            Logger.Default.Format("set parameter {0} to {1}", key, value);
            
            if (key == "min_thresh")
                segmenter.min_thresh = (float)value;
            else
                throw new Exception("CurvedCutSegmenter:Set: unknown key");
        }

        public override void Charseg(ref Intarray segmentation, Bytearray inraw)
        {
            Logger.Default.Image("segmenting", inraw);
            
            OcrRoutine.optional_check_background_is_lighter(inraw);
            Bytearray image = new Bytearray();
            image.Copy(inraw);
            OcrRoutine.binarize_simple(image);
            OcrRoutine.Invert(image);

            segmenter.SetImage(image);
            segmenter.FindAllCuts();
            segmenter.FindBestCuts();

            Intarray seg = new Intarray();
            seg.Copy(image);

            for (int r = 0; r < segmenter.bestcuts.Length(); r++)
            {
                int w = seg.Dim(0);
                int c = segmenter.bestcuts[r];
                Narray<Point> cut = segmenter.cuts[c];
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
            ImgLabels.propagate_labels_to(ref segmentation, seg);

            SegmRoutine.line_segmentation_merge_small_components(ref segmentation, small_merge_threshold);
            SegmRoutine.line_segmentation_sort_x(segmentation);

            SegmRoutine.make_line_segmentation_white(segmentation);
            // set_line_number(segmentation, 1);
            Logger.Default.Image("resulting segmentation", segmentation);
        }

    }
}
