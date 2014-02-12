using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ocronet.Dynamic.Interfaces;
using Ocronet.Dynamic.Utils;
using Ocronet.Dynamic.Segmentation;
using Ocronet.Dynamic.ImgLib;
using Ocronet.Dynamic.Grouper;
using Ocronet.Dynamic.Component;
using Ocronet.Dynamic.IOData;

namespace Ocronet.Dynamic.Recognizers
{
    public class Linerec : IRecognizeLine
    {
        bool _disableJunk;
        const int reject_class = '~';
        ComponentContainerISegmentLine segmenter;
        ComponentContainerIGrouper grouper;
        ComponentContainerIModel classifier;
        Intarray counts;
        bool counts_warned;
        int ntrained;
        bool classifTrainRoundAttached;
        public event TrainEventHandler TrainRound;

        // temporary
        string transcript;
        //Bytearray line;
        Intarray segmentation;
        Bytearray binarized;
        int high_cost = 100;
        float space_threshold;

        public Linerec()
            : this("none")
        {
        }

        public Linerec(string classifier1 = "latin"/*none*/, string extractor1 = "scaledfe",
            string segmenter1 = "DpSegmenter", int use_reject = 1)
        {
            transcript = "";
            //line = new Bytearray();
            segmentation = new Intarray();
            binarized = new Bytearray();

            // component choices
            PDef("classifier", classifier1, "character classifier");
            PDef("extractor", extractor1, "feature extractor");
            PDef("segmenter", segmenter1, "line segmenter");
            PDef("grouper", "SimpleGrouper", "line grouper");
            // retraining
            PDef("cpreload", "none", "classifier to be loaded prior to training");
            // debugging
            PDef("verbose", 0, "verbose output from glinerec");
            // outputs
            PDef("use_priors", 0, "correct the classifier output by priors");
            PDef("use_reject", use_reject, "use a reject class (use posteriors only and train on junk chars)");
            PDef("maxcost", 20.0, "maximum cost of a character to be added to the output");
            PDef("minclass", 32, "minimum output class to be added (default=unicode space)");
            PDef("minprob", 1e-9, "minimum probability for a character to appear in the output at all");
            PDef("invert", 1, "invert the input line prior to char extraction");
            // segmentation
            PDef("maxrange", 5, "maximum number of components that are grouped together");
            // sanity limits on input
            PDef("minheight", 9, "minimum height of input line");
            PDef("maxheight", 300, "maximum height of input line");
            PDef("maxaspect", 2.0, "maximum height/width ratio of input line");
            // space estimation (FIXME factor this out eventually)
            PDef("space_fractile", 0.5, "fractile for space estimation");
            PDef("space_multiplier", 2.0, "multipler for space estimation");
            PDef("space_min", 0.2, "minimum space threshold (in xheight)");
            PDef("space_max", 1.1, "maximum space threshold (in xheight)");
            PDef("space_yes", 1.0, "cost of inserting a space");
            PDef("space_no", 5.0, "cost of not inserting a space");
            // back compability
            PDef("minsize_factor", 0.0, "");

            counts = new Intarray();
            segmenter = new ComponentContainerISegmentLine(ComponentCreator.MakeComponent<ISegmentLine>(PGet("segmenter")));
            grouper = new ComponentContainerIGrouper(ComponentCreator.MakeComponent<IGrouper>(PGet("grouper")));
            classifier = new ComponentContainerIModel(IModel.MakeModel(PGet("classifier")));
            TryAttachClassifierEvent(classifier.Object);

            Persist(classifier, "classifier");
            Persist(counts, "counts");
            Persist(segmenter, "segmenter");
            Persist(grouper, "grouper");

            if (!classifier.IsEmpty)
            {
                classifier.Object.Set("junk", PGeti("use_reject"));
                classifier.Object.SetExtractor(PGet("extractor"));
            }
            ntrained = 0;
            counts_warned = false;
        }

        public override string Name
        {
            get { return "linerec"; }
        }

        public bool DisableJunk
        {
            get { return _disableJunk; }
            set
            {
                _disableJunk = value;
                GetClassifier().DisableJunk = value;
            }
        }

        public void OnTrainRound(object sender, TrainEventArgs args)
        {
            if (TrainRound != null)
                TrainRound(this, args);
        }

