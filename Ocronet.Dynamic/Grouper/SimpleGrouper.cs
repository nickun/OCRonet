using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Ocronet.Dynamic.Interfaces;
using Ocronet.Dynamic.Segmentation;
using Ocronet.Dynamic.ImgLib;
using Ocronet.Dynamic.Utils;

namespace Ocronet.Dynamic.Grouper
{
    public class SimpleGrouper : IGrouper
    {
        public int maxrange;
        public int maxdist;
        public Intarray labels;
        public Narray<Rect> boxes;
        public Narray<Intarray> segments; // objlist<Intarray>
        public Narray<Rect> rboxes;
        public Narray<Narray<string>> class_outputs;
        public Narray<Floatarray> class_costs;
        public Floatarray spaces;
        public bool fullheight;
        string gttranscript;
        Narray<Intarray> gtsegments;

        public SimpleGrouper()
        {
            PDef("maxrange", 4, "maximum range");
            PDef("maxdist", 2, "maximum dist");
            PDef("fullheight", false, "fullheight");
            labels = new Intarray();
            boxes = new Narray<Rect>();
            segments = new Narray<Intarray>();
            rboxes = new Narray<Rect>();
            class_outputs = new Narray<Narray<string>>();
            class_costs = new Narray<Floatarray>();
            spaces = new Floatarray();

            gttranscript = "";
            gtsegments = new Narray<Intarray>();
        }

        public override string Description
        {
            get { return "SimpleGrouper"; }
        }

        public override string Name
        {
            get { return "simplegrouper"; }
        }

        /// <summary>
        /// Set a segmentation.
        /// </summary>
        public override void SetSegmentation(Intarray segmentation)
        {
            maxrange = PGeti("maxrange");
            maxdist = PGeti("maxdist");
            fullheight = PGetb("fullheight");
            labels.Copy(segmentation);
            SegmRoutine.make_line_segmentation_black(labels);
            GrouperRoutine.check_approximately_sorted(labels);
            boxes.Dealloc();
            segments.Dealloc();
            class_outputs.Dealloc();
            class_costs.Dealloc();
            spaces.Dealloc();
            computeGroups();
        }

        /// <summary>
        /// Set a character segmentation.
        /// </summary>
        public override void SetCSegmentation(Intarray segmentation)
        {
            maxrange = 1;
            maxdist = 2;
            labels.Copy(segmentation);
            SegmRoutine.make_line_segmentation_black(labels);
            GrouperRoutine.check_approximately_sorted(labels);
            boxes.Dealloc();
            segments.Dealloc();
            class_outputs.Dealloc();
            class_costs.Dealloc();
            spaces.Dealloc();
            computeGroups();
        }

        /// <summary>
        /// Compute the groups for a segmentation (internal method).
        /// </summary>
        private void computeGroups()
        {
            rboxes.Clear();
            ImgLabels.bounding_boxes(ref rboxes, labels);
            int n = rboxes.Length();
            // NB: we start with i=1 because i=0 is the background
            for (int i = 1; i < n; i++)
            {
                for (int range = 1; range <= maxrange; range++)
                {
                    if (i + range > n) continue;
                    Rect box = rboxes.At1d(i);
                    Intarray seg = new Intarray();
                    bool bad = false;
                    for (int j = i; j < i + range; j++)
                    {
                        if (j > i && rboxes.At1d(j).x0 - rboxes.At1d(j - 1).x1 > maxdist)
                        {
                            bad = true;
                            break;
                        }
                        box.Include(rboxes.At1d(j));
                        seg.Push(j);
                    }
                    if (bad) continue;
                    boxes.Push(box);
                    segments.Push(seg);
                }
            }
        }

        /// <summary>
        /// Return the number of character candidates found.
        /// </summary>
        public override int Length()
        {
            return boxes.Length();
        }

        /// <summary>
        /// Return the bounding box for a character.
        /// </summary>
        public override Rect BoundingBox(int index)
        {
            return boxes.At1d(index);
        }

        /// <summary>
        /// Return the starting segment
        /// </summary>
        public override int Start(int index)
        {
            return NarrayUtil.Min(segments[index]);
        }

        /// <summary>
        /// Return the last segment
        /// </summary>
        public override int End(int index)
        {
            return NarrayUtil.Max(segments[index]);
        }

