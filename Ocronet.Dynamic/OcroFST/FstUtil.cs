using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ocronet.Dynamic.Interfaces;

namespace Ocronet.Dynamic.OcroFST
{
    public static class FstUtil
    {
        public const int L_SIGMA = -4;
        public const int L_RHO = -3;
        public const int L_PHI = -2;
        public const int L_EPSILON = 0;

        private static Random rnd = new Random();


        /// <summary>
        /// Copy one FST to another.
        /// </summary>
        /// <param name="dst">The destination. Will be cleared before copying.</param>
        /// <param name="src">The FST to copy.</param>
        public static void fst_copy(IGenericFst dst, IGenericFst src)
        {
            dst.Clear();
            int n = src.nStates();
            for (int i = 0; i < n; i++)
                dst.NewState();
            dst.SetStart(src.GetStart());
            for (int i = 0; i < n; i++)
            {
                dst.SetAccept(i, src.GetAcceptCost(i));
                Intarray targets = new Intarray(), outputs = new Intarray(), inputs = new Intarray();
                Floatarray costs = new Floatarray();
                src.Arcs(inputs, targets, outputs, costs, i);
                int inlen = inputs.Length();
                if (inlen != targets.Length())
                    throw new Exception("ASSERT: inputs.length() == targets.length()");
                if (inlen != outputs.Length())
                    throw new Exception("ASSERT: inputs.length() == outputs.length()");
                if (inlen != costs.Length())
                    throw new Exception("ASSERT: inputs.length() == costs.length()");
                for (int j = 0; j < inputs.Length(); j++)
                    dst.AddTransition(i, targets.At1d(j), outputs.At1d(j), costs.At1d(j), inputs.At1d(j));
            }
        }

        /// <summary>
        /// Reverse the FST's arcs, adding a new start vertex (former accept).
        /// </summary>
        public static void fst_copy_reverse(IGenericFst dst, IGenericFst src, bool no_accept = false)
        {
            dst.Clear();
            int n = src.nStates();
            for (int i = 0; i <= n; i++)
                dst.NewState();
            if (!no_accept)
                dst.SetAccept(src.GetStart());
            dst.SetStart(n);
            for (int i = 0; i < n; i++)
            {
                dst.AddTransition(n, i, 0, src.GetAcceptCost(i), 0);
                Intarray targets = new Intarray(), outputs = new Intarray(), inputs = new Intarray();
                Floatarray costs = new Floatarray();
                src.Arcs(inputs, targets, outputs, costs, i);
                if (inputs.Length() != targets.Length())
                    throw new Exception("ASSERT: inputs.length() == targets.length()");
                if (inputs.Length() != outputs.Length())
                    throw new Exception("ASSERT: inputs.length() == outputs.length()");
                if (inputs.Length() != costs.Length())
                    throw new Exception("ASSERT: inputs.length() == costs.length()");
                for (int j = 0; j < inputs.Length(); j++)
                    dst.AddTransition(targets.At1d(j), i, outputs.At1d(j), costs.At1d(j), inputs.At1d(j));
            }
        }

        /// <summary>
        /// Copy one FST to another, preserving only lowest-cost arcs.
        /// This is useful for visualization.
        /// </summary>
        /// <param name="dst">The destination. Will be cleared before copying.</param>
        /// <param name="src">The FST to copy.</param>
        public static void fst_copy_best_arcs_only(IGenericFst dst, IGenericFst src)
        {
            dst.Clear();
            int n = src.nStates();
            for (int i = 0; i < n; i++)
                dst.NewState();
            dst.SetStart(src.GetStart());
            for(int i = 0; i < n; i++)
            {
                dst.SetAccept(i, src.GetAcceptCost(i));
                Intarray targets = new Intarray(), outputs = new Intarray(), inputs = new Intarray();
                Floatarray costs = new Floatarray();
                src.Arcs(inputs, targets, outputs, costs, i);
                int inlen = inputs.Length();
                if (inlen != targets.Length())
                    throw new Exception("ASSERT: inputs.length() == targets.length()");
                if (inlen != outputs.Length())
                    throw new Exception("ASSERT: inputs.length() == outputs.length()");
                if (inlen != costs.Length())
                    throw new Exception("ASSERT: inputs.length() == costs.length()");
                Dictionary< int, int > hash = new Dictionary<int,int>();
                for(int j = 0; j < n; j++) {
                    int t = targets[j];
                    int best_so_far = -1;
                    if (hash.ContainsKey(t))
                        best_so_far = hash[t];
                    if(best_so_far == -1 || costs[j] < costs[best_so_far])
                        hash[t] = j;
                }
                Intarray keys = new Intarray();
                //hash.keys(keys);
                keys.Clear();
                foreach (int key in hash.Keys)
                {
                    keys.Push(key);
                }

                for(int k = 0; k < keys.Length(); k++) {
                    int j = hash[keys[k]];
                    dst.AddTransition(i, targets[j], outputs[j], costs[j], inputs[j]);
                }
            }
        }

