using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ocronet.Dynamic.OcroFST
{
    public class Heap
    {
        Intarray heap;       // the priority queue
        Intarray heapback;   // heap[heapback[node]] == node; -1 if not in the heap
        Floatarray costs; // the cost of the node on the heap

        /// <summary>
        /// Constructor.
        /// Create a heap storing node indices from 0 to n - 1.
        /// </summary>
        public Heap(int n)
        {
            heap = new Intarray();
            heapback = new Intarray(n);
            heapback.Fill(-1);
            costs = new Floatarray();
        }

        public int Length()
        {
            return heap.Length();
        }

        public int Pop()
        {
            heapswap(0, heap.Length() - 1);
            int result = heap.Pop();
            costs.Pop();
            heapify_down(0);
            heapback[result] = -1;
            return result;
        }

        /// <summary>
        /// Push the node in the heap if it's not already there, otherwise promote.
        /// </summary>
        /// <returns>
        /// True if the heap was changed, false if the item was already 
        /// in the heap and with a better cost.
        /// </returns>
        public bool Push(int node, float cost)
        {
            int i = heapback[node];
            if (i != -1)
            {
                if (cost < costs[i])
                {
                    costs[i] = cost;
                    heapify_up(i);
                    return true;
                }
                return false;
            }
            else
            {
                heap.Push(node);
                costs.Push(cost);
                heapback[node] = heap.Length() - 1;
                heapify_up(heap.Length() - 1);
                return true;
            }
        }

        public float MinCost()
        {
            return costs[0];
        }

        public bool Remove(int node)
        {
            int i = heapback[node];
            if (i == -1)
                return false;

            heapswap(i, heap.Length() - 1);
            heap.Pop();
            costs.Pop();
            heapify_down(i);
            heapback[node] = -1;
            return true;
        }


        int left(int i) { return 2 * i + 1; }
        int right(int i) { return 2 * i + 2; }
        int parent(int i) { return (i - 1) / 2; }

        private int rotate(int i)
        {
            int size = heap.Length();
            int j = left(i);
            int k = right(i);
            if (k < size && costs[k] < costs[j])
            {
                if (costs[k] < costs[i])
                {
                    heapswap(k, i);
                    return k;
                }
            }
            else if (j < size)
            {
                if (costs[j] < costs[i])
                {
                    heapswap(j, i);
                    return j;
                }
            }
            return i;
        }

        /// <summary>
        /// Swap 2 nodes on the heap, maintaining heapback.
        /// </summary>
        private void heapswap(int i, int j)
        {
            int t = heap[i];
            heap[i] = heap[j];
            heap[j] = t;

            float c = costs[i];
            costs[i] = costs[j];
            costs[j] = c;

            heapback[heap[i]] = i;
            heapback[heap[j]] = j;
        }

        /// <summary>
        /// Fix the heap invariant broken by increasing the cost of heap node i.
        /// </summary>
        private void heapify_down(int i)
        {
            while (true)
            {
                int k = rotate(i);
                if (i == k)
                    return;
                i = k;
            }
        }

        /// <summary>
        /// Fix the heap invariant broken by decreasing the cost of heap node i.
        /// </summary>
        private void heapify_up(int i)
        {
            while (i > 0)
            {
                int j = parent(i);
                if (costs[i] < costs[j])
                {
                    heapswap(i, j);
                    i = j;
                }
                else
                {
                    return;
                }
            }
        }
    }
}