        /// <summary>
        /// Return a list of all segments
        /// </summary>
        public override void GetSegments(Intarray result, int index)
        {
            result.Copy(segments[index]);
        }

        /// <summary>
        /// Return the segmentation-derived mask for the character.
        /// This may optionally be grown by some pixels.
        /// </summary>
        public override void GetMask(out Rect r, ref Bytearray outmask, int index, int grow)
        {
            r = boxes.At1d(index).Grow(grow);
            r.Intersect(new Rect(0, 0, labels.Dim(0), labels.Dim(1)));
            if (fullheight)
            {
                r.y0 = 0;
                r.y1 = labels.Dim(1);
            }
            int x = r.x0, y = r.y0, w = r.Width(), h = r.Height();
            Intarray segs = segments.At1d(index);
            outmask.Resize(w, h);
            outmask.Fill(0);
            for (int i = 0; i < w; i++)
                for (int j = 0; j < h; j++)
                {
                    int label = labels[x + i, y + j];
                    if (NarrayUtil.first_index_of(segs, label) >= 0)
                    {
                        outmask[i, j] = (byte)255;
                    }
                }
            if (grow > 0)
                Morph.binary_dilate_circle(outmask, grow);
        }

        /// <summary>
        /// Get a mask at a given location
        /// </summary>
        public override void GetMaskAt(ref Bytearray mask, int index, Rect b)
        {
            if (!(b.x0 > -1000 && b.x1 < 10000 && b.y0 > -1000 && b.y1 < 10000))
                throw new Exception("CHECK: (b.Left > -1000 && b.Right < 10000 && b.Top > -1000 && b.Bottom < 10000)");
            mask.Resize(b.Width(), b.Height());
            mask.Fill(0);
            Intarray segs = segments.At1d(index);
            int w = b.Width(), h = b.Height();
            for (int i = 0; i < w; i++)
            {
                int x = b.x0 + i;
                unchecked
                {
                    if ((uint)x >= labels.Dim(0)) continue;
                }
                for (int j = 0; j < h; j++)
                {
                    int y = b.y0 + j;
                    unchecked
                    {
                        if (((uint)y) >= labels.Dim(1)) continue;
                    }
                    int label = labels[b.x0 + i, b.y0 + j];
                    if (NarrayUtil.first_index_of(segs, label) >= 0)
                    {
                        mask[i, j] = (byte)255;
                    }
                }
            }
        }

        /// <summary>
        /// Extract the masked character from the source image/source
        /// feature map.
        /// </summary>
        void extractMasked<T>(Narray<T> outa, Bytearray outmask, Narray<T> source, int index, int grow = 0)
        {
            if (!labels.SameDims(source))
                throw new Exception("ASSERT: labels.SameDims(source)");
            Rect r;
            GetMask(out r, ref outmask, index, grow);
            int x = r.x0, y = r.y0, w = r.Width(), h = r.Height();
            outa.Resize(w, h);
            outa.Fill(0);
            for (int i = 0; i < w; i++) for (int j = 0; j < h; j++)
                {
                    if (outmask[i, j] > 0)
                        outa[i, j] = source[i + x, j + y];
                }
        }

        /// <summary>
        /// Extract the character by bounding rectangle (no masking).
        /// </summary>
        void extractWithBackground<T>(Narray<T> outa, Narray<T> source, T dflt, int index, int grow = 0)
        {
            if (!labels.SameDims(source))
                throw new Exception("ASSERT: labels.SameDims(source)");
            Bytearray mask = new Bytearray();
            Rect r;
            GetMask(out r, ref mask, index, grow);
            int x = r.x0, y = r.y0, w = r.Width(), h = r.Height();
            outa.Resize(w, h);
            outa.Fill(dflt);
            for (int i = 0; i < w; i++) for (int j = 0; j < h; j++)
                {
                    if (mask[i, j] > 0)
                        outa[i, j] = source[i + x, j + y];
                }
        }

        // Overloaded convenience functions.

        public override void Extract(Bytearray outa, Bytearray source, byte dflt, int index, int grow = 0)
        {
            extractWithBackground(outa, source, dflt, index, grow);
        }

        public override void Extract(Floatarray outa, Floatarray source, float dflt, int index, int grow = 0)
        {
            extractWithBackground(outa, source, dflt, index, grow);
        }