        /// <summary>
        /// Compose two FSTs.
        /// This function copies the composition of two given FSTs.
        /// That causes expansion (storing all arcs explicitly).
        /// </summary>
        public static void fst_expand_composition(IGenericFst outf, OcroFST f1, OcroFST f2)
        {
            CompositionFst composition = FstFactory.MakeCompositionFst(f1, f2);
            try
            {
                fst_copy(outf, composition);
            }
            catch (Exception ex)
            {
                composition.Move1();
                composition.Move2();
                throw ex;
            }
            composition.Move1();
            composition.Move2();
        }

        /// <summary>
        /// Pick an array element with probability proportional to exp(-cost).
        /// </summary>
        public static int sample_by_costs(Floatarray costs)
        {
            Doublearray p = new Doublearray();
            p.Copy(costs);
            double mincost = NarrayUtil.Min(costs);
            p -= mincost;

            for (int i = 0; i < p.Length(); i++)
                p.UnsafePut1d(i, Math.Exp(-p.UnsafeAt1d(i)));
            double sump = NarrayUtil.Sum(p);
            p /= sump;

            double choice = rnd.NextDouble();
            double s = 0;
            for (int i = 0; i < p.Length(); i++)
            {
                s += p[i];
                if (choice < s)
                    return i;
            }

            // shouldn't happen...
            return costs.Length() - 1;
        }

        /// <summary>
        /// Remove epsilons (zeros) and converts integers to string.
        /// </summary>
        public static void remove_epsilons(out string outs, Intarray a)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < a.Length(); i++)
            {
                if (a[i] > 0)
                    sb.Append((char)a[i]);
            }
            outs = sb.ToString();
        }

        /// <summary>
        /// Randomly sample an FST, assuming any input.
        /// </summary>
        /// <param name="result">The array of output symbols, excluding epsilons.</param>
        /// <param name="fst">The FST.</param>
        /// <param name="max">The maximum length of the result.</param>
        /// <returns>total cost</returns>
        public static double fst_sample(Intarray result, IGenericFst fst, int max=1000)
        {
            double total_cost = 0;
            int current = fst.GetStart();

            for (int counter = 0; counter < max; counter++)
            {
                Intarray inputs  = new Intarray();
                Intarray outputs = new Intarray();
                Intarray targets = new Intarray();
                Floatarray costs = new Floatarray();

                fst.Arcs(inputs, targets, outputs, costs, current);

                // now we need to deal with the costs uniformly, so:
                costs.Push(fst.GetAcceptCost(current));
                int choice = sample_by_costs(costs);
                if (choice == costs.Length() - 1)
                    break;
                result.Push(outputs[choice]);
                total_cost += costs[choice];
                current = targets[choice];
            }
            return total_cost + fst.GetAcceptCost(current);
        }

        /// <summary>
        /// Randomly sample an FST, assuming any input.
        /// </summary>
        public static double fst_sample(out string result, IGenericFst fst, int max)
        {
            Intarray tmp = new Intarray();
            double cost = fst_sample(tmp, fst, max);
            remove_epsilons(out result, tmp);
            return cost;
        }

        /// <summary>
        /// Make an in-place Kleene closure of the FST.
        /// </summary>
        public static void fst_star(IGenericFst fst)
        {
            int s = fst.GetStart();
            fst.SetAccept(s);
            for (int i = 0; i < fst.nStates(); i++)
            {
                double c = fst.GetAcceptCost(i);
                if (c < 1e37)
                    fst.AddTransition(i, s, 0, (float)c, 0);
            }
        }

        /// <summary>
        /// Make a Kleene closure.
        /// </summary>
        public static void fst_star(IGenericFst result, IGenericFst fst)
        {
            fst_copy(result, fst);
            fst_star(result);
        }


        public static void fst_line(IGenericFst fst, string s)
        {
            int n = s.Length;
            Intarray inputs = new Intarray(n);
            for(int j = 0; j < n; j++)
                inputs[j] = (int)s[j];
            Floatarray costs = new Floatarray(n);
            costs.Fill(0f);
            fst.SetString(s, costs, inputs);
        }

        public static void scale_fst(OcroFST fst, float scale)
        {
            if(Math.Abs(scale-1.0f)<1e-6f)
                return;
            for (int i = 0; i < fst.nStates(); i++)
            {
                Floatarray cost = fst.Costs(i);
                cost *= scale;
                float accept = fst.AcceptCost(i);
                if (accept >= 0.0f && accept < 1e37f)
                    fst.SetAcceptCost(i, accept * scale);
            }
        }

    }
}
