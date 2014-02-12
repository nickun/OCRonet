using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ocronet.Dynamic.OcroFST
{
    public class BeamSearch
    {
        OcroFST fst1;
        OcroFST fst2;
        SearchTree stree;

        Intarray beam; // indices into stree
        Floatarray beamcost; // global cost, corresponds to the beam

        PriorityQueue nbest;
        Intarray all_inputs;
        Intarray all_targets1;
        Intarray all_targets2;
        Intarray all_outputs;
        Floatarray all_costs;
        Intarray parent_trails; // indices into the beam
        int beam_width;
        int accepted_from1;
        int accepted_from2;
        float g_accept;   // best cost for accept so far
        int best_so_far;  // ID into stree (-1 for start)
        float best_cost_so_far;

        public BeamSearch(OcroFST fst1, OcroFST fst2, int beam_width)
        {
            stree = new SearchTree();
            beam = new Intarray();
            beamcost = new Floatarray();
            all_inputs = new Intarray();
            all_targets1 = new Intarray();
            all_targets2 = new Intarray();
            all_outputs = new Intarray();
            all_costs = new Floatarray();
            parent_trails = new Intarray();

            this.fst1 = fst1;
            this.fst2 = fst2;
            this.nbest = new PriorityQueue(beam_width);
            this.beam_width = beam_width;
            accepted_from1 = -1;
            accepted_from2 = -1;
        }

        public void Clear()
        {
            nbest.Clear();
            all_targets1.Clear();
            all_targets2.Clear();
            all_inputs.Clear();
            all_outputs.Clear();
            all_costs.Clear();
            parent_trails.Clear();
        }

        /// <summary>
        /// This looks at the transition from state pair
        /// (f1,f2) -> (t1,t2), withthe given cost.
        /// </summary>
        public void Relax( int f1, int f2,   // input state pair
                           int t1, int t2,   // output state pair
                           float cost,      // transition cost
                           int arc_id1,      // (unused)
                           int arc_id2,      // (unused)
                           int input,        // input label
                           int intermediate, // (unused)
                           int output,       // output label
                           float base_cost, // cost of the path so far
                           int trail_index )
        {
            //logger.format("relaxing %d %d -> %d %d (bcost %f, cost %f)", f1, f2, t1, t2, base_cost, cost);

            if (!nbest.AddReplacingId(t1 * fst2.nStates() + t2,
                                       all_costs.Length(),
                                       -base_cost - cost))
                return;

            //logger.format("nbest changed");
            //nbest.log(logger);

            if (input > 0)
            {
                // The candidate for the next beam is stored in all_XX arrays.
                // (can we store it in the stree instead?)
                all_inputs.Push(input);
                all_targets1.Push(t1);
                all_targets2.Push(t2);
                all_outputs.Push(output);
                all_costs.Push(cost);
                parent_trails.Push(trail_index);
            }
            else
            {
                // Beam control hack
                // -----------------
                // if a node is important (changes nbest) AND its input is 0,
                // then it's added to the CURRENT beam.

                //logger.format("pushing control point from trail %d to %d, %d",
                //trail_index, t1, t2);
                int new_node = stree.Add(beam[trail_index], t1, t2, input, output, (float)cost);
                beam.Push(new_node);
                beamcost.Push(base_cost + cost);

                // This is a stub entry indicating that the node should not
                // be added to the next generation beam.
                all_inputs.Push(0);
                all_targets1.Push(-1);
                all_targets2.Push(-1);
                all_outputs.Push(0);
                all_costs.Push(0);
                parent_trails.Push(-1);
            }
        }

        /// <summary>
        /// Call relax() for each arc going out of the given node.
        /// </summary>
        public void Traverse(int n1, int n2, float cost, int trail_index)
        {
            //logger.format("traversing %d %d", n1, n2);
            Intarray o1 = fst1.Outputs(n1);
            Intarray i1 = fst1.Inputs(n1);
            Intarray t1 = fst1.Targets(n1);
            Floatarray c1 = fst1.Costs(n1);

            Intarray o2 = fst2.Outputs(n2);
            Intarray i2 = fst2.Inputs(n2);
            Intarray t2 = fst2.Targets(n2);
            Floatarray c2 = fst2.Costs(n2);

            // for optimization
            int[] O1 = o1.data;
            int[] O2 = o2.data;
            int[] I1 = i1.data;
            int[] I2 = i2.data;
            int[] T1 = t1.data;
            int[] T2 = t2.data;
            float[] C1 = c1.data;
            float[] C2 = c2.data;

            // Relax outbound arcs in the composition
            int k1, k2;


            // relaxing fst1 RHO moves
            // these can be rho->rho or x->rho moves
            for (k1 = 0; k1 < o1.Length() && O1[k1] == FstUtil.L_RHO; k1++)
            {
                for (int j = 0; j < o2.Length(); j++)
                {
                    if (I2[j] <= FstUtil.L_EPSILON) continue;
                    // if it's rho->rho, then pick up the label,
                    // if it's x->rho leave it alone
                    int inn = I1[k1] == FstUtil.L_RHO ? I2[j] : I1[k1];
                    Relax(n1, n2,         // from pair
                          T1[k1], T2[j],  // to pair
                          C1[k1] + C2[j], // cost
                          k1, j,         // arc ids
                          inn, I2[j], O2[j],   // input, intermediate, output
                          cost, trail_index);
                }
            }

            // relaxing fst2 RHO moves
            // these can be rho->rho or rho->x moves
            for (k2 = 0; k2 < o2.Length() && I2[k2] == FstUtil.L_RHO; k2++)
            {
                for (int j = 0; j < o1.Length(); j++)
                {
                    if (O1[j] <= FstUtil.L_EPSILON) continue;
                    // if it's rho->rho, then pick up the label,
                    // if it's rho->x leave it alone
                    int outn = O2[k2] == FstUtil.L_RHO ? O1[j] : O2[k2];
                    Relax(n1, n2,       // from pair
                          T1[j], T2[k2],   // to pair
                          C1[j] + C2[k2],       // cost
                          j, k2,       // arc ids
                          I1[j], O1[j], outn, // input, intermediate, output
                          cost, trail_index);
                }
            }

            // relaxing fst1 EPSILON moves
            for (k1 = 0; k1 < o1.Length() && O1[k1] == FstUtil.L_EPSILON; k1++)
            {
                Relax(n1, n2,       // from pair
                      T1[k1], n2,   // to pair
                      C1[k1],       // cost
                      k1, -1,       // arc ids
                      I1[k1], 0, 0, // input, intermediate, output
                      cost, trail_index);
            }

            // relaxing fst2 EPSILON moves
            for (k2 = 0; k2 < o2.Length() && I2[k2] == FstUtil.L_EPSILON; k2++)
            {
                Relax(n1, n2,       // from pair
                      n1, T2[k2],   // to pair
                      C2[k2],       // cost
                      -1, k2,       // arc ids
                      0, 0, O2[k2], // input, intermediate, output
                      cost, trail_index);
            }

            // relaxing non-epsilon moves
            while (k1 < o1.Length() && k2 < i2.Length())
            {
                while (k1 < o1.Length() && O1[k1] < I2[k2]) k1++;
                if (k1 >= o1.Length()) break;
                while (k2 < i2.Length() && O1[k1] > I2[k2]) k2++;
                while (k1 < o1.Length() && k2 < i2.Length() && O1[k1] == I2[k2])
                {
                    for (int j = k2; j < i2.Length() && O1[k1] == I2[j]; j++)
                        Relax(n1, n2,           // from pair
                              T1[k1], T2[j],    // to pair
                              C1[k1] + C2[j],   // cost
                              k1, j,            // arc ids
                              I1[k1], O1[k1], O2[j], // input, intermediate, output
                              cost, trail_index);
                    k1++;
                }
            }
        }

        /// <summary>
        /// The main loop iteration.
        /// </summary>
        public void Radiate()
        {
            Clear();

            //logger("beam", beam);
            //logger("beamcost", beamcost);

            int control_beam_start = beam.Length();
            for (int i = 0; i < control_beam_start; i++)
                TryAccept(i);

            // in this loop, traversal may add "control nodes" to the beam
            for (int i = 0; i < beam.Length(); i++)
            {
                Traverse(stree.v1[beam[i]], stree.v2[beam[i]],
                         beamcost[i], i);
            }

            // try accepts from control beam nodes
            // (they're not going to the next beam)
            for (int i = control_beam_start; i < beam.Length(); i++)
                TryAccept(i);


            Intarray new_beam = new Intarray();
            Floatarray new_beamcost = new Floatarray();
            for (int i = 0; i < nbest.Length(); i++)
            {
                int k = nbest.Tag(i);
                if (parent_trails[k] < 0) // skip the control beam nodes
                    continue;
                new_beam.Push(stree.Add(beam[parent_trails[k]],
                                        all_targets1[k], all_targets2[k],
                                        all_inputs[k], all_outputs[k],
                                        all_costs[k]));
                new_beamcost.Push(beamcost[parent_trails[k]] + all_costs[k]);
                //logger.format("to new beam: trail index %d, stree %d, target %d,%d",
                //k, new_beam[new_beam.length() - 1], all_targets1[k], all_targets2[k]);
            }
            //move(beam, new_beam);
            beam.Move(new_beam);
            //move(beamcost, new_beamcost);
            beamcost.Move(new_beamcost);
        }

        /// <summary>
        /// Relax the accept arc from the beam node number i.
        /// Origin name: try_accept
        /// </summary>
        public void TryAccept(int i)
        {
            float a_cost1 = fst1.GetAcceptCost(stree.v1[beam[i]]);
            float a_cost2 = fst2.GetAcceptCost(stree.v2[beam[i]]);
            float candidate = beamcost[i] + a_cost1 + a_cost2;
            if (candidate < best_cost_so_far)
            {
                //logger.format("accept from beam #%d (stree %d), cost %f",
                //              i, beam[i], candidate);
                best_so_far = beam[i];
                best_cost_so_far = candidate;
            }
        }

        public void BestPath(Intarray v1, Intarray v2, Intarray inputs,
                      Intarray outputs, Floatarray costs)
        {
            stree.Clear();

            beam.Resize(1);
            beamcost.Resize(1);
            beam[0] = stree.Add(-1, fst1.GetStart(), fst2.GetStart(), 0, 0, 0);
            beamcost[0] = 0;

            best_so_far = 0;
            best_cost_so_far = fst1.GetAcceptCost(fst1.GetStart()) +
                               fst2.GetAcceptCost(fst1.GetStart());

            while (beam.Length() > 0)
                Radiate();

            stree.Get(v1, v2, inputs, outputs, costs, best_so_far);
            costs.Push(fst1.GetAcceptCost(stree.v1[best_so_far]) +
                       fst2.GetAcceptCost(stree.v2[best_so_far]));

            //logger("costs", costs);
        }




        public static void beam_search(
                Intarray vertices1,
                Intarray vertices2,
                Intarray inputs,
                Intarray outputs,
                Floatarray costs,
                OcroFST fst1,
                OcroFST fst2,
                int beam_width)
        {
            BeamSearch b = new BeamSearch(fst1, fst2, beam_width);
            //CHECK(L_SIGMA < L_EPSILON);
            //CHECK(L_RHO < L_PHI);
            //CHECK(L_PHI < L_EPSILON);
            //CHECK(L_EPSILON < 1);
            fst1.SortByOutput();
            fst2.SortByInput();
            b.BestPath(vertices1, vertices2, inputs, outputs, costs);
        }

        public static double beam_search(out string result, OcroFST fst1, OcroFST fst2,
                                         int beam_width)
        {
            Intarray v1 = new Intarray();
            Intarray v2 = new Intarray();
            Intarray i = new Intarray();
            Intarray o = new Intarray();
            Floatarray c = new Floatarray();
            //fprintf(stderr,"starting beam search\n");
            beam_search(v1, v2, i, o, c, fst1, fst2, beam_width);
            //fprintf(stderr,"finished beam search\n");
            FstUtil.remove_epsilons(out result, o);
            return NarrayUtil.Sum(c);
        }

        public static double beam_search(out string result, Intarray inputs, Floatarray costs, OcroFST fst1, OcroFST fst2,
                                         int beam_width)
        {
            Intarray v1 = new Intarray();
            Intarray v2 = new Intarray();
            Intarray o = new Intarray();
            //fprintf(stderr,"starting beam search\n");
            beam_search(v1, v2, inputs, o, costs, fst1, fst2, beam_width);
            //fprintf(stderr,"finished beam search\n");
            FstUtil.remove_epsilons(out result, o);
            return NarrayUtil.Sum(costs);
        }
    }
}
