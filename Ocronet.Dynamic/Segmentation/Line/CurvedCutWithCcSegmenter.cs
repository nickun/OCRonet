using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ocronet.Dynamic.Utils;
using Ocronet.Dynamic.ImgLib;

namespace Ocronet.Dynamic.Segmentation.Line
{
    /// <summary>
    /// old CurvedCutSegmenterToISegmentLineAdapterWithCc
    /// </summary>
    public class CurvedCutWithCcSegmenter : CurvedCutSegmenter
    {
        public override void Charseg(ref Intarray result_segmentation, Bytearray orig_image)
        {
            Bytearray image = new Bytearray();
            image.Copy(orig_image);
            OcrRoutine.optional_check_background_is_lighter(image);
            OcrRoutine.binarize_simple(image);
            OcrRoutine.Invert(image);

            Intarray ccseg = new Intarray();
            ccseg.Copy(image);
            ImgLabels.label_components(ref ccseg);

            base.Charseg(ref result_segmentation, orig_image);
            SegmRoutine.combine_segmentations(ref result_segmentation, ccseg);
        }
    }
}
