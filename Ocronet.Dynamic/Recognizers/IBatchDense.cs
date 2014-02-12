using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ocronet.Dynamic.IOData;

namespace Ocronet.Dynamic.Recognizers
{
    public class IBatchDense : IBatch
    {
        protected Intarray c2i, i2c;

        public IBatchDense()
        {
            c2i = new Intarray();
            i2c = new Intarray();
            Persist(c2i, "c2i");
            Persist(i2c, "i2c");
        }

        protected override float Outputs(OutputVector result, Floatarray v)
        {
            Floatarray outar = new Floatarray();
            float cost = OutputsDense(outar, v);
            result.Clear();
            for (int i = 0; i < outar.Length(); i++)
                result[i2c[i]] = outar[i];
            return cost;
        }


        protected override void Train(IDataset ds)
        {
            if (!(ds.nSamples() > 0))
                throw new Exception("nSamples of IDataset must be > 0");
            if (!(ds.nFeatures() > 0))
                throw new Exception("nFeatures of IDataset must be > 0");
            if (c2i.Length() < 1)
            {
                Intarray raw_classes = new Intarray();
                raw_classes.ReserveTo(ds.nSamples());
                for (int i = 0; i < ds.nSamples(); i++)
                    raw_classes.Push(ds.Cls(i));
                ClassMap(c2i, i2c, raw_classes);
                /*Intarray classes = new Intarray();
                ctranslate(classes, raw_classes, c2i);*/
                //debugf("info","[mapped %d to %d classes]\n",c2i.length(),i2c.length());
            }
            TranslatedDataset mds = new TranslatedDataset(ds, c2i);
            TrainDense(mds);
        }

        public virtual void TrainDense(IDataset dataset)
        {
            throw new NotImplementedException();
        }

        public virtual float OutputsDense(Floatarray result, Floatarray v)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Compute a classmap that maps a set of possibly sparse classes onto a dense
        /// list of new classes and vice versa
        /// </summary>
        public static void ClassMap(Intarray out_class_to_index, Intarray out_index_to_class, Intarray classes)
        {
            int nclasses = NarrayUtil.Max(classes) + 1;
            Intarray hist = new Intarray(nclasses);
            hist.Fill(0);
            for (int i = 0; i < classes.Length(); i++)
            {
                if (classes[i] == -1) continue;
                hist[classes[i]]++;
            }
            int count = 0;
            for (int i = 0; i < hist.Length(); i++)
                if (hist[i] > 0) count++;
            out_class_to_index.Resize(nclasses);
            out_class_to_index.Fill(-1);
            out_index_to_class.Resize(count);
            out_index_to_class.Fill(-1);
            int index = 0;
            for (int i = 0; i < hist.Length(); i++)
            {
                if (hist[i] > 0)
                {
                    out_class_to_index[i] = index;
                    out_index_to_class[index] = i;
                    index++;
                }
            }
            CHECK_ARG(out_class_to_index.Length() == nclasses, "class_to_index.Length() == nclasses");
            CHECK_ARG(out_index_to_class.Length() == NarrayUtil.Max(out_class_to_index) + 1,
                "index_to_class.Length() == Max(class_to_index)+1");
            CHECK_ARG(out_index_to_class.Length() <= out_class_to_index.Length(),
                "index_to_class.Length() <= class_to_index.Length()");
        }

        /// <summary>
        /// Translate classes using a translation map
        /// </summary>
        private void ctranslate(Intarray result, Intarray values, Intarray translation)
        {
            result.Resize(values.Length());
            for (int i = 0; i < values.Length(); i++)
            {
                int v = values[i];
                if (v < 0) result[i] = v;
                else result[i] = translation[v];
            }
        }

    }
}
