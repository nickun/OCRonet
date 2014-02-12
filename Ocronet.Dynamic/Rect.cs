using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Ocronet.Dynamic
{
    public struct Rect
    {
        public int x0, y0, x1, y1;

        public static Rect CreateEmpty()
        {
            return new Rect(0, 0, -1, -1);
        }

        public Rect(Rect r)
        {
            x0 = r.x0;
            y0 = r.y0;
            x1 = r.x1;
            y1 = r.y1;
        }

        public Rect(int x0, int y0, int x1, int y1)
        {
            this.x0 = x0;
            this.y0 = y0;
            this.x1 = x1;
            this.y1 = y1;
        }

        public override string ToString()
        {
            return String.Format("x0:{0} y0:{1} x1:{2} y1:{3}", x0, y0, x1, y1);
        }

        public int W
        {
            get { return Width(); }
        }

        public int H
        {
            get { return Height(); }
        }

        static void ASSERT(bool cond, string msg = "")
        {
            if (!cond)
                throw new Exception("ASSERT: " + msg);
        }

        public bool Empty()
        {
            return x0 >= x1 || y0 >= y1;
        }

        /// <summary>
        /// Origin name: pad_by
        /// </summary>
        public void PadBy(int dx, int dy)
        {
            ASSERT(!Empty(), "!Rect.Empty()");
            x0 -= dx;
            y0 -= dy;
            x1 += dx;
            y1 += dy;
        }

        /// <summary>
        /// Origin name: shift_by
        /// </summary>
        public void ShiftBy(int dx, int dy)
        {
            ASSERT(!Empty(), "!Rect.Empty()");
            x0 += dx;
            y0 += dy;
            x1 += dx;
            y1 += dy;
        }

        public int Width()
        {
            int w = x1 - x0;
            return (w < 0) ? 0 : w;
        }

        public int Height()
        {
            int h = y1 - y0;
            return (h < 0) ? 0 : h;
        }

        public void Include(int x, int y)
        {
            if (Empty())
            {
                x0 = x;
                y0 = y;
                x1 = x + 1;
                y1 = y + 1;
            }
            else
            {
                x0 = Math.Min(x, x0);
                y0 = Math.Min(y, y0);
                x1 = Math.Max(x + 1, x1);
                y1 = Math.Max(y + 1, y1);
            }
        }

        public bool Contains(int x, int y)
        {
            return x >= x0 && x < x1 && y >= y0 && y < y1;
        }
        public bool Contains(Point p)
        {
            return p.X >= x0 && p.X < x1 && p.Y >= y0 && p.Y < y1;
        }

        public void Intersect(Rect other)
        {
            if (Empty()) return;
            x0 = Math.Max(x0, other.x0);
            y0 = Math.Max(y0, other.y0);
            x1 = Math.Min(x1, other.x1);
            y1 = Math.Min(y1, other.y1);
        }

        public void Include(Rect other)
        {
            if (Empty())
            {
                x0 = other.x0;
                y0 = other.y0;
                x1 = other.x1;
                y1 = other.y1;
            }
            else
            {
                x0 = Math.Min(x0, other.x0);
                y0 = Math.Min(y0, other.y0);
                x1 = Math.Max(x1, other.x1);
                y1 = Math.Max(y1, other.y1);
            }
        }

        public Rect Intersection(Rect other)
        {
            if (Empty()) return this;
            return new Rect(
                Math.Max(x0, other.x0),
                Math.Max(y0, other.y0),
                Math.Min(x1, other.x1),
                Math.Min(y1, other.y1));
        }

        public Rect Inclusion(Rect other)
        {
            if (Empty()) return other;
            return new Rect(
                Math.Min(x0, other.x0),
                Math.Min(y0, other.y0),
                Math.Max(x1, other.x1),
                Math.Max(y1, other.y1));
        }

        // FIXME see what that should do if empty()
        public Rect Grow(int offset)
        {
            if (Empty()) throw new Exception("grow: rectangle is empty");
            return new Rect(x0 - offset, y0 - offset, x1 + offset, y1 + offset);
        }

        public int Xcenter()
        {
            return (x0 + x1) / 2;
        }
        public int Ycenter()
        {
            return (y0 + y1) / 2;
        }

        static int heaviside(int x)
        {
            return x < 0 ? 0 : x;
        }

        public int Area()
        {
            return heaviside(x1 - x0) * heaviside(y1 - y0);
        }

        public bool Overlaps(Rect other)
        {
            return
                x0 <= other.x1 && x1 >= other.x0 &&
                y0 <= other.y1 && y1 >= other.y0;
        }

        public bool Includes(int x, int y)
        {
            return (x >= x0 && x <= x1 && y >= y0 && y <= y1);
        }

        public bool Includes(float x, float y)
        {
            return (x >= x0 && x <= x1 && y >= y0 && y <= y1);
        }

        public bool Includes(Rect other)
        {
            return Includes(other.x0, other.y0) && Includes(other.x1, other.y1);
        }

        /// <summary>
        /// Origin name: dilated_by
        /// </summary>
        public Rect DilatedBy(int dx0, int dy0, int dx1, int dy1)
        {
            return new Rect(x0 - dx0, y0 - dy0, x1 + dx1, y1 + dy1);
        }

        public float Aspect()
        {
            return (y1 - y0) / (float)(x1 - x0);
        }

        public float Centricity(Rect other)
        {
            float width = x1 - x0;
            float height = y1 - y0;
            return
                (other.x1 - x0) / width *
                (other.y1 - y0) / height *
                (x1 - other.x0) / width *
                (y1 - other.y0) / height;
        }

        /// <summary>
        /// Origin name: fraction_covered_by
        /// </summary>
        public float FractionCoveredBy(Rect other)
        {
            Rect isect = Intersection(other);
            int area = Area();
            if (area > 0)
                return isect.Area() / (float)area;
            else
                return -1;
        }
    }
}
