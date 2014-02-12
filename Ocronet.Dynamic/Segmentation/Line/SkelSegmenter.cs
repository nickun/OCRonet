using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ocronet.Dynamic.Interfaces;
using Ocronet.Dynamic.ImgLib;
using Ocronet.Dynamic.Utils;

namespace Ocronet.Dynamic.Segmentation.Line
{
    public class SkelSegmenter : ISegmentLine
    {
        public override string Name
        {
            get { return "skelseg"; }
        }

        public override string Description
        {
            get { return "skeleton segmenter"; }
        }

        public override void Charseg(ref Intarray segmentation, Bytearray image)
        {
            Bytearray timage = new Bytearray();
            timage.Copy(image);
            //for (int i = 0; i < image.Length(); i++) image[i] = (byte)(image[i] > 0 ? 0 : 1);
            OcrRoutine.binarize_simple(timage);
            OcrRoutine.Invert(image);
            Skeleton.Thin(ref timage);
            //ImgIo.write_image_gray("_thinned.png", timage);
            ImgMisc.remove_singular_points(ref timage, 2);
            //ImgIo.write_image_gray("_segmented.png", timage);
            Intarray tsegmentation = new Intarray();
            tsegmentation.Copy(timage);
            ImgLabels.label_components(ref tsegmentation);
            SegmRoutine.remove_small_components(tsegmentation, 4, 4);
            //ImgIo.write_image_packed("_labeled.png", tsegmentation);
            segmentation.Copy(image);
            ImgLabels.propagate_labels_to(ref segmentation, tsegmentation);
            //ImgIo.write_image_packed("_propagated.png", segmentation);
        }
    }
}
