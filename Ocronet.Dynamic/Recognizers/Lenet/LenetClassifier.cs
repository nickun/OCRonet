using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Ocronet.Dynamic.IOData;
using Ocronet.Dynamic.Utils;
using Ocronet.Dynamic.Component;

namespace Ocronet.Dynamic.Recognizers.Lenet
{
    public class LenetClassifier : IBatch
    {
        public AutoLenetWrapper CharClass;
        public AutoLenetWrapper JunkClass;
        int csize = 32;
        int junkchar;
        int totalEpochCount;    // for train event

        public LenetClassifier()
        {
            DRandomizer.Default.init_drand(DateTime.Now.Millisecond);
            //LenetWrapper.StartRedirectStdout();
            // Char Class
            CharClass = new AutoLenetWrapper();
            CharClass.TrainRound += new TrainEventHandler(CharClass_TrainRound);
            // Junk Class
            JunkClass = new AutoLenetWrapper();
            JunkClass.TrainRound += new TrainEventHandler(JunkClass_TrainRound);

            PDef("junkchar", (int)'~', "junk character");
            PDef("junk", 1, "train a separate junk classifier");
            PDef("epochs", 10, "number of training epochs");
            junkchar = -1;
            Persist(new LenetIOWrapper(CharClass, "CharClass"), "charclass");
            Persist(new LenetIOWrapper(JunkClass, "JunkClass"), "junkclass");
        }

        private void CharClass_TrainRound(object sender, TrainEventArgs e)
        {
            OnTrainRound(this, new TrainEventArgs(
                totalEpochCount, e.Error, e.SuccessSamples, e.TotalSamples, e.BestError,
                e.TrainCycleDuration, e.TestCycleDuration, "Char Classifier"
                ));
            totalEpochCount++;
        }

        private void JunkClass_TrainRound(object sender, TrainEventArgs e)
        {
            OnTrainRound(this, new TrainEventArgs(
                totalEpochCount, e.Error, e.SuccessSamples, e.TotalSamples, e.BestError,
                e.TrainCycleDuration, e.TestCycleDuration, "Junk Classifier"
                ));
            totalEpochCount++;
        }
        

        public override string Name
        {
            get { return "lenet"; }
        }


        public override bool HigherOutputIsBetter
        {
            get { return !CharClass.AsciiTarget; }
        }


        public void GetStdout(out StringBuilder sbout)
        {
            sbout = new StringBuilder(4096);
            LenetWrapper.GetStdout(sbout);
        }

        /// <summary>
        /// Junk ascii code
        /// </summary>
        public int jc()
        {
            if (junkchar < 0)
                junkchar = PGeti("junkchar");
            return junkchar;
        }

        public override void Info()
        {
            bool bak = Logger.Default.verbose;
            Logger.Default.verbose = true;
            Logger.Default.WriteLine(
                String.Format("LenetClassifier ({0}{1})", CharClass.IsEmpty ? "" : "char", JunkClass.IsEmpty ? "" : ",junk"));
            Logger.Default.verbose = bak;
        }

        public void InitNumSymbLatinAlphabet()
        {
            string sclasses = "!\"$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_abcdefghijklmnopqrstuvwxyz{|}";
            SortedList<int, int> classes = new SortedList<int, int>(sclasses.Length);
            foreach(char c in sclasses)
                classes.Add((int)c, (int)c);
            Initialize(classes.Keys.ToArray(), false);
        }

        /// <summary>
        /// Do external initialize (dll instance)
        /// </summary>
        public void Initialize(int[] classes, bool asciiTarget = true)
        {
            List<int> cl = new List<int>(classes.Length);
            for (int i = 0; i < classes.Length; i++)
                if (classes[i] != jc())
                    cl.Add(classes[i]);
            CharClass.AsciiTarget = asciiTarget;
            //CharClass.NetNorm = true;
            CharClass.Classes = cl.ToArray();
            JunkClass.AsciiTarget = asciiTarget;
            //JunkClass.NetNorm = true;
            int[] jcl;
            if (asciiTarget)
                jcl = new int[] { (int)'0', (int)'1' }; // '0' - any symbol, '1' - junk
            else
                jcl = new int[] { 0, 1 };
            JunkClass.Classes = jcl;

            // create char classifier instance
            CharClass.CreateLenet(CharClass.Classes.Length, CharClass.Classes,
                CharClass.TanhSigmoid, CharClass.NetNorm, CharClass.AsciiTarget);
            // create junk classifier instance
            if (PGeti("junk") > 0)
                JunkClass.CreateLenet(JunkClass.Classes.Length, JunkClass.Classes,
                    JunkClass.TanhSigmoid, JunkClass.NetNorm, JunkClass.AsciiTarget);
        }

        public override void SetExtractor(string name)
        {
            base.SetExtractor(name);
            // set input size 1024 (32*32) for Lenet
            if (!_extractor.IsEmpty)
            {
                _extractor.Object.PSet("csize", csize);
                _extractor.Object.PSet("indent", 2);
            }
        }

        public override void UpdateModel()
        {
            base.UpdateModel();
        }

        public override int nFeatures()
        {
            if (CharClass.HrLenet != IntPtr.Zero)
                return csize * csize;
            else
                return 0;
        }

        public override int nClasses()
        {
            if (CharClass.HrLenet != IntPtr.Zero)
                return CharClass.Classes.Length + (PGeti("junk")>0 && !DisableJunk ? 1 : 0);  // plus one for junk
            else
                return 0;
        }

