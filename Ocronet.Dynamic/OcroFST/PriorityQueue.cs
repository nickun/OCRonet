using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ocronet.Dynamic.OcroFST
{
    public class PriorityQueue
    {
        int n;
        int fill;
        Intarray ids;
        Intarray tags;
        Floatarray values;

        /// <summary>
        /// constructor for a NBest data structure of size n
        /// </summary>
        public PriorityQueue(int n)
        {
            this.n = n;
            ids = new Intarray();
            tags = new Intarray();
            values = new Floatarray();
            ids.Resize(n + 1);
            tags.Resize(n + 1);
            values.Resize(n + 1);
            Clear();
        }

        /// <summary>
        /// remove all elements
        /// </summary>
        public void Clear()
        {
            fill = 0;
        }

        /// <summary>
        /// Origin name: move_value
        /// </summary>
        public void MoveValue(int id, int tag, float value, int start, int end)
        {
            int i = start;
            while (i > end)
            {
                if (values[i - 1] >= value) break;
                values[i] = values[i - 1];
                tags[i] = tags[i - 1];
                ids[i] = ids[i - 1];
                i--;
            }
            values[i] = value;
            tags[i] = tag;
            ids[i] = id;
        }

        /// <summary>
        /// Add the id with the corresponding value
        /// </summary>
        /// <returns>True if the queue was changed</returns>
        public bool Add(int id, int tag, float value)
        {
            if (fill == n)
            {
                if (values[n - 1] >= value) return false;
                MoveValue(id, tag, value, n - 1, 0);
            }
            else if (fill == 0)
            {
                values[0] = value;
                ids[0] = id;
                tags[0] = tag;
                fill++;
            }
            else
            {
                MoveValue(id, tag, value, fill, 0);
                fill++;
            }
            return true;
        }

        /// <summary>
        /// Origin name: find_id
        /// </summary>
        public int FindId(int id)
        {
            for (int i = 0; i < fill; i++)
            {
                if (ids[i] == id)
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// This function will move the existing id up
        /// instead of creating a new one.
        /// Origin name: add_replacing_id
        /// </summary>
        /// <returns>True if the queue was changed</returns>
        public bool AddReplacingId(int id, int tag, float value)
        {
            int former = FindId(id);
            if (former == -1)
                return Add(id, tag, value);
            if (values[former] >= value)
                return false;
            MoveValue(id, tag, value, former, 0);
            return true;
        }

        /// <summary>
        /// get the value corresponding to rank i
        /// </summary>
        public float Value(int i)
        {
            if (Math.Abs(i) >= Math.Abs(fill))
                throw new Exception("range error");
            return values[i];
        }

        public int Tag(int i)
        {
            if (Math.Abs(i) >= Math.Abs(fill))
                throw new Exception("range error");
            return tags[i];
        }

        /// <summary>
        /// get the number of elements in the NBest structure (between 0 and n)
        /// </summary>
        public int Length()
        {
            return fill;
        }

        /// <summary>
        /// get the id corresponding to rank i
        /// </summary>
        public int this[int index]
        {
            get 
            {
                if (Math.Abs(index) >= Math.Abs(fill))
                    throw new Exception("range error");
                return ids[index];
            }
            set
            {
                if (Math.Abs(index) >= Math.Abs(fill))
                    throw new Exception("range error");
                ids[index] = value;
            }
        }

    }
}
