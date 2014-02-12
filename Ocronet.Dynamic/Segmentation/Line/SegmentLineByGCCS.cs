using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ocronet.Dynamic.Interfaces;
using Ocronet.Dynamic.ImgLib;
using Ocronet.Dynamic.Utils;

namespace Ocronet.Dynamic.Segmentation.Line
{
    public class SegmentLineByGCCS : ISegmentLine
    {
        public SegmentLineByGCCS()
        {
        }

        public override string Name
        {
            get { return "seggccs"; }
        }

        public override string Description
        {
            get { return "connected component segmenter using grouping by overlap"; }
        }

        public override void Charseg(ref Intarray outimage, Bytearray inarray)
        {
            Bytearray image = new Bytearray();
            image.Copy(inarray);
            OcrRoutine.binarize_simple(image);
            OcrRoutine.Invert(image);
            outimage.Copy(image);
            Intarray labels = new Intarray();
            labels.Copy(image);
            ImgLabels.label_components(ref labels);
            Narray<Rect> boxes = new Narray<Rect>();
            ImgLabels.bounding_boxes(ref boxes, labels);
            Intarray equiv = new Intarray(boxes.Length());
            for(int i=0; i<boxes.Length(); i++)
                equiv[i] = i;
            for(int i=1; i<boxes.Length(); i++) {
                Rect p = boxes[i];
                for(int j=1;j<boxes.Length();j++) {
                    if(i==j) continue;
                    Rect q = boxes[j];
                    int x0 = Math.Max(p.x0, q.x0);
                    int x1 = Math.Min(p.x1, q.x1);
                    int iw = x1-x0;
                    if(iw <= 0) continue; // no overlap
                    int ow = Math.Min(p.Width(), q.Width());
                    float frac = iw/(float)(ow);
                    if(frac < 0.5f) continue; // insufficient overlap
                    // printf("%d %d : %d %d : %g\n",i,j,iw,ow,frac);
                    equiv.Put1d(Math.Max(i, j), Math.Min(i, j));
                }
            }
            for(int i=0; i<labels.Length(); i++)
                labels.Put1d(i, equiv.At1d(labels.At1d(i)));
            ImgLabels.renumber_labels(labels, 1);
            outimage.Move(labels);
            SegmRoutine.make_line_segmentation_white(outimage);
            SegmRoutine.check_line_segmentation(outimage);
        }
    }
}
