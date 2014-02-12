using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ocronet.Dynamic.Interfaces;

namespace Ocronet.Dynamic.OcroFST
{
    public class AStarSearch
    {
        IGenericFst fst;

        Intarray came_from; // the previous node in the best path; 
                            // -1 for unseen, self for the start
        int accepted_from;
        float g_accept;     // best cost for accept so far
        int n;              // the number of nodes; also the virtual accept index
        Heap heap;

        public Floatarray g;       // the cost of the best path from the start to here

        public virtual double Heuristic(int index)
        {
            return 0;
        }

        public AStarSearch(IGenericFst fst)
        {
            this.fst = fst;
            this.accepted_from = -1;
            this.heap = new Heap(fst.nStates() + 1);
            this.n = fst.nStates();
            this.came_from = new Intarray(n);
            this.came_from.Fill(-1);
            this.g = new Floatarray(n);
            // insert the start node
            int s = fst.GetStart();
            g[s] = 0;
            came_from[s] = s;
            heap.Push(s, Convert.ToSingle(Heuristic(s)));
        }

        public bool Step()
        {
            int node = heap.Pop();
            if (node == n)
                return true;  // accept has popped up

            // get outbound arcs
            Intarray inputs = new Intarray();
            Intarray targets = new Intarray();
            Intarray outputs = new Intarray();
            Floatarray costs = new Floatarray();
            fst.Arcs(inputs, targets, outputs, costs, node);
            for (int i = 0; i < targets.Length(); i++)
            {
                int t = targets[i];
                if (came_from[t] == -1 || g[node] + costs[i] < g[t])
                {
                    // relax the edge
                    came_from[t] = node;
                    g[t] = g[node] + costs[i];
                    heap.Push(t, g[t] + Convert.ToSingle(Heuristic(t)));
                }
            }
            if (accepted_from == -1
                || g[node] + fst.GetAcceptCost(node) < g_accept)
            {
                // relax the accept edge
                accepted_from = node;
                g_accept = g[node] + fst.GetAcceptCost(node);
                heap.Push(n, g_accept);
            }
            return false;
        }

        public bool Loop()
        {
            while (heap.Length() > 0)
            {
                if (Step())
                    return true;
            }
            return false;
        }

        public bool reconstruct_vertices(Intarray result_vertices)
        {
            Intarray vertices = new Intarray();
            if (accepted_from == -1)
                return false;
            vertices.Push(accepted_from);
            int last = accepted_from;
            int next;
            while ((next = came_from[last]) != last)
            {
                vertices.Push(next);
                last = next;
            }
            NarrayUtil.Reverse(result_vertices, vertices);
            return true;
        }

        public void reconstruct_edges(Intarray inputs,
                               Intarray outputs,
                               Floatarray costs,
                               Intarray vertices)
        {
            int n = vertices.Length();
            inputs.Resize(n);
            outputs.Resize(n);
            costs.Resize(n);
            for (int i = 0; i < n - 1; i++)
            {
                int source = vertices[i];
                int target = vertices[i + 1];
                Intarray out_ins = new Intarray();
                Intarray out_targets = new Intarray();
                Intarray out_outs = new Intarray();
                Floatarray out_costs = new Floatarray();
                fst.Arcs(out_ins, out_targets, out_outs, out_costs, source);

                costs[i] = 1e38f;

                // find the best arc
                for (int j = 0; j < out_targets.Length(); j++)
                {
                    if (out_targets[j] != target) continue;
                    if (out_costs[j] < costs[i])
                    {
                        inputs[i] = out_ins[j];
                        outputs[i] = out_outs[j];
                        costs[i] = out_costs[j];
                    }
                }
            }
            inputs[n - 1] = 0;
            outputs[n - 1] = 0;
            costs[n - 1] = fst.GetAcceptCost(vertices[n - 1]);
        }
    }
}
