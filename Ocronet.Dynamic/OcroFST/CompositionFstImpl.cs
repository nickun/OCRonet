using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ocronet.Dynamic.Interfaces;

namespace Ocronet.Dynamic.OcroFST
{
    public class CompositionFstImpl : CompositionFst
    {
        public IGenericFst l1;
        public IGenericFst l2;
        public int override_start;
        public int override_finish;

        public CompositionFstImpl(IGenericFst l1, IGenericFst l2,
                               int o_s, int o_f)
        {
            override_start = o_s;
            override_finish = o_f;

            if (l1.nStates() == 0)
                throw new Exception("CHECK_ARG: l1->nStates() > 0");
            if (l2.nStates() == 0)
                throw new Exception("CHECK_ARG: l2->nStates() > 0");

            // this should be here, not in the initializers.
            // (otherwise if CHECKs throw an exception, bad things happen)
            this.l1 = l1;
            this.l2 = l2;
        }

        public override string Description
        {
            get { return "CompositionLattice"; }
        }

        public override IGenericFst Move1()
        {
            IGenericFst result = l1;
            l1 = null;
            return result;
        }

        public override IGenericFst Move2()
        {
            IGenericFst result = l2;
            l2 = null;
            return result;
        }

        public override void CheckOwnsNothing()
        {
            if (l1 != null)
                throw new Exception("ALWAYS_ASSERT(!l1)");
            if (l2 != null)
                throw new Exception("ALWAYS_ASSERT(!l2)");
        }

        public override int nStates()
        {
            return l1.nStates() * l2.nStates();
        }

        public int Combine(int i1, int i2)
        {
            return i1 * l2.nStates() + i2;
        }

        public override int GetStart()
        {
            int s1 = override_start >= 0 ? override_start : l1.GetStart();
            return Combine(s1, l2.GetStart());
        }

        public override void SplitIndex(out int result1, out int result2, int index)
        {
            int k = l2.nStates();
            result1 = index / k;
            result2 = index % k;
        }

        public override void SplitIndices(Intarray result1, Intarray result2, Intarray indices)
        {
            result1.MakeLike(indices);
            result2.MakeLike(indices);
            int k = l2.nStates();
            for (int i = 0; i < indices.Length(); i++)
            {
                result1.Put1d(i, indices.At1d(i) / k);
                result2.Put1d(i, indices.At1d(i) % k);
            }
        }

        public override float GetAcceptCost(int node)
        {
            int i1 = node / l2.nStates();
            int i2 = node % l2.nStates();
            float cost1;
            if (override_finish >= 0)
                cost1 = i1 == override_finish ? 0f : 1e38f;
            else
                cost1 = l1.GetAcceptCost(i1);
            return cost1 + l2.GetAcceptCost(i2);
        }

        public override void Load(string path)
        {
            // Saving compositions can't be implemented because we need to
            // delegate save() to the operands, but they need to accept streams,
            // not file names. Fortunately, we don't need it. --IM
            throw new NotImplementedException("CompositionFstImpl.Load unimplemented");
        }

        public override void Save(string path)
        {
            throw new NotImplementedException("CompositionFstImpl.Save unimplemented");
        }

        public override void Arcs(Intarray ids, Intarray targets, Intarray outputs, Floatarray costs, int node)
        {
            int n1 = node / l2.nStates();
            int n2 = node % l2.nStates();
            Intarray ids1 = new Intarray();
            Intarray ids2 = new Intarray();
            Intarray t1 = new Intarray();
            Intarray t2 = new Intarray();
            Intarray o1 = new Intarray();
            Intarray o2 = new Intarray();
            Floatarray c1 = new Floatarray();
            Floatarray c2 = new Floatarray();
            l1.Arcs(ids1, t1, o1, c1, n1);
            l2.Arcs(ids2, t2, o2, c2, n2);

            // sort & permute
            Intarray p1 = new Intarray();
            Intarray p2 = new Intarray();

            NarrayUtil.Quicksort(p1, o1);
            NarrayUtil.Permute(ids1, p1);
            NarrayUtil.Permute(t1, p1);
            NarrayUtil.Permute(o1, p1);
            NarrayUtil.Permute(c1, p1);

            NarrayUtil.Quicksort(p2, ids2);
            NarrayUtil.Permute(ids2, p2);
            NarrayUtil.Permute(t2, p2);
            NarrayUtil.Permute(o2, p2);
            NarrayUtil.Permute(c2, p2);

            int k1, k2;
            // l1 epsilon moves
            for (k1 = 0; k1 < o1.Length() && o1.At1d(k1) == 0; k1++)
            {
                ids.Push(ids1.At1d(k1));
                targets.Push(Combine(t1.At1d(k1), n2));
                outputs.Push(0);
                costs.Push(c1.At1d(k1));
            }
            // l2 epsilon moves
            for (k2 = 0; k2 < o2.Length() && ids2.At1d(k2) == 0; k2++)
            {
                ids.Push(0);
                targets.Push(Combine(n1, t2.At1d(k2)));
                outputs.Push(o2.At1d(k2));
                costs.Push(c2.At1d(k2));
            }
            // non-epsilon moves
            while (k1 < o1.Length() && k2 < ids2.Length())
            {
                while (k1 < o1.Length() && o1.At1d(k1) < ids2.At1d(k2)) k1++;
                if (k1 >= o1.Length()) break;
                while (k2 < ids2.Length() && o1.At1d(k1) > ids2.At1d(k2)) k2++;
                while (k1 < o1.Length() && k2 < ids2.Length() && o1.At1d(k1) == ids2.At1d(k2))
                {
                    for (int j = k2; j < ids2.Length() && o1.At1d(k1) == ids2.At1d(j); j++)
                    {
                        ids.Push(ids1.At1d(k1));
                        targets.Push(Combine(t1.At1d(k1), t2.At1d(j)));
                        outputs.Push(o2.At1d(j));
                        costs.Push(c1.At1d(k1) + c2.At1d(j));
                    }
                    k1++;
                }
            }
        }

        public override double BestPath(out string result)
        {
            throw new NotImplementedException("NIY");
        }


    }
}
