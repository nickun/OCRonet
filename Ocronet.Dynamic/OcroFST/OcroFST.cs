using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ocronet.Dynamic.Interfaces;
using Ocronet.Dynamic.Component;

namespace Ocronet.Dynamic.OcroFST
{
    public abstract class OcroFST : IGenericFst
    {
        public const int SORTED_BY_INPUT = 1;
        public const int SORTED_BY_OUTPUT = 2;
        public const int HAS_HEURISTICS = 4;

        public abstract Intarray Targets(int vertex);
        public abstract Intarray Inputs(int vertex);
        public abstract Intarray Outputs(int vertex);
        public abstract Floatarray Costs(int vertex);
        public abstract float AcceptCost(int vertex);
        public abstract void SetAcceptCost(int vertex, float new_value);
        public abstract Floatarray Heuristics();

        public abstract bool HasFlag(int flag);
        public abstract void ClearFlags();

        public abstract void SortByInput();
        public abstract void SortByOutput();
        public abstract void CalculateHeuristics();

        /// <summary>
        /// Создает экземпляр класса, наследованного от OcroFST
        /// </summary>
        /// <param name="name">имя класа</param>
        public static new OcroFST MakeFst(string name)
        {
            return ComponentCreator.MakeComponent<OcroFST>(name);
        }

        public static OcroFST MakeOcroFst()
        {
            return ComponentCreator.MakeComponent<OcroFST>("OcroFST");
        }
    }

}
