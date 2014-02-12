using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Ocronet.Dynamic.OcroFST
{
    public class OcroFSTImpl : OcroFST
    {
        ObjList<Intarray> m_targets;
        ObjList<Intarray> m_inputs;
        ObjList<Intarray> m_outputs;
        ObjList<Floatarray> m_costs;
        Floatarray m_heuristics;
        Floatarray accept_costs;
        int start;
        int flags;

        public OcroFSTImpl()
            : this(0)
        {
        }

        public OcroFSTImpl(int max_size)
        {
            m_targets = new ObjList<Intarray>(max_size);
            m_inputs = new ObjList<Intarray>(max_size);
            m_outputs = new ObjList<Intarray>(max_size);
            m_costs = new ObjList<Floatarray>(max_size);
            m_heuristics = new Floatarray();
            accept_costs = new Floatarray(max_size);
            // init sub arrays
            if (max_size > 0)
            {
                for (int i = 0; i < max_size; i++)
                {
                    m_targets[i] = new Intarray();
                    m_inputs[i] = new Intarray();
                    m_outputs[i] = new Intarray();
                    m_costs[i] = new Floatarray();
                }
            }
            start = 0;
            flags = 0;
        }

        public override string Description
        {
            get { return "Lattice"; }
        }

        protected void CorrectTargetsNull(int vertex)
        {
            if (m_targets[vertex] == null)
                m_targets[vertex] = new Intarray();
        }
        public override Intarray Targets(int vertex)
        {
            return m_targets[vertex];
        }

        protected void CorrectInputsNull(int vertex)
        {
            if (m_inputs[vertex] == null)
                m_inputs[vertex] = new Intarray();
        }
        public override Intarray Inputs(int vertex)
        {
            return m_inputs[vertex];
        }

        protected void CorrectOutputsNull(int vertex)
        {
            if (m_outputs[vertex] == null)
                m_outputs[vertex] = new Intarray();
        }
        public override Intarray Outputs(int vertex)
        {
            return m_outputs[vertex];
        }

        protected void CorrectCostsNull(int vertex)
        {
            if (m_costs[vertex] == null)
                m_costs[vertex] = new Floatarray();
        }
        public override Floatarray Costs(int vertex)
        {
            return m_costs[vertex];
        }

        public override float AcceptCost(int vertex)
        {
            return accept_costs[vertex];
        }

        public override void SetAcceptCost(int vertex, float new_value)
        {
            accept_costs[vertex] = new_value;
        }

        // reading
        public override int nStates()
        {
            return accept_costs.Length();
        }

        public override int GetStart()
        {
            return start;
        }

        public override float GetAcceptCost(int node)
        {
            return accept_costs[node];
        }

        public override void Arcs(Intarray out_inputs, Intarray out_targets, Intarray out_outputs, Floatarray out_costs, int from)
        {
            out_inputs.Copy(m_inputs[from]);
            out_targets.Copy(m_targets[from]);
            out_outputs.Copy(m_outputs[from]);
            out_costs.Copy(m_costs[from]);
        }

        public override void Clear()
        {
            start = 0;
            m_targets.Clear();
            m_inputs.Clear();
            m_outputs.Clear();
            m_costs.Clear();
            accept_costs.Clear();
        }

        // writing
        public override int NewState()
        {
            accept_costs.Push(1e38f);
            m_targets.Push();
            m_inputs.Push();
            m_outputs.Push();
            m_costs.Push();
            int i = accept_costs.Length() - 1;
            CorrectTargetsNull(i);
            CorrectOutputsNull(i);
            CorrectInputsNull(i);
            CorrectCostsNull(i);
            return i;
        }

        public override void AddTransition(int from, int to, int output, float cost, int input)
        {
            m_targets[from].Push(to);
            m_outputs[from].Push(output);
            m_inputs[from].Push(input);
            m_costs[from].Push(cost);
        }

        public override void Rescore(int from, int to, int output, float cost, int input)
        {
            Intarray t = m_targets[from];
            Intarray i = m_inputs[from];
            Intarray o = m_outputs[from];
            for (int j = 0; j < t.Length(); j++)
            {
                if (t[j] == to
                && i[j] == input
                && o[j] == output)
                {
                    m_costs[from][j] = cost;
                    break;
                }
            }
        }

        public override void SetStart(int node)
        {
            start = node;
        }

        public override void SetAccept(int node, float cost = 0.0f)
        {
            accept_costs[node] = cost;
        }

        public override int Special(string s)
        {
            return 0;
        }

        public override double BestPath(out string result)
        {
            return AStarUtil.a_star(out result, this);
        }

        public override void Save(BinaryWriter writer)
        {
            FstIO.fst_write(writer, this);
        }

        public override void Load(BinaryReader reader)
        {
            FstIO.fst_read(this, reader);
        }


        private void achieve(int flag)
        {
            if (!(flag == SORTED_BY_INPUT
                   || flag == SORTED_BY_OUTPUT
                   || flag == HAS_HEURISTICS))
                throw new Exception("CHECK_ARG: flag == SORTED_BY_INPUT || flag == SORTED_BY_OUTPUT || flag == HAS_HEURISTICS");

            if(flags > 0 & flag > 0)
                return;

            if(flag == HAS_HEURISTICS) {
                AStarUtil.a_star_backwards(m_heuristics, this);
                return;
            }

            for(int node = 0; node < nStates(); node++) {
                Intarray permutation = new Intarray();
                if(flag == OcroFST.SORTED_BY_INPUT)
                    NarrayUtil.Quicksort(permutation, m_inputs[node]);
                else
                    NarrayUtil.Quicksort(permutation, m_outputs[node]);
                NarrayUtil.Permute(m_inputs[node], permutation);
                NarrayUtil.Permute(m_outputs[node], permutation);
                NarrayUtil.Permute(m_targets[node], permutation);
                NarrayUtil.Permute(m_costs[node], permutation);
            }
            flags |= flag;
        }

        public override void SortByInput()
        {
            achieve(SORTED_BY_INPUT);
        }

        public override void SortByOutput()
        {
            achieve(SORTED_BY_OUTPUT);
        }

        public override bool HasFlag(int flag)
        {
            return (flags & flag) > 0;
        }

        public override Floatarray Heuristics()
        {
            return m_heuristics;
        }

        public override void CalculateHeuristics()
        {
            achieve(HAS_HEURISTICS);
        }

        public override void ClearFlags()
        {
            flags = 0;
        }
    }
}
