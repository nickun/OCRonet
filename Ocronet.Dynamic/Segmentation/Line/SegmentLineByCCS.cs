using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ocronet.Dynamic.Interfaces;
using Ocronet.Dynamic.ImgLib;
using Ocronet.Dynamic.Utils;

namespace Ocronet.Dynamic.Segmentation.Line
{
    /// <summary>
    /// Purpose: attempt to segment a line (or page) of text into characters by
    /// labeling the connected components
    /// </summary>
    public class SegmentLineByCCS : ISegmentLine
    {
        public SegmentLineByCCS()
        {
            PDef("swidth", 0, "smearing width");
            PDef("sheight", 10, "smearing height");
        }

        public override string Name
        {
            get { return "segccs"; }
        }

        public override string Description
        {
            get { return "connected component segmenter using morphology for grouping"; }
        }

        public override void Charseg(ref Intarray outimage, Bytearray inimage)
        {
            int swidth = PGeti("swidth");
            int sheight = PGeti("sheight");
            Bytearray image = new Bytearray();
            image.Copy(inimage);
            OcrRoutine.binarize_simple(image);
            OcrRoutine.Invert(image);
            outimage.Copy(image);
            if (swidth > 0 || sheight > 0)
                Morph.binary_close_rect(image, swidth, sheight);
            Intarray labels = new Intarray();
            labels.Copy(image);
            ImgLabels.label_components(ref labels);
            for(int i=0; i<outimage.Length1d(); i++)
                if (outimage.At1d(i) > 0)
                    outimage.Put1d(i, SegmRoutine.cseg_pixel(labels.At1d(i)));
            SegmRoutine.make_line_segmentation_white(outimage);
            SegmRoutine.check_line_segmentation(outimage);
        }
    }
}
