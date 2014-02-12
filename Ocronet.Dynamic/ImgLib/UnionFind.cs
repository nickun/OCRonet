using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ocronet.Dynamic.ImgLib
{
    public class UnionFind
    {
        public Narray<int> p, rank;

        public UnionFind(int max = 10000)
        {
            p = new Narray<int>();
            p.Resize(max);
            p.Fill(-1);
            rank = new Narray<int>();
            rank.Resize(max);
            rank.Fill(-1);
        }

        public void make_set(int x)
        {
            if (x < 0)
                throw new Exception("UnionFind::make_set: range error");
            p[x] = x;
            rank[x] = 0;
        }

        public void make_union(int x, int y)
        {
            if (x == y) return;
            link(find_set(x), find_set(y));
        }

        public void link(int x, int y)
        {
            if (rank[x] > rank[y])
            {
                p[y] = x;
            }
            else
            {
                p[x] = y;
                if (rank[x] == rank[y]) rank[y]++;
            }
        }

        public int find_set(int x)
        {
            if (x < 0)
                throw new Exception("UnionFind::find_set: range error");
            if (p[x] < 0)
                throw new Exception("UnionFind::find_set: trying to find a set that hasn't been created yet");
            if (x != p[x]) p[x] = find_set(p[x]);
            return p[x];
        }
    }
}