        private void TryAttachClassifierEvent(IModel classifier)
        {
            if (classifier != null && !classifTrainRoundAttached)
            {
                classifier.TrainRound += new TrainEventHandler(Classifier_TrainRound);
                classifTrainRoundAttached = true;
            }
        }

        private void Classifier_TrainRound(object sender, TrainEventArgs e)
        {
            OnTrainRound(sender, e);
        }

        public override void Info()
        {
            bool bak = Logger.Default.verbose;
            Logger.Default.verbose = true;
            Logger.Default.WriteLine("Linerec");
            PPrint();
            Logger.Default.WriteLine(String.Format("segmenter: {0}", segmenter.IsEmpty ? "null" : segmenter.Object.Description));
            Logger.Default.WriteLine(String.Format("grouper: {0}", grouper.IsEmpty ? "null" : grouper.Object.Description));
            Logger.Default.WriteLine(String.Format("counts: {0} {1}", counts.Length(), NarrayUtil.Sum(counts)));
            //classifier.Object.Info();
            Logger.Default.verbose = bak;
        }

        public void SetClassifier(IModel classifier)
        {
            this.classifier.Object = classifier;
            if (this.classifier.Object != null)
                this.classifier.Object.SetExtractor(PGet("extractor"));
            TryAttachClassifierEvent(classifier);
        }

        public IModel GetClassifier()
        {
            return this.classifier.Object;
        }

        /// <summary>
        /// Original name: inc_class
        /// </summary>
        public void IncClass(int c)
        {
            while (counts.Length() <= c)
                counts.Push(0);
            counts[c]++;
        }

        public override void StartTraining(string type = "adaptation")
        {
            string preload = PGet("cpreload");
            if (preload != "none" && File.Exists(preload))
            {
                Load(preload);
                Global.Debugf("info", "preloaded classifier");
                classifier.Object.Info();
            }
        }

        public override void FinishTraining()
        {
            TryAttachClassifierEvent(classifier.Object);
            classifier.Object.UpdateModel();
        }

        public void SetLine(Bytearray image)
        {
            CHECK_ARG(image.Dim(1) < PGeti("maxheight"), "image.Dim(1) < PGeti(\"maxheight\")");
            
            // run the segmenter
            /*Narray<Rect> bboxes = new Narray<Rect>();
            Intarray iar = new Intarray();
            iar.Copy(image);
            ImgLabels.bounding_boxes(ref bboxes, iar);*/
            //Console.WriteLine("IMG SETLINE: imin:{0} imax:{1}", NarrayUtil.ArgMin(iar), NarrayUtil.ArgMax(iar));
            //Console.WriteLine("INDEX_BLACK:{0} {1} {2} {3}", bboxes[0].x0, bboxes[0].y0, bboxes[0].x1, bboxes[0].y1);
            //ImgIo.write_image_gray("image.png", image);
            OcrRoutine.binarize_simple(binarized, image);
            segmenter.Object.Charseg(ref segmentation, binarized);
            
            /*Intarray segm = new Intarray();
            segm.Copy(segmentation);
            ImgLabels.simple_recolor(segm);
            ImgIo.write_image_packed("segm_image.png", segm);*/

            //NarrayUtil.Sub(255, binarized);

            SegmRoutine.make_line_segmentation_black(segmentation);
            SegmRoutine.remove_small_components(segmentation, 3, 3);       // i add this line
            ImgLabels.renumber_labels(segmentation, 1);

            // set up the grouper
            grouper.Object.SetSegmentation(segmentation);
        }

        private int riuniform(int hi)
        {
            return DRandomizer.Default.nrand() % hi;
        }

        public override void Epoch(int n)
        {
            base.Epoch(n);
        }

        /// <summary>
        /// Train on a text line.
        /// <remarks>Usage is: call addTrainingLine with training data, then call finishTraining
        /// The state of the object is undefined between calling addTrainingLine and finishTraining, and it is
        /// an error to call recognizeLine before finishTraining completes.  This allows both batch
        /// and incemental training.
        /// NB: you might train on length 1 strings for single character training
        /// and might train on words if line alignment is not working
        /// (well, for some training data)</remarks>
        /// </summary>
        public void AddTrainingLine(Intarray cseg, string tr)
        {
            Bytearray gimage = new Bytearray();
            ClassifierUtil.segmentation_as_bitmap(gimage, cseg);
            AddTrainingLine(cseg, gimage, tr);
        }

