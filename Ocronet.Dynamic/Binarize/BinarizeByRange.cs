using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ocronet.Dynamic.Interfaces;
using Ocronet.Dynamic.Utils;
using Ocronet.Dynamic.ImgLib;

namespace Ocronet.Dynamic.Binarize
{
    /// <summary>
    /// Simple document binarization based on thresholding the min/max range
    /// </summary>
    public class BinarizeByRange : IBinarize
    {
        protected float fraction;

        public override string Description
        {
            get { return "Binarize by thresholding the range between min(image) and max(image)"; }
        }

        public override string Name
        {
            get { return "binarizerange"; }
        }

        public BinarizeByRange()
        {
            PDef("f", 0.5, "Fraction");
            fraction = 0.5f;
        }

        public override void Binarize(Bytearray outa, Bytearray ina_)
        {
            fraction = (float)PGetf("f");
            Floatarray ina = new Floatarray();
            ina.Copy(ina_);
            binarize_by_range(outa, ina, fraction);
        }

        public static void binarize_by_range(Bytearray outa, Floatarray ina, float fraction)
        {
            float imin = NarrayUtil.Min(ina);
            float imax = NarrayUtil.Max(ina);
            float thresh = (int)(imin + (imax - imin) * fraction);
            outa.MakeLike(ina);
            for (int i = 0; i < ina.Length1d(); i++)
            {
                if (ina.At1d(i) > thresh) outa.Put1d(i, 255);
                else outa.Put1d(i, 0);
            }
        }
    }
}
