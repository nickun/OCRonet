using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ocronet.Dynamic.Interfaces
{
    public abstract class ICurvedCutSegmenter : IDisposable
    {
        public int down_cost;
        public int outside_diagonal_cost;
        public int inside_diagonal_cost;
        public int boundary_diagonal_cost;
        public int inside_weight;
        public int boundary_weight;
        public int outside_weight;
        public int min_range;
        public float min_thresh;

        protected abstract void params_for_lines();
        public abstract void FindAllCuts();
        public abstract void FindBestCuts();
        public abstract void SetImage(Bytearray image);
        public void Dispose() { }
    }
}