        /// <summary>
        /// Train on a text line, given a segmentation.
        /// <remarks>This is analogous to addTrainingLine(bytearray,nustring) except that
        /// it takes the "ground truth" line segmentation.</remarks>
        /// </summary>
        public override bool AddTrainingLine(Intarray cseg, Bytearray image_grayscale, string tr)
        {
            Bytearray image = new Bytearray();
            image.Copy(image_grayscale);
            if (String.IsNullOrEmpty(tr))
            {
                Global.Debugf("error", "input transcript is empty");
                return false;
            }
            if (image.Dim(0) < PGeti("minheight"))
            {
                Global.Debugf("error", "input line too small ({0} x {1})", image.Dim(0), image.Dim(1));
                return false;
            }
            if (image.Dim(1) > PGeti("maxheight"))
            {
                Global.Debugf("error", "input line too high ({0} x {1})", image.Dim(0), image.Dim(1));
                return false;
            }
            if (image.Dim(1) * 1.0 / image.Dim(0) > PGetf("maxaspect"))
            {
                Global.Debugf("warn", "input line has bad aspect ratio ({0} x {1})", image.Dim(0), image.Dim(1));
                return false;
            }
            CHECK_ARG(image.Dim(0) == cseg.Dim(0) && image.Dim(1) == cseg.Dim(1),
                "image.Dim(0) == cseg.Dim(0) && image.Dim(1) == cseg.Dim(1)");

            bool use_reject = PGetb("use_reject") && !DisableJunk;

            // check and set the transcript
            transcript = tr;
            SetLine(image_grayscale);
            if (PGeti("invert") > 0)
                NarrayUtil.Sub(NarrayUtil.Max(image), image);
            for (int i = 0; i < transcript.Length; i++)
                CHECK_ARG((int)transcript[i] >= 32, "(int)transcript[i] >= 32");

            // compute correspondences between actual segmentation and
            // ground truth segmentation
            Narray<Intarray> segments = new Narray<Intarray>();
            GrouperRoutine.segmentation_correspondences(segments, segmentation, cseg);

            // now iterate through all the hypothesis segments and
            // train the classifier with them
            int total = 0;
            int junk = 0;
            for (int i = 0; i < grouper.Object.Length(); i++)
            {
                Intarray segs = new Intarray();
                grouper.Object.GetSegments(segs, i);

                // see whether this is a ground truth segment
                int match = -1;
                for (int j = 0; j < segments.Length(); j++)
                {
                    if (GrouperRoutine.Equals(segments[j], segs))
                    {
                        match = j;
                        break;
                    }
                }
                match -= 1;         // segments are numbered starting at 1
                int c = reject_class;
                if (match >= 0)
                {
                    if (match >= transcript.Length)
                    {
                        Global.Debugf("error", "mismatch between transcript and cseg: {0}", transcript);
                        continue;
                    }
                    else
                    {
                        c = (int)transcript[match];
                        Global.Debugf("debugmismatch", "index {0} position {1} char {2} [{3}]", i, match, (char)c, c);
                    }
                }

                if (c == reject_class)
                    junk++;

                // extract the character and add it to the classifier
                Rect b;
                Bytearray mask = new Bytearray();
                grouper.Object.GetMask(out b, ref mask, i, 0);
                Bytearray cv = new Bytearray();
                grouper.Object.ExtractWithMask(cv, mask, image, i, 0);
                Floatarray v = new Floatarray();
                v.Copy(cv);
                v /= 255.0;
                Global.Debugf("cdim", "character dimensions ({0},{1})", v.Dim(0), v.Dim(1));
                total++;
                if (use_reject)
                {
                    classifier.Object.XAdd(v, c);
                }
                else
                {
                    if (c != reject_class)
                        classifier.Object.XAdd(v, c);
                }
                if (c != reject_class)
                    IncClass(c);
                ntrained++;
            }
            Global.Debugf("detail", "AddTrainingLine trained {0} chars, {1} junk", total - junk, junk);
            return true;
        }

