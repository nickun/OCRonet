using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ocronet.Dynamic.ImgLib;

namespace Ocronet.Dynamic.Recognizers
{
    public class SimpleSpaceModel : ISpaceModel
    {
        double nonspace_cost;
        double space_cost;
        double aspect_threshold;
        int maxrange;

        public SimpleSpaceModel()
        {
            nonspace_cost = 4.0;
            space_cost = 9999.0;
            aspect_threshold = 2.0;
            maxrange = 30;
        }

        public List<List<float>> SpaceCosts(List<Candidate> candidates, Bytearray image)
        {
            /*
                Given a list of character recognition candidates and their
                classifications, and an image of the corresponding text line,
                compute a list of pairs of costs for putting/not putting a space
                after each of the candidate characters.

                The basic idea behind this simple algorithm is to try larger
                and larger horizontal closing operations until most of the components
                start having a "wide" aspect ratio; that's when characters have merged
                into words.  The remaining whitespace should be spaces.

                This is just a simple stopgap measure; it will be replaced with
                trainable space modeling.
             */
            int w = image.Dim(0);
            int h = image.Dim(1);

            Bytearray closed = new Bytearray();
            int r;
            for (r = 0; r < maxrange; r++)
            {
                if (r > 0)
                {
                    closed.Copy(image);
                    Morph.binary_close_circle(closed, r);
                }
                else
                    closed.Copy(image);
                Intarray labeled = new Intarray();
                labeled.Copy(closed);
                ImgLabels.label_components(ref labeled);
                Narray<Rect> rects = new Narray<Rect>();
                ImgLabels.bounding_boxes(ref rects, labeled);
                Floatarray aspects = new Floatarray();
                for (int i = 0; i < rects.Length(); i++)
                {
                    Rect rect = rects[i];
                    float aspect = rect.Aspect();
                    aspects.Push(aspect);
                }
                float maspect = NarrayUtil.Median(aspects);
                if (maspect >= this.aspect_threshold)
                    break;
            }

            // close with a little bit of extra space
            closed.Copy(image);
            Morph.binary_close_circle(closed, r+1);

            // compute the remaining aps
            //Morph.binary_dilate_circle();

            // every character box that ends near a cap gets a space appended


            return null;
        }
    }
}
