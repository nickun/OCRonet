using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ocronet.Dynamic.Utils;
using Ocronet.Dynamic.ImgLib;

namespace Ocronet.Dynamic.Recognizers
{
    public class BiggestCcExtractor : IExtractor
    {
        public override string Name
        {
            get { return "biggestcc"; }
        }

        public BiggestCcExtractor()
        {
            PDef("csize", 30, "taget image size");
            PDef("aa", 0, "anti-aliasing");
            PDef("noupscale", 1, "no upscaling");
            PDef("threshold", 0.25, "threshold for finding the largest cc");
            PDef("pad", 1, "amount to pad bounding rectangle by");
        }

        protected void rescale(Floatarray v, Floatarray input)
        {
            if (input.Rank() != 2)
                throw new Exception("CHECK_ARG: sub.Rank()==2");
            
            Floatarray sub = new Floatarray();
            // find the largest connected component
            // and crop to its bounding box
            // (use a binary version of the character
            // to compute the bounding box)
            Intarray components = new Intarray();
            float threshold = PGetf("threshold") * NarrayUtil.Max(input);
            Global.Debugf("biggestcc", "threshold {0}", threshold);
            components.MakeLike(input);
            components.Fill(0);
            for (int i = 0; i < components.Length(); i++)
                components[i] = (input[i] > threshold ? 1 : 0);
            int n = ImgLabels.label_components(ref components);
            Intarray totals = new Intarray(n + 1);
            totals.Fill(0);
            for (int i = 0; i < components.Length(); i++)
                totals[components[i]]++;
            totals[0] = 0;
            Narray<Rect> boxes = new Narray<Rect>();
            ImgLabels.bounding_boxes(ref boxes, components);
            int biggest = NarrayUtil.ArgMax(totals);
            Rect r = boxes[biggest];
            int pad = (int)(PGetf("pad") + 0.5f);
            r.PadBy(pad, pad);
            Global.Debugf("biggestcc", "({0}) {1}[{2}] :: {3} {4} {5} {6}",
                   n, biggest, totals[biggest],
                   r.x0, r.y0, r.x1, r.y1);

            // now perform normal feature extraction
            // (use the original grayscale input)
            sub = input;
            ImgMisc.Crop(sub, r);
            int csize = PGeti("csize");
            float s = Math.Max(sub.Dim(0), sub.Dim(1))/(float)csize;
            if(PGetf("noupscale") > 0 && s < 1.0f)
                s = 1.0f;
            float sig = s * PGetf("aa");
            float dx = (csize*s-sub.Dim(0))/2f;
            float dy = (csize*s-sub.Dim(1))/2f;
            if(sig > 1e-3f)
                Gauss.Gauss2d(sub, sig, sig);
            v.Resize(csize, csize);
            v.Fill(0f);
            for (int i = 0; i < csize; i++)
            {
                for (int j = 0; j < csize; j++)
                {
                    float x = i * s - dx;
                    float y = j * s - dy;
                    if (x < 0 || x >= sub.Dim(0)) continue;
                    if (y < 0 || y >= sub.Dim(1)) continue;
                    float value = ImgOps.bilin(sub, x, y);
                    v[i, j] = value;
                }
            }
            /*Global.Debugf("biggestcc", "{0} {1} ({2}) -> {3} {4} ({5})",
                   sub.Dim(0), sub.Dim(1), NarrayUtil.Max(sub),
                   v.Dim(0), v.Dim(1), NarrayUtil.Max(v));*/

        }

        public override void Extract(Narray<Floatarray> outarrays, Floatarray inarray)
        {
            outarrays.Clear();
            Floatarray image = outarrays.Push();
            rescale(image, inarray);
            //image /= Math.Max(1.0f, NarrayUtil.Max(image));
        }
    }
}
