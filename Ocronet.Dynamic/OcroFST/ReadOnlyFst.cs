using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ocronet.Dynamic.Interfaces;

namespace Ocronet.Dynamic.OcroFST
{
    public abstract class ReadOnlyFst : IGenericFst
    {
        private int oops() { throw new Exception("this FST is read-only"); }

        public override void Clear()
        {
            oops();
        }

        public override int NewState()
        {
            return oops();
        }

        public override void AddTransition(int from, int to, int output, float cost, int input)
        {
            oops();
        }

        public override void SetStart(int node)
        {
            oops();
        }

        public override void SetAccept(int node, float cost = 0.0f)
        {
            oops();
        }

        public override int Special(string s)
        {
            return oops();
        }
    }
}
