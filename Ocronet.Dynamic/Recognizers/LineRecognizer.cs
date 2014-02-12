using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ocronet.Dynamic.Interfaces;
using Ocronet.Dynamic.Component;

namespace Ocronet.Dynamic.Recognizers
{
    public class LineRecognizer
    {
        ISegmentLine segmenter;
        IGrouper grouper;
        IModel cmodel;
        int best;
        float maxcost;
        float reject_cost;
        float min_height;
        float rho_scale;
        float maxoverlap;
        ISpaceModel spacemodel;

        public void SetDefaults()
        {
            segmenter = ComponentCreator.MakeComponent<ISegmentLine>("DpSegmenter");
            grouper = ComponentCreator.MakeComponent<IGrouper>("SimpleGrouper");
            cmodel = null;
            best = 10;
            maxcost = 30.0f;
            reject_cost = 10.0f;
            min_height = 0.5f;
            rho_scale = 1.0f;
            maxoverlap = 0.8f;
            spacemodel = new SimpleSpaceModel();
            //linemodel = null;
        }
    }
}
