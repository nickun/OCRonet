using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ocronet.Dynamic.Interfaces
{
    public abstract class IDpSegmenter : ISegmentLine
    {
        public float down_cost;
        public float outside_diagonal_cost;
        public float outside_diagonal_cost_r;
        public float inside_diagonal_cost;
        public float boundary_diagonal_cost;
        public float inside_weight;
        public float boundary_weight;
        public float outside_weight;
        public int min_range;
        public float cost_smooth;
        public float min_thresh;
        public Intarray dimage;
    }
}
