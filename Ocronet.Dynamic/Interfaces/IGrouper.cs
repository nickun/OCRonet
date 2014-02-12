using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Ocronet.Dynamic.Interfaces
{
    public abstract class IGrouper : IComponent
    {
        public override string Interface
        {
            get { return "IGrouper"; }
        }

        /// <summary>
        /// Set the grouper for iterating over the elements of the
        /// segmentation.
        /// </summary>
        public abstract void SetSegmentation(Intarray segmentation);


        /// <summary>
        /// Set the grouper for iterating over a pre-segmented image (i.e.,
        /// one group per input segment).
        /// </summary>
        public abstract void SetCSegmentation(Intarray segmentation);


        /// <summary>
        /// Number of groups generated.
        /// </summary>
        public abstract int Length();


        /// <summary>
        /// Get the bounding rectangle and mask for group "index".
        /// Optionally, expand the mask by the given margin.
        /// </summary>
        public abstract void GetMask(out Rect r, ref Bytearray outmask, int index, int margin);


        /// <summary>
        /// Get the mask around a given rectangle.
        /// </summary>
        public abstract void GetMaskAt(ref Bytearray outmask, int index, Rect b);


        /// <summary>
        /// Get the bounding box for the group "index".
        /// </summary>
        public abstract Rect BoundingBox(int index);

        public abstract int Start(int index);


        /// <summary>
        /// Return the last segment
        /// </summary>
        public abstract int End(int index);


        /// <summary>
        /// Return a list of all segments
        /// </summary>
        public abstract void GetSegments(Intarray result, int index);

        // Extract images corresponding to group "index" from the source.

        public abstract void Extract(Bytearray outa, Bytearray source,
                             byte dflt, int index, int grow=0);
        public abstract void Extract(Floatarray outa, Floatarray source,
                             float dflt, int index, int grow = 0);
        public abstract void ExtractWithMask(Bytearray outa, Bytearray mask,
                             Bytearray source, int index, int grow = 0);
        public abstract void ExtractWithMask(Floatarray outa, Bytearray mask,
                             Floatarray source, int index, int grow = 0);

        // slice extraction

        public abstract void ExtractSliced(Bytearray outa, Bytearray mask,
                                   Bytearray source, int index, int grow=0);
        public abstract void ExtractSliced(Bytearray outa, Bytearray source,
                                   byte dflt, int index, int grow=0);
        public abstract void ExtractSliced(Floatarray outa, Bytearray mask,
                                   Floatarray source, int index, int grow=0);
        public abstract void ExtractSliced(Floatarray outa, Floatarray source,
                                   float dflt, int index, int grow=0);

        /// <summary>
        /// Set the cost for classifying group "index" as class cls.
        /// </summary>
        public virtual void SetClass(int index, int cls, float cost)
        {
            SetClass(index, ((char)cls).ToString(), cost);
        }


        /// <summary>
        /// Set the cost for classifying group "index" as a sequence of strings
        /// </summary>
        public abstract void SetClass(int index, string cls, float cost);


        /// <summary>
        /// Space handling.  For any component, pixelSpace gets the amount
        /// of subsequent space (-1 for the last component).
        /// </summary>
        public abstract int PixelSpace(int i);


        /// <summary>
        /// Sets the costs associated with inserting a space after the
        /// character and not inserting the space.
        /// </summary>
        public abstract void SetSpaceCost(int index, float yes, float no);


        /// <summary>
        /// Extract the lattice corresponding to the classifications
        /// stored in the Grouper.
        /// </summary>
        public abstract void GetLattice(IGenericFst fst);

        /// <summary>
        /// Clear the lattice to allow constructing a new lattice.
        /// </summary>
        public abstract void ClearLattice();


        /// <summary>
        /// Set the grouper for iterating over the elements of the segmentation;
        /// This also computes the ground truth alignment.
        /// </summary>
        public abstract void SetSegmentationAndGt(Intarray segmentation, Intarray cseg, ref string text);

        public abstract int GetGtClass(int index);
        public abstract int GetGtIndex(int index);

    }
}
