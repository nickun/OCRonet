using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Ocronet.Dynamic.ImgLib;
using Ocronet.Dynamic.Interfaces;

namespace Ocronet.Dynamic.Segmentation.Line
{
    public class CurvedCutSegmenterImpl : ICurvedCutSegmenter
    {
        // input
        Intarray wimage;
        int where;

        // output
        Intarray costs;
        Intarray sources;
        int direction;
        int limit;

        public Intarray bestcuts;

        public string debug;
        Intarray dimage;

        public Narray<Narray<Point>> cuts;
        Floatarray cutcosts;

        public CurvedCutSegmenterImpl()
        {
            wimage = new Intarray();
            costs = new Intarray();
            sources = new Intarray();
            bestcuts = new Intarray();
            dimage = new Intarray();
            cuts = new Narray<Narray<Point>>();
            cutcosts = new Floatarray();
            //params_for_chars();
            params_for_lines();
            //params_from_hwrec_c();
        }

        protected override void params_for_lines()
        {
            down_cost = 0;
            outside_diagonal_cost = 4;
            inside_diagonal_cost = 4;
            boundary_diagonal_cost = 0;
            outside_weight = 0;
            boundary_weight = -1;
            inside_weight = 4;
            min_range = 3;
            min_thresh = 10.0f;
        }

        protected void Step(int x0, int x1, int y)
        {
            int w = wimage.Dim(0), h = wimage.Dim(1);
            Queue<Point> queue = new Queue<Point>(w*h);
            for(int i=x0; i<x1; i++) queue.Enqueue(new Point(i, y));
            int low = 1;
            int high = wimage.Dim(0)-1;

            while(queue.Count > 0) {
                Point p = queue.Dequeue();
                int i = p.X, j = p.Y;
                int cost = costs[i, j];
                int ncost = cost + wimage[i, j] + down_cost;
                if(costs[i, j+direction] > ncost) {
                    costs[i, j+direction] = ncost;
                    sources[i, j+direction] = i;
                    if(j+direction != limit) queue.Enqueue(new Point(i, j+direction));
                }
                if(i > low) {
                    if(wimage[i, j] == 0)
                        ncost = cost + wimage[i, j] + outside_diagonal_cost;
                    else if(wimage[i, j] > 0)
                        ncost = cost + wimage[i, j] + inside_diagonal_cost;
                    else if(wimage[i, j] < 0)
                        ncost = cost + wimage[i,j] + boundary_diagonal_cost;
                    if(costs[i-1, j+direction]>ncost) {
                        costs[i-1, j+direction] = ncost;
                        sources[i-1, j+direction] = i;
                        if(j+direction != limit) queue.Enqueue(new Point(i-1, j+direction));
                    }
                }
                if(i < high) {
                    if(wimage[i, j] == 0)
                        ncost = cost + wimage[i, j] + outside_diagonal_cost;
                    else if(wimage[i, j] > 0)
                        ncost = cost + wimage[i, j] + inside_diagonal_cost;
                    else if(wimage[i, j] < 0)
                        ncost = cost + wimage[i, j] + boundary_diagonal_cost;
                    if(costs[i+1, j+direction] > ncost) {
                        costs[i+1, j+direction] = ncost;
                        sources[i+1, j+direction] = i;
                        if(j+direction != limit) queue.Enqueue(new Point(i+1, j+direction));
                    }
                }
            }
        }

        public override void FindAllCuts()
        {
            int w = wimage.Dim(0), h = wimage.Dim(1);
            // initialize dimensions of cuts, costs etc
            cuts.Resize(w);
            cutcosts.Resize(w);
            costs.Resize(w, h);
            sources.Resize(w, h);

            costs.Fill(1000000000);
            for (int i = 0; i < w; i++) costs[i, 0] = 0;
            sources.Fill(-1);
            limit = where;
            direction = 1;
            Step(0, w, 0);

            for (int x = 0; x < w; x++)
            {
                cutcosts[x] = costs[x, where];
                cuts[x] = new Narray<Point>();
                cuts[x].Clear();
                // bottom should probably be initialized with 2*where instead of
                // h, because where cannot be assumed to be h/2. In the most extreme
                // case, the cut could go through 2 pixels in each row
                Narray<Point> bottom = new Narray<Point>();
                int i = x, j = where;
                while (j >= 0)
                {
                    bottom.Push(new Point(i, j));
                    i = sources[i, j];
                    j--;
                }
                //cuts(x).resize(h);
                for (i = bottom.Length() - 1; i >= 0; i--) cuts[x].Push(bottom[i]);
            }

            costs.Fill(1000000000);
            for (int i = 0; i < w; i++) costs[i, h - 1] = 0;
            sources.Fill(-1);
            limit = where;
            direction = -1;
            Step(0, w, h - 1);

            for (int x = 0; x < w; x++)
            {
                cutcosts[x] += costs[x, where];
                // top should probably be initialized with 2*(h-where) instead of
                // h, because where cannot be assumed to be h/2. In the most extreme
                // case, the cut could go through 2 pixels in each row
                Narray<Point> top = new Narray<Point>();
                int i = x, j = where;
                while (j < h)
                {
                    if (j > where) top.Push(new Point(i, j));
                    i = sources[i, j];
                    j++;
                }
                for (i = 0; i < top.Length(); i++) cuts[x].Push(top[i]);
            }

            // add costs for line "where"
            for (int x = 0; x < w; x++)
            {
                cutcosts[x] += wimage[x, where];
            }
        }

        public override void FindBestCuts()
        {
            unchecked
            {
                for (int i = 0; i < cutcosts.Length(); i++) NarrayUtil.ExtPut(dimage, i, (int)(cutcosts[i] + 10), 0xff0000);
                for (int i = 0; i < cutcosts.Length(); i++) NarrayUtil.ExtPut(dimage, i, (int)(min_thresh + 10), 0x800000);
            }
            Floatarray temp = new Floatarray();
            Gauss.Gauss1d(temp, cutcosts, 3.0f);
            cutcosts.Move(temp);
            SegmRoutine.local_minima(ref bestcuts, cutcosts, min_range, min_thresh);
            for(int i=0; i<bestcuts.Length(); i++) {
                Narray<Point> cut = cuts[bestcuts[i]];
                for(int j=0; j<cut.Length(); j++) {
                    Point p = cut[j];
                    NarrayUtil.ExtPut(dimage, p.X, p.Y, 0x00ff00);
                }
            }
            ///-if(debug.Length > 0) write_image_packed(debug, dimage);
            // dshow1d(cutcosts,"Y");
            //dshow(dimage,"Y");
        }

        public override void SetImage(Bytearray image)
        {
            dimage.Copy(image);
            int w = image.Dim(0), h = image.Dim(1);
            wimage.Resize(w, h);
            wimage.Fill(0);
            float s1 = 0.0f, sy = 0.0f;
            for(int i=1; i<w; i++) for(int j=0; j<h; j++) {
                    if(image[i,j] > 0) { s1++; sy += j; }
                    if(image[i-1,j]==0 && image[i,j]>0) wimage[i,j] = boundary_weight;
                    else if(image[i,j]>0) wimage[i,j] = inside_weight;
                    else wimage[i,j] = outside_weight;
                }
            where = (int)(sy/s1);
            for(int i=0;i<dimage.Dim(0);i++) dimage[i, where] = 0x008000;
        }
    }
}