        public override int nModels()
        {
            return 2;
        }


        protected override float Outputs(OutputVector result, Floatarray v)
        {
            result.Clear();
            if (v.Rank() == 1)
                v.Reshape(csize, csize, 0, 0);
            // byte array input
            StdInput vinput = new StdInput(v);
            byte[] buffer = vinput.GetDataBuffer();
            
            // char classifier compute outputs
            if (CharClass.AsciiTarget)
                // net output 0..~ (lower - winner)
                CharClass.ComputeOutputs(buffer, vinput.Length, vinput.Height, vinput.Width, result);
            else
                // net output 0..1; (higher - winner)
                CharClass.ComputeOutputsRaw(buffer, vinput.Length, vinput.Height, vinput.Width, result);

            // junk classifier
            if (PGetb("junk") && !DisableJunk && !JunkClass.IsEmpty)
            {
                OutputVector jv = new OutputVector();
                if (JunkClass.AsciiTarget)
                {
                    JunkClass.ComputeOutputs(buffer, vinput.Length, vinput.Height, vinput.Width, jv);
                    result[jc()] = jv.Value(1);
                }
                else
                {
                    //result.Normalize();
                    result.Normalize2();
                    JunkClass.ComputeOutputsRaw(buffer, vinput.Length, vinput.Height, vinput.Width, jv);
                    jv.Normalize2();
                    for (int i = 0; i < result.nKeys(); i++)
                        result.Values[i] *= jv.Value(0);
                    result[jc()] = jv.Value(1);
                }
            }

            return 0.0f;
        }

        private int[] CreateClassesFromDataset(IDataset ds)
        {
            // create class list from dataset
            SortedDictionary<int, int> keymap = new SortedDictionary<int, int>();
            for (int i = 0; i < ds.nSamples(); i++)
            {
                if (!keymap.ContainsKey(ds.Cls(i)))
                    keymap.Add(ds.Cls(i), 1);
                else
                    keymap[ds.Cls(i)]++;
            }
            int[] classes = new int[keymap.Count];
            keymap.Keys.CopyTo(classes, 0);

            // show class counts
            Console.WriteLine("Classes counts:");
            string showline = "";
            int ishow = 0;
            for (int i = 0; i < keymap.Count; i++)
            {
                ishow++;
                if (classes[i] >= 32)
                    showline += String.Format("{0,-9}", String.Format("{0}[{1}]", (char)classes[i], keymap[classes[i]]));
                else
                    showline += String.Format("{0,-9}", String.Format("{0}[{1}]", classes[i], keymap[classes[i]]));
                if (ishow % 8 == 0)
                {
                    Console.WriteLine(showline);
                    showline = "";
                }
            }
            Console.WriteLine(showline);
            return classes;
        }


        protected override void Train(IDataset ds)
        {
            bool use_junk = PGetb("junk") && !DisableJunk;
            int nsamples = ds.nSamples();
            if (PExists("%nsamples"))
                nsamples += PGeti("%nsamples");

            Global.Debugf("info", "Training content classifier");

            if (CharClass.IsEmpty)
            {
                Initialize(CreateClassesFromDataset(ds));
            }
            if (use_junk/*&& !JunkClass.IsEmpty*/)
            {
                Intarray nonjunk = new Intarray();
                for (int i = 0; i < ds.nSamples(); i++)
                    if (ds.Cls(i) != jc())
                        nonjunk.Push(i);
                Datasubset nonjunkds = new Datasubset(ds, nonjunk);
                CharClass.TrainDense(nonjunkds, PGeti("epochs"));
            }
            else
            {
                CharClass.TrainDense(ds, PGeti("epochs"));
            }

            if (use_junk /*&& !JunkClass.IsEmpty*/)
            {
                Global.Debugf("info", "Training junk classifier");
                Intarray isjunk = new Intarray();
                int njunk = 0;
                for (int i = 0; i < ds.nSamples(); i++)
                {
                    bool j = (ds.Cls(i) == jc());
                    isjunk.Push(JunkClass.Classes[Convert.ToInt32(j)]);
                    if (j) njunk++;
                }
                if (njunk > 0)
                {
                    MappedDataset junkds = new MappedDataset(ds, isjunk);
                    JunkClass.TrainDense(junkds, PGeti("epochs"));
                }
                else
                {
                    Global.Debugf("warn", "you are training a junk class but there are no samples to train on");
                    JunkClass.DeleteLenet();
                }
            }
            PSet("%nsamples", nsamples);
        }

        /// <summary>
        /// Save network to new format
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

        /// <summary>
        /// Load network from new format
        /// </summary>
        /// <param name="filepath">full file path<</param>
        public override void Load(string filepath)
        {
            FileStream stream = new FileStream(filepath, FileMode.Open, FileAccess.Read);
            BinaryReader reader = new BinaryReader(stream);
            try
            {
                string s;
                BinIO.string_read(reader, out s);
                if (s == "<object>")
                {
                    BinIO.string_read(reader, out s);
                    if (s != this.Name && s != this.GetType().Name)
                        throw new Exception("LenetClassifier: incorrect file format");
                    this.Load(reader);
                    BinIO.string_read(reader, out s);
                    if (s != "</object>")
                        throw new Exception("Expected string: </object>");
                }
                else
                    throw new Exception("Expected string: <object>");
            }
            finally
            {
                reader.Close();
                stream.Close();
            }
        }

    }
}
