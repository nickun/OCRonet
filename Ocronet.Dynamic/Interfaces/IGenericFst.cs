using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ocronet.Dynamic.Component;

namespace Ocronet.Dynamic.Interfaces
{
    /// <summary>
    /// A generic interface for language models.
    /// 
    /// An IGenericFst is a directed graph
    /// with output/cost/id written on arcs,
    /// accept cost written on vertices and
    /// a fixed start vertice.
    /// </summary>
    public abstract class IGenericFst : IComponent, IDisposable
    {
        public override string Interface
        {
            get { return "IGenericFst"; }
        }

        /// <summary>
        /// Создает экземпляр класса, наследованного от IGenericFst
        /// </summary>
        /// <param name="name">имя класа</param>
        public static IGenericFst MakeFst(string name)
        {
            return ComponentCreator.MakeComponent<IGenericFst>(name);
        }

        /// <summary>
        /// Clear the language model
        /// </summary>
        public abstract void Clear();

        /// <summary>
        /// Get a single new state
        /// </summary>
        /// <returns></returns>
        public abstract int NewState();

        /// <summary>
        /// Add a transition between the given states
        /// </summary>
        public abstract void AddTransition(int from, int to, int output, float cost, int input);

        /// <summary>
        /// A variant of addTransition() with equal input and output.
        /// </summary>
        public virtual void AddTransition(int from, int to, int symbol, float cost)
        {
            AddTransition(from, to, symbol, cost, symbol);
        }

        /// <summary>
        /// Set the start state
        /// </summary>
        public abstract void SetStart(int node);

        /// <summary>
        /// Set a state as an accept state
        /// </summary>
        public abstract void SetAccept(int node, float cost = 0.0f);

        /// <summary>
        /// Obtain codes for "specials" (language model dependent)
        /// </summary>
        public abstract int Special(string s);

        /// <summary>
        /// Compute the best path through the language model.
        /// Useful for simple OCR tasks and for debugging.
        /// </summary>
        public abstract double BestPath(out string result);

        /// <summary>
        /// destroy the language model
        /// </summary>
        public virtual void Dispose() { }

        /// <summary>
        /// simple interface for line recognizers
        /// </summary>
        public virtual void SetString(string text, Floatarray costs, Intarray ids)
        {
            int n = text.Length;
            Intarray states = new Intarray();
            states.Clear();
            for(int i=0; i<n+1; i++)
                states.Push(NewState());
            for(int i=0; i<n; i++)
                AddTransition(states[i], states[i+1], text[i], costs[i], ids[i]);
            SetStart(states[0]);
            SetAccept(states[n]);
        }

        // reading methods

        /// <summary>
        /// Get the number of states.
        /// </summary>
        public virtual int nStates() { throw new NotImplementedException("IGenericFst:nStates: unimplemented"); }

        /// <summary>
        /// Get the starting state.
        /// </summary>
        public virtual int GetStart() { throw new NotImplementedException("IGenericFst:GetStart: unimplemented"); }

        /// <summary>
        /// Get the accept cost of a given vertex (a cost to finish the line and quit).
        /// </summary>
        public virtual float GetAcceptCost(int node) { throw new NotImplementedException("IGenericFst:GetAcceptCost: unimplemented"); }

        /// <summary>
        /// Determine whether the given node is an accepting state.
        /// </summary>
        public virtual bool isAccepting(int node) { return GetAcceptCost(node) < 1e30f; }

        /// <summary>
        /// Return an array of arcs leading from the given node.
        /// WARN_DEPRECATED
        /// </summary>
        public virtual void Arcs(Intarray ids,
                                 Intarray targets,
                                 Intarray outputs,
                                 Floatarray costs,
                                 int from) 
        {
            throw new NotImplementedException("IGenericFst:Arcs: unimplemented");
        }

        /// <summary>
        /// A variant of addTransition() with equal input and output.
        /// </summary>
        public virtual void GetTransitions(Intarray tos, Intarray symbols, Floatarray costs, Intarray inputs, int from)
        {
            Arcs(inputs, tos, symbols, costs, from);
        }

        /// <summary>
        /// Change a transition score between the given states
        /// </summary>
        public virtual void Rescore(int from, int to, int output, float new_cost, int input) 
        {
            throw new NotImplementedException("IGenericFst:Rescore: unimplemented");
        }

        /// <summary>
        /// A variant of rescore() with equal input and output.
        /// </summary>
        public virtual void Rescore(int from, int to, int symbol, float new_cost)
        {
            Rescore(from, to, symbol, new_cost, symbol);
        }

        /*/// <summary>
        /// This method should save in OpenFST format.
        /// (A simple way of doing that is to convert internally to OpenFST,
        /// then call its save method.)
        /// </summary>
        public virtual void Save(string path)
        {
            throw new NotImplementedException("IGenericFst:Save: unimplemented");
        }

        /// <summary>
        /// This method should load in OpenFST format.
        /// (A simple way of doing that is to convert internally to OpenFST,
        /// then call its load method.)
        /// </summary>
        public virtual void Load(string path)
        {
            throw new NotImplementedException("IGenericFst:Load: unimplemented");
        }*/
    }
}
