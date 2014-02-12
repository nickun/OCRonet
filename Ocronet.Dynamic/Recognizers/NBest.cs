using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ocronet.Dynamic.Recognizers
{
    public class NBest
    {
        public int n;
        public int fill;
        public List<int> ids;
        public List<double> values;

        public NBest(int n)
        {
            this.n = n;
            ids = new List<int>();
            values = new List<double>();
            Clear();
        }

        /// <summary>
        /// remove all elements
        /// </summary>
        public void Clear()
        {
            fill = 0;
            for (int i = 0; i <= n; i++) ids[i] = -1;
            for (int i = 0; i <= n; i++) values[i] = -1e38;
        }

        /// <summary>
        /// add the id with the corresponding value
        /// </summary>
        public bool Add(int id, double value)
        {
            if (fill == n)
            {
                int i = n - 1;
                if (values[i] >= value) return false;
                while (i > 0)
                {
                    if (values[i - 1] >= value) break;
                    values[i] = values[i - 1];
                    ids[i] = ids[i - 1];
                    i--;
                }
                values[i] = value;
                ids[i] = id;
            }
            else if (fill == 0)
            {
                values[0] = value;
                ids[0] = id;
                fill++;
            }
            else
            {
                int i = fill;
                while (i > 0)
                {
                    if (values[i - 1] >= value) break;
                    values[i] = values[i - 1];
                    ids[i] = ids[i - 1];
                    i--;
                }
                values[i] = value;
                ids[i] = id;
                fill++;
            }
            return true;
        }

        /// <summary>
        /// get the value corresponding to rank i
        /// </summary>
        public double Value(int i)
        {
            if (Math.Abs(i) >= Math.Abs(n))
                throw new Exception("NBest.Value(i): range error");
            return values[i];
        }

        /// <summary>
        /// get the id corresponding to rank i
        /// </summary>
        public int this[int i]
        {
            get 
            {
                if (Math.Abs(i) >= Math.Abs(n))
                    throw new Exception("NBest[i]: range error");
                return ids[i];
            }
        }

        /// <summary>
        /// get the number of elements in the NBest structure (between 0 and n)
        /// </summary>
        public int Length()
        {
            return fill;
        }

    }
}