        /// <summary>
        /// Recognize a text line and return a lattice representing
        /// the recognition alternatives.
        /// </summary>
        public override double RecognizeLine(IGenericFst result, Bytearray image)
        {
            Intarray segmentation_ = new Intarray();
            return RecognizeLine(segmentation_, result, image);
        }

        public void EstimateSpaceSize()
        {
            Intarray labels = new Intarray();
            labels.Copy(segmentation);
            ImgLabels.label_components(ref labels);
            Narray<Rect> boxes = new Narray<Rect>();
            ImgLabels.bounding_boxes(ref boxes, labels);
            Floatarray distances = new Floatarray();
            distances.Resize(boxes.Length());
            distances.Fill(99999f);
            for (int i = 1; i < boxes.Length(); i++)
            {
                Rect b = boxes[i];
                for (int j = 1; j < boxes.Length(); j++)
                {
                    Rect n = boxes[j];
                    int delta = n.x0 - b.x1;
                    if (delta < 0) continue;
                    if (delta >= distances[i]) continue;
                    distances[i] = delta;
                }
            }
            float interchar = NarrayUtil.Fractile(distances, PGetf("space_fractile"));
            space_threshold = interchar * PGetf("space_multiplier");
            // impose some reasonable upper and lower bounds
            float xheight = 10.0f; // FIXME
            space_threshold = Math.Max(space_threshold, PGetf("space_min") * xheight);
            space_threshold = Math.Min(space_threshold, PGetf("space_max") * xheight);
        }

        /// <summary>
        /// This is a weird, optional method that exposes character segmentation
        /// for those line recognizers that have it segmentation contains colored pixels,
        /// and a transition in the transducer of the form * --- 1/eps --> * --- 2/a --> *
        /// means that pixels with color 1 and 2 together form the letter "a"
        /// </summary>
        public override double RecognizeLine(Intarray segmentation_, IGenericFst result, Bytearray image_)
        {
            double rate = 0.0;
            CHECK_ARG(image_.Dim(1) < PGeti("maxheight"),
                String.Format("input line too high ({0} x {1})", image_.Dim(0), image_.Dim(1)));
            CHECK_ARG(image_.Dim(1) * 1.0 / image_.Dim(0) < PGetf("maxaspect"),
                String.Format("input line has bad aspect ratio ({0} x {1})", image_.Dim(0), image_.Dim(1)));
            bool use_reject = PGetb("use_reject") && !DisableJunk;
            //Console.WriteLine("IMG: imin:{0} imax:{1}", NarrayUtil.ArgMin(image_), NarrayUtil.ArgMax(image_));
            Bytearray image = new Bytearray();
            image.Copy(image_);

            SetLine(image_);

            if (PGeti("invert") > 0)
                NarrayUtil.Sub(NarrayUtil.Max(image), image);
            segmentation_.Copy(segmentation);
            Bytearray available = new Bytearray();
            Floatarray cp = new Floatarray();
            Floatarray ccosts = new Floatarray();
            Floatarray props = new Floatarray();
            OutputVector p = new OutputVector();
            int ncomponents = grouper.Object.Length();
            int minclass = PGeti("minclass");
            float minprob = PGetf("minprob");
            float space_yes = PGetf("space_yes");
            float space_no = PGetf("space_no");
            float maxcost = PGetf("maxcost");

            // compute priors if possible; fall back on
            // using no priors if no counts are available
            Floatarray priors = new Floatarray();
            bool use_priors = PGeti("use_priors") > 0;
            if (use_priors)
            {
                if (counts.Length() > 0)
                {
                    priors.Copy(counts);
                    priors /= NarrayUtil.Sum(priors);
                }
                else
                {
                    if (!counts_warned)
                        Global.Debugf("warn", "use_priors specified but priors unavailable (old model)");
                    use_priors = false;
                    counts_warned = true;
                }
            }

            EstimateSpaceSize();

            for (int i = 0; i < ncomponents; i++)
            {
                Rect b;
                Bytearray mask = new Bytearray();
                grouper.Object.GetMask(out b, ref mask, i, 0);
                Bytearray cv = new Bytearray();
                grouper.Object.ExtractWithMask(cv, mask, image, i, 0);
                //ImgIo.write_image_gray("extrmask_image.png", cv);
                Floatarray v = new Floatarray();
                v.Copy(cv);
                v /= 255.0f;
                float ccost = classifier.Object.XOutputs(p, v);
                if (use_reject && classifier.Object.HigherOutputIsBetter)
                {
                    ccost = 0;
                    float total = p.Sum();
                    if (total > 1e-11f)
                    {
                        //p /= total;
                    }
                    else
                        p.Values.Fill(0.0f);
                }
                int count = 0;

                Global.Debugf("dcost", "output {0}", p.Keys.Length());
                for (int index = 0; index < p.Keys.Length(); index++)
                {
                    int j = p.Keys[index];
                    if (j < minclass) continue;
                    if (j == reject_class) continue;
                    float value = p.Values[index];
                    if (value <= 0.0f) continue;
                    if (value < minprob) continue;
                    float pcost = classifier.Object.HigherOutputIsBetter ? (float)-Math.Log(value) : value;
                    Global.Debugf("dcost", "{0} {1} {2}", j, pcost + ccost, (j > 32 ? (char)j : '_'));
                    float total_cost = pcost + ccost;
                    if (total_cost < maxcost)
                    {
                        if (use_priors)
                        {
                            total_cost -= (float)-Math.Log(priors[j]);
                        }
                        grouper.Object.SetClass(i, j, total_cost);
                        count++;
                    }
                }
                Global.Debugf("dcost", "");

                if (count == 0)
                {
                    float xheight = 10.0f;
                    if (b.Height() < xheight / 2 && b.Width() < xheight / 2)
                    {
                        grouper.Object.SetClass(i, (int)'~', high_cost / 2);
                    }
                    else
                    {
                        grouper.Object.SetClass(i, (int)'#', (b.Width() / xheight) * high_cost);
                    }
                }
                if (grouper.Object.PixelSpace(i) > space_threshold)
                {
                    Global.Debugf("spaces", "space {0}", grouper.Object.PixelSpace(i));
                    grouper.Object.SetSpaceCost(i, space_yes, space_no);
                }
            }

            grouper.Object.GetLattice(result);
            return rate;
        }

