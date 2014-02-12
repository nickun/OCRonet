using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Ocronet.Dynamic.ImgLib;

namespace Ocronet.Dynamic.Grouper
{
    public class GrouperRoutine
    {
        public static void check_approximately_sorted(Intarray labels)
        {
            for (int i = 0; i < labels.Length1d(); i++)
                if (labels.At1d(i) > 100000)
                    throw new Exception("labels out of range");
            Narray<Rect> rboxes = new Narray<Rect>();
            ImgLabels.bounding_boxes(ref rboxes, labels);
#if false
            // TODO/tmb disabling check until the overseg issue is fixed --tmb
            for(int i=1;i<rboxes.Length();i++) {
                if(rboxes[i].Right<rboxes[i-1].Left) {
                    /*errors_log("bad segmentation", labels);
                    errors_log.recolor("bad segmentation (recolored)", labels);
                    throw_fmt("boxes aren't approximately sorted: "
                              "box %d is to the left from box %d", i, i-1);*/
                }
            }
#endif
        }

        public static bool Equals(Intarray a, Intarray b)
        {
            if (a.Length() != b.Length()) return false;
            for (int i = 0; i < a.Length(); i++)
                if (a.UnsafeAt1d(i) != b.UnsafeAt1d(i)) return false;
            return true;
        }

        public static void segmentation_correspondences(Narray<Intarray> outsegments, Intarray seg, Intarray cseg)
        {
            if (NarrayUtil.Max(seg) >= 10000)
                throw new Exception("CHECK_ARG: (max(seg)<10000)");
            if (NarrayUtil.Max(cseg) >= 10000)
                throw new Exception("CHECK_ARG: (max(cseg)<10000)");

            int nseg = NarrayUtil.Max(seg) + 1;
            int ncseg = NarrayUtil.Max(cseg) + 1;
            Intarray overlaps = new Intarray(nseg, ncseg);
            overlaps.Fill(0);
            if (seg.Length() != cseg.Length())
                throw new Exception("CHECK_ARG: (seg.Length()==cseg.Length())");
            for (int i = 0; i < seg.Length(); i++)
                overlaps[seg.At1d(i), cseg.At1d(i)]++;
            outsegments.Clear();
            outsegments.Resize(ncseg);
            for (int i = 0; i < nseg; i++)
            {
                int j = NarrayRowUtil.RowArgMax(overlaps, i);
                if (!(j >= 0 && j < ncseg))
                    throw new Exception("ASSERT: (j>=0 && j<ncseg)");
                if (outsegments[j] == null)
                    outsegments[j] = new Intarray();
                outsegments[j].Push(i);
            }
        }
    }
}