        public override void ExtractWithMask(Bytearray outa, Bytearray mask, Bytearray source, int index, int grow = 0)
        {
            extractMasked(outa, mask, source, index, grow);
        }

        public override void ExtractWithMask(Floatarray outa, Bytearray mask, Floatarray source, int index, int grow = 0)
        {
            extractMasked(outa, mask, source, index, grow);
        }

        /// <summary>
        /// Extract the masked character from the source image/source
        /// feature map.
        /// </summary>
        void extractSlicedMasked<T>(Narray<T> outa, Bytearray outmask, Narray<T> source, int index, int grow = 0)
        {
            if (!labels.SameDims(source))
                throw new Exception("ASSERT: labels.SameDims(source)");
            Rect r;
            GetMask(out r, ref outmask, index, grow);
            int x = r.x0, y = r.y0, w = r.Width(), h = r.Height();
            outa.Resize(w, source.Dim(1));
            outa.Fill(0);
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    if (outmask[i, j] > 0)
                        outa[i, j + y] = source[i + x, j + y];
                }
            }
        }

        /// <summary>
        /// Extract the character by bounding rectangle (no masking).
        /// </summary>
        void extractSlicedWithBackground<T>(Narray<T> outa, Narray<T> source, T dflt, int index, int grow = 0)
        {
            if (!labels.SameDims(source))
                throw new Exception("ASSERT: labels.SameDims(source)");
            Bytearray mask = new Bytearray();
            Rect r;
            GetMask(out r, ref mask, index, grow);
            int x = r.x0, y = r.y0, w = r.Width(), h = r.Height();
            outa.Resize(w, source.Dim(1));
            outa.Fill(dflt);
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    if (mask[i, j] > 0)
                        outa[i, j + y] = source[i + x, j + y];
                }
            }
        }

        // slice extraction
        public override void ExtractSliced(Bytearray outa, Bytearray mask, Bytearray source, int index, int grow = 0)
        {
            extractSlicedMasked(outa, mask, source, index, grow);
        }

        public override void ExtractSliced(Bytearray outa, Bytearray source, byte dflt, int index, int grow = 0)
        {
            extractSlicedWithBackground(outa, source, dflt, index, grow);
        }

        public override void ExtractSliced(Floatarray outa, Bytearray mask, Floatarray source, int index, int grow = 0)
        {
            extractSlicedMasked(outa, mask, source, index, grow);
        }

        public override void ExtractSliced(Floatarray outa, Floatarray source, float dflt, int index, int grow = 0)
        {
            extractSlicedWithBackground(outa, source, dflt, index, grow);
        }

        public override void ClearLattice()
        {

            class_costs.Dealloc();
            class_costs.Resize(boxes.Length());
            class_outputs.Dealloc();
            class_outputs.Resize(boxes.Length());
            spaces.Resize(boxes.Length(), 2);
            spaces.Fill(float.PositiveInfinity);
        }

        void maybeInit()
        {
            if (class_costs.Length1d() == 0)
            {
                ClearLattice();
            }
        }

        /// <summary>
        /// After classification, set the class for the given character.
        /// </summary>
        public override void SetClass(int index, string cls, float cost)
        {
            maybeInit();
            if (class_outputs[index] == null)
                class_outputs[index] = new Narray<string>();
            if (class_costs[index] == null)
                class_costs[index] = new Floatarray();
            class_outputs[index].Push(cls);
            class_costs[index].Push(cost);
        }

        /// <summary>
        /// Get spacing to the next component.
        /// </summary>
        public override int PixelSpace(int i)
        {
            int end = NarrayUtil.Max(segments.At1d(i));
            if (end >= rboxes.Length() - 1) return -1;
            int x0 = rboxes.At1d(end).x1;
            int x1 = rboxes.At1d(end + 1).x0;
            return x1 - x0;
        }

        /// <summary>
        /// Set the cost for inserting a space after the given
        /// character.
        /// </summary>
        public override void SetSpaceCost(int index, float yes, float no)
        {
            maybeInit();
            spaces[index, 0] = yes;
            spaces[index, 1] = no;
        }

        /// <summary>
        /// Output the segmentation into a segmentation graph.
        /// Construct a state for each of the segments, then
        /// add transitions between states (segments)
        /// from min(segments[i]) to max(segments[i])+1.
        /// </summary>
        public override void GetLattice(IGenericFst fst)
        {
            fst.Clear();

            int final = NarrayUtil.Max(labels) + 1;
            Intarray states = new Intarray(final + 1);

            states.Fill(-1);
            for (int i = 1; i < states.Length(); i++)
                states[i] = fst.NewState();
            fst.SetStart(states[1]);
            fst.SetAccept(states[final]);

            for (int i = 0; i < boxes.Length(); i++)
            {
                int start = NarrayUtil.Min(segments.At1d(i));
                int end = NarrayUtil.Max(segments.At1d(i));
                int id = (start << 16) + end;
                if (segments.At1d(i).Length() == 0)
                    id = 0;

                float yes = spaces[i, 0];
                float no = spaces[i, 1];
                // if no space is set, assume no space is present
                if (yes == float.PositiveInfinity && no == float.PositiveInfinity)
                    no = 0.0f;

                for (int j = 0; j < class_costs[i].Length(); j++)
                {
                    float cost = class_costs[i][j];
                    string str = class_outputs[i][j];
                    int n = str.Length;
                    int last = start;
                    for (int k = 0; k < n; k++)
                    {
                        int c = (int)str[k];
                        if (k < n - 1)
                        {
                            // add intermediate states/transitions for all but the last character
                            states.Push(fst.NewState());
                            fst.AddTransition(states[last], states.Last(), c, 0.0f, 0);
                            last = states.Length() - 1;
                        }
                        else
                        {
                            // for the last character, handle the spaces as well
                            if (no < 1000.0f)
                            {
                                // add the last character as a direct transition with no space
                                fst.AddTransition(states[last], states[end + 1], c, cost + no, id);
                            }
                            if (yes < 1000.0f)
                            {
                                // insert another state to handle spaces
                                states.Push(fst.NewState());
                                int space_state = states.Last();
                                fst.AddTransition(states[start], space_state, c, cost, id);
                                fst.AddTransition(space_state, states[end + 1], (int)' ', yes, 0);
                            }
                        }
                    } // for k
                } // for j
            } // for i
        }


        // This is all the code for dealing with ground truth segmentations
        // and correspondences.

        void remove_spaces(ref string p)
        {
            p = p.Replace(" ", "");
        }

        void chomp(ref string p)
        {
            int i = p.IndexOf("\n");
            if (i > 0)
            {
                p = p.Substring(0, i);
            }
        }

        void fixup_transcript(ref string nutranscript, bool old_csegs)
        {
            chomp(ref nutranscript);
            if (old_csegs) remove_spaces(ref nutranscript);
        }


        public override void SetSegmentationAndGt(Intarray segmentation, Intarray cseg, ref string text)
        {
            // first, set the segmentation as usual
            SetSegmentation(segmentation);

            // Maybe fix up the transcript (remove spaces).
            gttranscript = text;
            string s = text;
            fixup_transcript(ref s, false);
            int max_cseg = NarrayUtil.Max(cseg);
            bool old_csegs = (s.Length != max_cseg);
            fixup_transcript(ref gttranscript, old_csegs);

            // Complain if it doesn't match.
            if (gttranscript.Length != max_cseg)
            {
                Logger.Default.Format("transcript = '{0}'\n", gttranscript);
                throw new Exception(String.Format("transcript doesn't agree with cseg (transcript {0}, cseg {1})",
                       gttranscript.Length, max_cseg));
            }

            // Now compute the correspondences between the character segmentation
            // and the raw segmentation.
            GrouperRoutine.segmentation_correspondences(gtsegments, segmentation, cseg);
        }

        public override int GetGtIndex(int index)
        {
            Intarray segs = new Intarray();
            GetSegments(segs, index);

            // see whether this is a ground truth segment
            int match = -1;
            for (int j = 0; j < gtsegments.Length(); j++)
            {
                if (GrouperRoutine.Equals(gtsegments[j], segs))
                {
                    match = j;
                    break;
                }
            }
            return match;       // this returns the color in the cseg
        }

        public override int GetGtClass(int index)
        {
            int match = GetGtIndex(index);

            // if it's not a ground truth segment, return -1
            if (match < 0) return -1;

            // otherwise, look up the character
            match -= 1;
            if (match < 0 && match >= gttranscript.Length)
                throw new Exception("transcript / cseg mismatch");
            return (int)gttranscript[match];
        }
    }
}
