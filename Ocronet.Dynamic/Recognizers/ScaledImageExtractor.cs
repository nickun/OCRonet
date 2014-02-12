using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ocronet.Dynamic.ImgLib;
using Ocronet.Dynamic.Utils;

namespace Ocronet.Dynamic.Recognizers
{
    public class ScaledImageExtractor : IExtractor
    {
        public override string Name
        {
            get { return "scaledfe"; }
        }

        public ScaledImageExtractor()
        {
            PDef("csize", 30, "taget image size");
            PDef("aa", 0, "anti-aliasing");
            PDef("noupscale", 1, "no upscaling");
            PDef("indent", 0, "indent from edge");
        }

        protected void rescale(Floatarray outv, Floatarray sub)
        {
            if (sub.Rank() != 2)
                throw new Exception("CHECK_ARG: sub.Rank()==2");
            int csize = PGeti("csize");
            int indent = PGeti("indent");
            float s = Math.Max(sub.Dim(0), sub.Dim(1)) / (float)(csize - indent - indent);
            if (PGeti("noupscale") > 0 && s < 1.0f)
                s = 1.0f;
            float sig = s * PGetf("aa");
            float dx = (csize * s - sub.Dim(0)) / 2;
            float dy = (csize * s - sub.Dim(1)) / 2;
            if (sig > 1e-3f)
                Gauss.Gauss2d(sub, sig, sig);
            outv.Resize(csize, csize);
            outv.Fill(0f);
            for (int i = 0; i < csize; i++)
            {
                for (int j = 0; j < csize; j++)
                {
                    float x = i * s - dx;
                    float y = j * s - dy;
                    if (x < 0 || x >= sub.Dim(0)) continue;
                    if (y < 0 || y >= sub.Dim(1)) continue;
                    float value = ImgOps.bilin(sub, x, y);
                    outv[i, j] = value;
                }
            }
            /*Global.Debugf("fe", "{0} {1} ({2}) -> {3} {4} ({5})\n",
                   sub.Dim(0), sub.Dim(1), NarrayUtil.Max(sub),
                   outv.Dim(0), outv.Dim(1), NarrayUtil.Max(outv));*/
        }

        public override void Extract(Narray<Floatarray> outarrays, Floatarray inarray)
        {
            outarrays.Clear();
            Floatarray image = new Floatarray();
            outarrays.Push(image);
            rescale(image, inarray);
            // image /= Math.Max(1.0f, NarrayUtil.Max(image));
        }
    }
}