        public void LoadOldFormat(string path)
        {
            FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            BinaryReader reader = new BinaryReader(stream);
            try
            {
                LoadOldFormat(reader);
            }
            finally
            {
                reader.Close();
                stream.Close();
            }
        }

        public void LoadOldFormat(BinaryReader reader)
        {
            string magic = BinIO.magic_get(reader, "linerec".Length);
            CHECK_ARG(magic=="linerec" || magic=="linerc2", "magic=='linerec' || magic=='linerc2'");
            PLoad(reader);
            IComponent comp = ComponentIO.load_component(reader);
            IModel cmodel = comp as IModel;
            classifier.Object = cmodel;
            counts.Clear();
            if (magic == "linerec")
            {
                PSet("minsize_factor", 0.0);
            }
            else if (magic == "linerc2")
            {
                Narray<int> intcount = counts;
                BinIO.narray_read(reader, intcount);
            }
        }

        /// <summary>
        /// Save network to file
        /// </summary>
        /// <param name="filepath">full file path</param>
        public override void Save(string filepath)
        {
            FileStream stream = new FileStream(filepath, FileMode.Create, FileAccess.Write);
            BinaryWriter writer = new BinaryWriter(stream);
            try
            {
                ComponentIO.save_component(writer, this);
            }
            finally
            {
                writer.Close();
                stream.Close();
            }
        }

        public override void Load(string filename)
        {
            Linerec linerec = LoadLinerec(filename);
            if (linerec != null)
                this.classifier.Object = linerec.GetClassifier();
        }

        public static Linerec LoadLinerec(string filename)
        {
            FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            BinaryReader reader = new BinaryReader(stream);
            try
            {
                IComponent comp = ComponentIO.load_component(reader);
                // loaded component is Linerec
                Linerec linerec = comp as Linerec;
                if (linerec != null)
                    return linerec;

                // else component is IModel
                IModel cmodel = comp as IModel;
                if (cmodel != null)
                {
                    linerec = new Linerec();
                    linerec.SetClassifier(cmodel);
                    return linerec;
                }
            }
            finally
            {
                reader.Close();
                stream.Close();
            }

            // try load old format
            Linerec linerec_old = new Linerec();
            linerec_old.LoadOldFormat(filename);
            return linerec_old;
        }

    }
}
