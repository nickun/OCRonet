using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Ocronet.Dynamic.ImgLib
{
    public abstract class Metric
    {
        public abstract float metric(Point p, Point q);

        protected int abs_(int x) { return (x < 0) ? -x : x; }
        protected int max_(int x, int y) { return (x < y) ? y : x; }
        protected float abs_(float x) { return (x < 0) ? -x : x; }
        protected float max_(float x, float y) { return (x < y) ? y : x; }
    }

    public class Metric1 : Metric
    {
        protected static Metric _metric;

        public static Metric Default
        {
            get { if (_metric == null) _metric = new Metric1(); return _metric; }
        }

        public override float metric(Point p, Point q)
        {
            int dx = p.X - q.X;
            int dy = p.Y - q.Y;
            return abs_(dx) + abs_(dy);
        }
    }

    public class Metric2 : Metric
    {
        protected static Metric _metric;

        public static Metric Default
        {
            get { if (_metric == null) _metric = new Metric2(); return _metric; }
        }

        public override float metric(Point p, Point q)
        {
            int dx = p.X - q.X;
            int dy = p.Y - q.Y;
            return dx * dx + dy * dy;
        }
    }

    public class MetricInf : Metric
    {
        protected static Metric _metric;

        public static Metric Default
        {
            get { if (_metric == null) _metric = new MetricInf(); return _metric; }
        }

        public override float metric(Point p, Point q)
        {
            int dx = abs_(p.X - q.X);
            int dy = abs_(p.Y - q.Y);
            return max_(dx, dy);
        }
    }
}
