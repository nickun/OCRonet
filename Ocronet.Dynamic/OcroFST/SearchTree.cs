using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ocronet.Dynamic.OcroFST
{
    /// <summary>
    /// A SearchTree contains all vertices that were ever touched during the
    /// search, and can produce a prehistory for every ID.
    /// </summary>
    public class SearchTree
    {
        public Intarray parents;
        public Intarray inputs;
        public Intarray outputs;
        public Intarray v1; // vertices from FST 1
        public Intarray v2; // vertices from FST 2
        public Floatarray costs;

        public SearchTree()
        {
            parents = new Intarray();
            inputs = new Intarray();
            outputs = new Intarray();
            v1 = new Intarray();
            v2 = new Intarray();
            costs = new Floatarray();
        }

        public void Clear()
        {
            parents.Clear();
            inputs.Clear();
            outputs.Clear();
            v1.Clear();
            v2.Clear();
            costs.Clear();
        }

        public void Get(Intarray r_vertices1,
                 Intarray r_vertices2,
                 Intarray r_inputs,
                 Intarray r_outputs,
                 Floatarray r_costs,
                 int id)
        {
            Intarray t_v1 = new Intarray(); // vertices
            Intarray t_v2 = new Intarray(); // vertices
            Intarray t_i = new Intarray(); // inputs
            Intarray t_o = new Intarray(); // outputs
            Floatarray t_c = new Floatarray(); // costs
            int current = id;
            while (current != -1)
            {
                t_v1.Push(v1[current]);
                t_v2.Push(v2[current]);
                t_i.Push(inputs[current]);
                t_o.Push(outputs[current]);
                t_c.Push(costs[current]);
                current = parents[current];
            }

            NarrayUtil.Reverse(r_vertices1, t_v1);
            NarrayUtil.Reverse(r_vertices2, t_v2);
            NarrayUtil.Reverse(r_inputs, t_i);
            NarrayUtil.Reverse(r_outputs, t_o);
            NarrayUtil.Reverse(r_costs, t_c);
        }

        public int Add(int parent, int vertex1, int vertex2,
                   int input, int output, float cost)
        {
            int n = parents.Length();
            //logger.format("stree: [%d]: parent %d, v1 %d, v2 %d, cost %f",
            //               n, parent, vertex1, vertex2, cost);
            parents.Push(parent);
            v1.Push(vertex1);
            v2.Push(vertex2);
            inputs.Push(input);
            outputs.Push(output);
            costs.Push(cost);
            return n;
        }
    }
}
