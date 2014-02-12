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
    /// old CurvedCutSegmenterToISegmentLineAdapter
    /// </summary>
    public class CurvedCutSegmenter : ISegmentLine
    {
        protected CurvedCutSegmenterImpl segmenter;
        protected int small_merge_threshold;

        public CurvedCutSegmenter()
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
                throw new Exception("CurvedCutSegmenterToISegmentLineAdapter:Set: unknown key");
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

        public override void Charseg(ref Intarray result_segmentation, Bytearray orig_image)
        {
            Logger.Default.Image("segmenting", orig_image);

            int PADDING = 3;
            OcrRoutine.optional_check_background_is_lighter(orig_image);
            Bytearray image = new Bytearray();
            Narray<byte> bimage = image;
            image.Copy(orig_image);
            OcrRoutine.binarize_simple(image);
            OcrRoutine.Invert(image);
            ImgOps.pad_by(ref bimage, PADDING, PADDING);
            // pass image to segmenter
            segmenter.SetImage(image);
            // find all cuts in the image
            segmenter.FindAllCuts();
            // choose the best of all cuts
            segmenter.FindBestCuts();

            Intarray segmentation = new Intarray();
            segmentation.Resize(image.Dim(0), image.Dim(1));
            for (int i = 0; i < image.Dim(0); i++)
                for (int j = 0; j < image.Dim(1); j++)
                    segmentation[i, j] = image[i, j] > 0 ? 1 : 0;

            for (int r = 0; r < segmenter.bestcuts.Length(); r++)
            {
                int c = segmenter.bestcuts[r];
                Narray<Point> cut = segmenter.cuts[c];
                for (int y = 0; y < image.Dim(1); y++)
                {
                    for (int x = cut[y].X; x < image.Dim(0); x++)
                    {
                        if (segmentation[x, y] > 0) segmentation[x, y]++;
                    }
                }
            }
            ImgOps.extract_subimage(result_segmentation, segmentation, PADDING, PADDING,
                             segmentation.Dim(0) - PADDING, segmentation.Dim(1) - PADDING);
            
            if (small_merge_threshold > 0)
            {
                SegmRoutine.line_segmentation_merge_small_components(ref result_segmentation, small_merge_threshold);
                SegmRoutine.line_segmentation_sort_x(result_segmentation);
            }

            SegmRoutine.make_line_segmentation_white(result_segmentation);
            // set_line_number(segmentation, 1);
            Logger.Default.Image("resulting segmentation", result_segmentation);
        }
    }
}
