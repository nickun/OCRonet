using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ocronet.Dynamic.Interfaces;

namespace Ocronet.Dynamic.OcroFST
{
    public static class AStarUtil
    {
        internal static bool a_star2_internal(Intarray inputs,
                          Intarray vertices1,
                          Intarray vertices2,
                          Intarray outputs,
                          Floatarray costs,
                          IGenericFst fst1,
                          IGenericFst fst2,
                          Floatarray g1,
                          Floatarray g2,
                          CompositionFst composition)
        {
            Intarray vertices = new Intarray();
            AStarCompositionSearch a = new AStarCompositionSearch(g1, g2, composition);
            if (!a.Loop())
                return false;
            if (!a.reconstruct_vertices(vertices))
                return false;
            a.reconstruct_edges(inputs, outputs, costs, vertices);
            composition.SplitIndices(vertices1, vertices2, vertices);
            return true;
        }

        public static bool a_star(Intarray inputs,
                Intarray vertices,
                Intarray outputs,
                Floatarray costs,
                OcroFST fst)
        {
            AStarSearch a = new AStarSearch(fst);
            if (!a.Loop())
                return false;
            if (!a.reconstruct_vertices(vertices))
                return false;
            a.reconstruct_edges(inputs, outputs, costs, vertices);
            return true;
        }

        public static double a_star(out string result, OcroFST fst)
        {
            result = "";
            Intarray inputs = new Intarray();
            Intarray vertices = new Intarray();
            Intarray outputs = new Intarray();
            Floatarray costs = new Floatarray();
            if (!a_star(inputs, vertices, outputs, costs, fst))
                return 1e38;
            FstUtil.remove_epsilons(out result, outputs);
            return NarrayUtil.Sum(costs);
        }

        public static double a_star(out string result, OcroFST fst1, OcroFST fst2)
        {
            result = "";
            Intarray inputs = new Intarray();
            Intarray v1 = new Intarray();
            Intarray v2 = new Intarray();
            Intarray outputs = new Intarray();
            Floatarray costs = new Floatarray();
            if (!a_star_in_composition(inputs, v1, v2, outputs, costs, fst1, fst2))
                return 1e38;
            FstUtil.remove_epsilons(out result, outputs);
            return NarrayUtil.Sum(costs);
        }



        public static bool a_star_in_composition(Intarray inputs,
                               Intarray vertices1,
                               Intarray vertices2,
                               Intarray outputs,
                               Floatarray costs,
                               OcroFST fst1,
                               OcroFST fst2)
        {
            CompositionFst composition = FstFactory.MakeCompositionFst(fst1, fst2);
            bool result;
            try
            {
                //Floatarray g1 = new Floatarray();
                //Floatarray g2 = new Floatarray();
                fst1.CalculateHeuristics();
                fst2.CalculateHeuristics();
                result = a_star2_internal(inputs, vertices1, vertices2, outputs,
                                          costs, fst1, fst2,
                                          fst1.Heuristics(),
                                          fst2.Heuristics(), composition);
            }
            catch (Exception ex)
            {
                composition.Move1();
                composition.Move2();
                throw ex;
            }
            composition.Move1();
            composition.Move2();
            return result;
        }



        public static void a_star_backwards(Floatarray costs_for_all_nodes, IGenericFst fst)
        {
            IGenericFst reverse = FstFactory.MakeOcroFST();
            FstUtil.fst_copy_reverse(reverse, fst, true); // creates an extra vertex
            AStarSearch a = new AStarSearch(reverse);
            a.Loop();
            costs_for_all_nodes.Copy(a.g);
            costs_for_all_nodes.Pop(); // remove the extra vertex
        }


        public static bool a_star_in_composition(Intarray inputs,
                               Intarray vertices1,
                               Intarray vertices2,
                               Intarray outputs,
                               Floatarray costs,
                               OcroFST fst1,
                               Floatarray g1,
                               OcroFST fst2,
                               Floatarray g2)
        {
            CompositionFst composition = FstFactory.MakeCompositionFst(fst1, fst2);
            bool result;
            try
            {
                result = a_star2_internal(inputs, vertices1, vertices2, outputs,
                                          costs, fst1, fst2,
                                          g1,
                                          g2, composition);
            }
            catch (Exception ex)
            {
                composition.Move1();
                composition.Move2();
                throw ex;
            }
            composition.Move1();
            composition.Move2();
            return result;
        }

    }
}
