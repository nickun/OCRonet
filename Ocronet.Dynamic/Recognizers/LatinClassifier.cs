using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ocronet.Dynamic.Component;
using Ocronet.Dynamic.Interfaces;
using Ocronet.Dynamic.Utils;
using Ocronet.Dynamic.IOData;

namespace Ocronet.Dynamic.Recognizers
{
    /// <summary>
    /// Classifier specifically intended for Latin script
    /// (although there are actually not a lot of
    /// customizations for latin script)
    /// </summary>
    public class LatinClassifier : IBatch
    {
        ComponentContainerIModel charclass;
        ComponentContainerIModel junkclass;
        ComponentContainerIModel ulclass;
        int junkchar;
        int totalRoundCount;    // for train event
        bool charTrainRoundAttached = false;
        bool junkTrainRoundAttached = false;

        public LatinClassifier()
        {
            DRandomizer.Default.init_drand(DateTime.Now.Millisecond);
            charclass = new ComponentContainerIModel(IModel.MakeModel(PGet("charclass")));
            junkclass = new ComponentContainerIModel(IModel.MakeModel(PGet("junkclass")));
            ulclass = new ComponentContainerIModel();
            PDef("junkchar", (int)'~', "junk character");
            PDef("junkclass", "mlp", "junk classifier");
            PDef("charclass", "mappedmlp", "character classifier");
            PDef("junk", 1, "train a separate junk classifier");
            PDef("ul", 0, "do upper/lower reclassification");
            PDef("ulclass", "mlp", "upper/lower classifier");
            junkchar = -1;
            Persist(charclass, "charclass");
            Persist(junkclass, "junkclass");
            Persist(ulclass, "ulclass");
            TryAttachCharClassifierEvent(charclass.Object);
            TryAttachJunkClassifierEvent(junkclass.Object);
        }

        public override string Name
        {
            get { return "latin"; }
        }

        private void TryAttachCharClassifierEvent(IModel classifier)
        {
            if (classifier != null && !charTrainRoundAttached)
            {
                classifier.TrainRound += new TrainEventHandler(CharClass_TrainRound);
                charTrainRoundAttached = true;
            }
        }
        private void TryAttachJunkClassifierEvent(IModel classifier)
        {
            if (classifier != null && !junkTrainRoundAttached)
            {
                classifier.TrainRound += new TrainEventHandler(JunkClass_TrainRound);
                junkTrainRoundAttached = true;
            }
        }

        private void CharClass_TrainRound(object sender, TrainEventArgs e)
        {
            OnTrainRound(this, new TrainEventArgs(
                totalRoundCount, e.Error, e.SuccessSamples, e.TotalSamples, e.BestError,
                e.TrainCycleDuration, e.TestCycleDuration, "Char Classifier"
                ));
            totalRoundCount++;
        }
        private void JunkClass_TrainRound(object sender, TrainEventArgs e)
        {
            OnTrainRound(this, new TrainEventArgs(
                totalRoundCount, e.Error, e.SuccessSamples, e.TotalSamples, e.BestError,
                e.TrainCycleDuration, e.TestCycleDuration, "Junk Classifier"
                ));
            totalRoundCount++;
        }

        public override void Info()
        {
            bool bak = Logger.Default.verbose;
            Logger.Default.verbose = true;
            Logger.Default.WriteLine("CHARCLASS MODEL");
            if (!charclass.IsEmpty)
                charclass.Object.Info();
            Logger.Default.WriteLine("JUNKCLASS MODEL");
            if (!junkclass.IsEmpty)
                junkclass.Object.Info();
            Logger.Default.WriteLine("ULCLASS MODEL");
            if (!ulclass.IsEmpty)
                ulclass.Object.Info();
            Logger.Default.verbose = bak;
        }

        public int jc()
        {
            if (junkchar < 0)
                junkchar = PGeti("junkchar");
            return junkchar;
        }

        public override int nFeatures()
        {
            if (!charclass.IsEmpty)
                return charclass.Object.nFeatures();
            else
                return 0;
        }

        public override int nClasses()
        {
            if (!charclass.IsEmpty)
                return Math.Max(jc() + 1, charclass.Object.nClasses());
            else
                return 0;
        }

        public override int nModels()
        {
            return 2;
        }

        public override void SetModel(IModel cf, int which)
        {
            if (which == 0)
                charclass.SetComponent(cf);
            else if (which == 1)
                ulclass.SetComponent(cf);
        }

        public override IComponent GetModel(int which)
        {
            if (which == 0)
                return charclass.Object;
            else if (which == 1)
                return ulclass.Object;
            throw new Exception("oops");
        }

        public IModel GetIModel(int which)
        {
            if (which == 0)
                return charclass.Object;
            else if (which == 1) 
                return ulclass.Object;
            throw new Exception("oops");
        }

        protected override void Train(IDataset ds)
        {
            bool use_junk = PGetb("junk") && !DisableJunk;

            if (charclass.IsEmpty)
            {
                charclass.SetComponent(ComponentCreator.MakeComponent(PGet("charclass")));
                TryAttachCharClassifierEvent(charclass.Object);
            }
            if (junkclass.IsEmpty)
            {
                junkclass.SetComponent(ComponentCreator.MakeComponent(PGet("junkclass")));
                TryAttachJunkClassifierEvent(junkclass.Object);
            }
            if (ulclass.IsEmpty)
                ulclass.SetComponent(ComponentCreator.MakeComponent(PGet("ulclass")));
            
            Global.Debugf("info", "Training content classifier");
            if (use_junk && !junkclass.IsEmpty)
            {
                Intarray nonjunk = new Intarray();
                for (int i = 0; i < ds.nSamples(); i++)
                    if (ds.Cls(i) != jc())
                        nonjunk.Push(i);
                Datasubset nonjunkds = new Datasubset(ds, nonjunk);
                charclass.Object.XTrain(nonjunkds);
            }
            else
            {
                charclass.Object.XTrain(ds);
            }

            if (use_junk && !junkclass.IsEmpty)
            {
                Global.Debugf("info", "Training junk classifier");
                Intarray isjunk = new Intarray();
                int njunk = 0;
                for (int i = 0; i < ds.nSamples(); i++)
                {
                    bool j = (ds.Cls(i) == jc());
                    isjunk.Push(Convert.ToInt32(j));
                    if (j) njunk++;
                }
                if (njunk > 0)
                {
                    MappedDataset junkds = new MappedDataset(ds, isjunk);
                    junkclass.Object.XTrain(junkds);
                }
                else
                {
                    Global.Debugf("warn", "you are training a junk class but there are no samples to train on");
                    junkclass.SetComponent(null);
                }

                if (PGeti("ul") > 0 && !ulclass.IsEmpty)
                {
                    throw new Exception("ulclass not implemented");
                }
            }
        }

        protected override float Outputs(OutputVector result, Floatarray v)
        {
            result.Clear();
            charclass.Object.XOutputs(result, v);
            CHECK_ARG(result.nKeys() > 0, "result.nKeys() > 0");

            if (PGetb("junk") && !DisableJunk && !junkclass.IsEmpty)
            {
                result.Normalize();
                OutputVector jv = new OutputVector();
                junkclass.Object.XOutputs(jv, v);
                for (int i = 0; i < result.nKeys(); i++)
                    result.Values[i] *= jv.Value(0);
                result[jc()] = jv.Value(1);
            }

            if (PGeti("ul") > 0 && !ulclass.IsEmpty)
            {
                throw new Exception("ulclass not implemented");
            }

            return 0.0f;
        }

    }
}
