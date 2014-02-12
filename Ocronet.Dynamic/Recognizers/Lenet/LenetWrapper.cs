using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Ocronet.Dynamic.IOData;
using Ocronet.Dynamic.Utils;

namespace Ocronet.Dynamic.Recognizers.Lenet
{
    public class LenetWrapper
    {
        public event TrainEventHandler TrainRound;
        public IntPtr HrLenet;
        public int[] Classes;
        public Dictionary<int, int> C2i;
        public bool TanhSigmoid;
        public bool NetNorm;
        public bool AsciiTarget;
        int csize = 32;
        StringBuilder sbout;

        public LenetWrapper()
        {
            HrLenet = IntPtr.Zero;
            Classes = new int[0];
            TanhSigmoid = false;
            NetNorm = true;
            AsciiTarget = false;
            sbout = new StringBuilder(10240);
        }

        public void OnTrainRound(object sender, TrainEventArgs args)
        {
            if (TrainRound != null)
                TrainRound(this, args);
        }

        public bool IsEmpty
        {
            get { return HrLenet == IntPtr.Zero; }
        }

        public void SetEmpty()
        {
            HrLenet = IntPtr.Zero;
        }


        public int CreateLenet(int nclasses, int[] classes, bool tanhSigmoid = false,
            bool netNorm = false, bool asciiTarget = true)
        {
            Classes = classes;
            TanhSigmoid = tanhSigmoid;
            NetNorm = netNorm;
            AsciiTarget = asciiTarget;
            C2i = new Dictionary<int, int>(Classes.Length);
            for (int i = 0; i < Classes.Length; i++)
                C2i.Add(Classes[i], i);
            int result = CreateLenet(out HrLenet, nclasses, classes, tanhSigmoid, netNorm, asciiTarget);
            GetStdout(sbout);   // get train messages
            Console.Write(sbout.ToString());
            return result;
        }

        public void DeleteLenet()
        {
            DeleteLenet(HrLenet);
            HrLenet = IntPtr.Zero;
        }

        public void LoadNetworkFromBuffer(double[] buffer, int size)
        {
            LoadNetworkFromBuffer(HrLenet, buffer, size);
            GetStdout(sbout);   // get train messages
            Console.Write(sbout.ToString());
        }

        public void SaveNetworkToBuffer(out int size, out double[] darray)
        {
            IntPtr ppArray;
            SaveNetworkToBuffer(HrLenet, out size, out ppArray);
            double[] dbuffer = new double[size];
            Marshal.Copy(ppArray, dbuffer, 0, size);
            // free wrapped memory
            Marshal.FreeCoTaskMem(ppArray);
            darray = dbuffer;
        }

        public void ComputeOutputs(byte[] buffer, int size, int height, int width,
            out int[] outClasses, out double[] outEnergies, out int outSize)
        {
            int[] outclasses = new int[Classes.Length];
            double[] outenergies = new double[Classes.Length];
            ComputeOutputs(HrLenet, buffer, size, height, width, outclasses, outenergies, out outSize);
            outClasses = outclasses;
            outEnergies = outenergies;
        }

        public void ComputeOutputsRaw(byte[] buffer, int size, int height, int width,
            out int[] outClasses, out double[] outEnergies, out int outSize)
        {
            int[] outclasses = new int[Classes.Length];
            double[] outenergies = new double[Classes.Length];
            ComputeOutputsRaw(HrLenet, buffer, size, height, width, outclasses, outenergies, out outSize);
            outClasses = outclasses;
            outEnergies = outenergies;
        }

        public void ComputeOutputs(byte[] buffer, int size, int height, int width, OutputVector result)
        {
            int osize;
            int[] oclasses;
            double[] oenergies;
            ComputeOutputs(buffer, size, height, width, out oclasses, out oenergies, out osize);
            // fill result vector
            for (int i = 0; i < osize; i++)
                result[oclasses[i]] = Convert.ToSingle(oenergies[i]);
        }

        public void ComputeOutputsRaw(byte[] buffer, int size, int height, int width, OutputVector result)
        {
            int osize;
            int[] oclasses;
            double[] oenergies;
            ComputeOutputsRaw(buffer, size, height, width, out oclasses, out oenergies, out osize);
            // fill result vector
            for (int i = 0; i < osize; i++)
                result[oclasses[i]] = Convert.ToSingle(oenergies[i]);
        }

        public virtual void TrainDense(IDataset ds, int epochs)
        {
            Global.Debugf("info", "Start training..");
            float split = 0.8f;
            int mlp_cv_max = 5000;
            // perform a split for cross-validation, making sure
            // that we don't have the same sample in both the
            // test and the training set (even if the data set
            // is the result of resampling)
            Intarray test_ids = new Intarray();
            Intarray ids = new Intarray();
            for (int i = 0; i < ds.nSamples(); i++)
                ids.Push(ds.Id(i));
            NarrayUtil.Uniq(ids);
            Global.Debugf("cvdetail", "reduced {0} ids to {1} ids", ds.nSamples(), ids.Length());
            NarrayUtil.Shuffle(ids);
            int nids = (int)((1.0 - split) * ids.Length());
            nids = Math.Min(nids, mlp_cv_max);
            for (int i = 0; i < nids; i++)
                test_ids.Push(ids[i]);
            NarrayUtil.Quicksort(test_ids);
            Intarray training = new Intarray();
            Intarray testing = new Intarray();
            for (int i = 0; i < ds.nSamples(); i++)
            {
                int id = ds.Id(i);
                if (ClassifierUtil.Bincontains(test_ids, id))
                    testing.Push(i);
                else
                    training.Push(i);
            }
            Global.Debugf("info", "#training {0} #testing {1}",
                   training.Length(), testing.Length());
            Datasubset trs = new Datasubset(ds, training);
            Datasubset tss = new Datasubset(ds, testing);
            TrainBatch(trs, tss, epochs);
        }

        public TrainInfo TestDense(IDataset ts)
        {
            TrainInfo trInfo = new TrainInfo();
            bool first = true;
            Floatarray v = new Floatarray();
            // Send Test Dataset to Lenet
            for (int i = 0; i < ts.nSamples(); i++)
            {
                ts.Input(v, i);
                if (v.Rank() == 1)
                    v.Reshape(csize, csize, 0, 0);
                StdInput linput = new StdInput(v);
                if (first)
                {
                    first = false;
                    BeginTestEpoch(HrLenet, linput.Height, linput.Width, ts.nSamples());   // init test
                }
                try
                {
                    if (C2i.ContainsKey(ts.Cls(i)))
                        AddSampleToTestOfEpoch(HrLenet, linput.GetDataBuffer(), linput.Length, C2i[ts.Cls(i)]);
                }
                catch (Exception e)
                {
                    GetStdout(sbout);   // get train messages
                    if (sbout.Length > 0)
                        Global.Debugf("error", sbout.ToString() + "\r\nException in AddSampleToTestOfEpoch");
                    throw new Exception("Exception in AddSampleToTestOfEpoch\r\n" + e.Message);
                }
            }

            // do test one epoch
            try
            {
                EndAndRunTestEpoch(HrLenet, ref trInfo);
            }
            catch (Exception e)
            {
                GetStdout(sbout);   // get test messages
                if (sbout.Length > 0)
                    Global.Debugf("error", sbout.ToString() + "\r\nException in EndAndRunTestEpoch");
                throw new Exception("Exception in EndAndRunTestEpoch\r\n" + e.Message);
            }

            return trInfo;
        }

        public virtual TrainInfo TrainBatch(IDataset ds, IDataset ts, int epochs)
        {
            TrainInfo trInfo = new TrainInfo();
            bool first = true;
            Floatarray v = new Floatarray();

            // Send Train Dataset to Lenet
            for (int i = 0; i < ds.nSamples(); i++)
            {
                ds.Input(v, i);
                if (v.Rank() == 1)
                    v.Reshape(csize, csize, 0, 0);
                StdInput linput = new StdInput(v);
                try
                {
                    if (first)
                    {
                        first = false;
                        //StartRedirectStdout();  // start redirect cout to string buffer
                        BeginTrainEpoch(HrLenet, linput.Height, linput.Width, ds.nSamples(), ts.nSamples());   // init train
                    }
                    if (C2i.ContainsKey(ds.Cls(i)))
                        AddSampleToTrainOfEpoch(HrLenet, linput.GetDataBuffer(), linput.Length, C2i[ds.Cls(i)]);
                    else
                        Global.Debugf("error", "class '{0}' is not contained in the char list", (char)ds.Cls(i));
                }
                catch (Exception e)
                {
                    GetStdout(sbout);   // get train messages
                    if (sbout.Length > 0)
                        Global.Debugf("error", sbout.ToString() + "\r\nException in AddSampleToTrainOfEpoch");
                    throw new Exception("Exception in AddSampleToTrainOfEpoch\r\n" + e.Message);
                }
            }

            // Send Test Dataset to Lenet
            for (int i = 0; i < ts.nSamples(); i++)
            {
                ts.Input(v, i);
                if (v.Rank() == 1)
                    v.Reshape(csize, csize, 0, 0);
                StdInput linput = new StdInput(v);
                try
                {
                    if (C2i.ContainsKey(ts.Cls(i)))
                        AddSampleToTestOfEpoch(HrLenet, linput.GetDataBuffer(), linput.Length, C2i[ts.Cls(i)]);
                }
                catch (Exception e)
                {
                    GetStdout(sbout);   // get train messages
                    if (sbout.Length > 0)
                        Global.Debugf("error", sbout.ToString() + "\r\nException in AddSampleToTestOfEpoch");
                    throw new Exception("Exception in AddSampleToTestOfEpoch\r\n" + e.Message);
                }
            }

            // debug save mnist
            //SaveTrainMnist(HrLenet, "debug-images-idx3-ubyte", "debug-labels-idx1-ubyte");

            // do train epochs
            for (int epoch = 0; epoch < epochs; epoch++)
            {
                trInfo = new TrainInfo();
                try
                {
                    DateTime startDate = DateTime.Now;
                    EndAndRunTrainEpoch(HrLenet, ref trInfo);   // do train one epoch
                    // show train info
                    Global.Debugf("info",
                        String.Format("|{0,7}| Energy:{1:0.#####} Correct:{2:0.00#%} Errors:{3:0.00#%} Count:{4} ",
                            trInfo.age, trInfo.energy, (trInfo.correct / (float)trInfo.size),
                            (trInfo.error / (float)trInfo.size), trInfo.size) );
                    Global.Debugf("info",
                        String.Format("     TEST Energy={0:0.#####} Correct={1:0.00#%} Errors={2:0.00#%} Count={3} ",
                            trInfo.tenergy, (trInfo.tcorrect / (float)trInfo.tsize),
                            (trInfo.terror / (float)trInfo.tsize), trInfo.tsize));
                    TimeSpan spanTrain = DateTime.Now - startDate;
                    Global.Debugf("info", String.Format("          training time: {0} minutes, {1} seconds",
                        (int)spanTrain.TotalMinutes, spanTrain.Seconds));

                    // get dll stdout messages
                    GetStdout(sbout);
                    if (sbout.Length > 0)
                        Console.Write(sbout.ToString());
                }
                catch (Exception e)
                {
                    GetStdout(sbout);   // get train messages
                    if (sbout.Length > 0)
                        Global.Debugf("error", sbout.ToString() + "\r\nException in EndAndRunTrainEpoch");
                    throw new Exception("Exception in EndAndRunTrainEpoch\r\n" + e.Message);
                }
            }
            return trInfo;
        }


        #region Dll Imports

        [StructLayout(LayoutKind.Sequential)]
        public struct TrainInfo
        {
            [MarshalAs(UnmanagedType.I4)]
            public int age;
            [MarshalAs(UnmanagedType.I4)]
            public int size;
            [MarshalAs(UnmanagedType.I4)]
            public int correct;
            [MarshalAs(UnmanagedType.I4)]
            public int error;
            [MarshalAs(UnmanagedType.R8)]
            public double energy;
            [MarshalAs(UnmanagedType.I4)]
            public int tsize;
            [MarshalAs(UnmanagedType.I4)]
            public int tcorrect;
            [MarshalAs(UnmanagedType.I4)]
            public int terror;
            [MarshalAs(UnmanagedType.R8)]
            public double tenergy;
        }

        [DllImport("ConvClassifier.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool StartRedirectStdout();

        [DllImport("ConvClassifier.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetStdout([In, Out] StringBuilder outstring);

        /// <summary>
        /// Create and initialize lenet
        /// </summary>
        /// <param name="hrlenet"></param>
        /// <param name="nclasses"></param>
        /// <param name="classes"></param>
        /// <param name="tanhSigmoid">default false</param>
        /// <param name="netNorm">default false</param>
        /// <param name="asciiTarget">default true</param>
        /// <returns></returns>
        [DllImport("ConvClassifier.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int CreateLenet(out IntPtr hrlenet, int nclasses, [In]int[] classes, bool tanhSigmoid,
            bool netNorm, bool asciiTarget);

        [DllImport("ConvClassifier.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void DeleteLenet(IntPtr hrlenet);

        [DllImport("ConvClassifier.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int LoadNetwork(IntPtr hrlenet, string networkFile);

        [DllImport("ConvClassifier.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SaveNetwork(IntPtr hrlenet, string networkFile);

        [DllImport("ConvClassifier.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void LoadNetworkFromBuffer(IntPtr hrlenet, [In]double[] buffer, int size);

        /// <summary>
        /// Return allocated array of network (Marshal.CoTaskMemAlloc)
        /// </summary>
        /// <param name="buffer">return allocated array, call Marshal.FreeCoTaskMem
        /// to free this memory</param>
        /// <param name="size">return size of allocated array</param>
        /// <returns></returns>
        [DllImport("ConvClassifier.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SaveNetworkToBuffer(IntPtr hrlenet, out int size, out IntPtr ppArray);

        [DllImport("ConvClassifier.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int LoadTrainMnist(IntPtr hrlenet, string trainDatafile, string trainLabelfile);

        [DllImport("ConvClassifier.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int LoadTestMnist(IntPtr hrlenet, string testDatafile, string testLabelfile);

        [DllImport("ConvClassifier.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SaveTrainMnist(IntPtr hrlenet, string trainDatafile, string trainLabelfile);


        [DllImport("ConvClassifier.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void TrainOneEpoch(IntPtr hrlenet, ref TrainInfo trainInfo);

        [DllImport("ConvClassifier.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void BeginTrainEpoch(IntPtr hrlenet, int height, int width, int trainNsamples, int testNsamples = -1);

        [DllImport("ConvClassifier.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void BeginTestEpoch(IntPtr hrlenet, int height, int width, int testNsamples);

        [DllImport("ConvClassifier.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void AddSampleToTrainOfEpoch(IntPtr hrlenet, [In]byte[] buffer, int size, int iclass);

        [DllImport("ConvClassifier.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void AddSampleToTestOfEpoch(IntPtr hrlenet, [In]byte[] buffer, int size, int iclass);

        [DllImport("ConvClassifier.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void EndAndRunTrainEpoch(IntPtr hrlenet, ref TrainInfo trainInfo, int epochCount = 1);

        [DllImport("ConvClassifier.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void EndAndRunTestEpoch(IntPtr hrlenet, ref TrainInfo trainInfo);


        [DllImport("ConvClassifier.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void RecognizeRawData(IntPtr hrlenet, [In]byte[] buffer, int size, int height, int width,
            out int answer, out double rate);

        [DllImport("ConvClassifier.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void RecognizeImageFile(IntPtr hrlenet, string imageFilename, out int answer, out double rate);

        [DllImport("ConvClassifier.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ComputeOutputs(IntPtr hrlenet, [In]byte[] buffer, int size, int height, int width,
            [In, Out]int[] outclasses, [In, Out]double[] outenergies, out int outsize);

        [DllImport("ConvClassifier.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ComputeOutputsRaw(IntPtr hrlenet, [In]byte[] buffer, int size, int height, int width,
            [In, Out]int[] outclasses, [In, Out]double[] outenergies, out int outsize);


        [DllImport("ConvClassifier.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void TestIntCoTaskMemAlloc(out int size, out IntPtr ppArray);

        [DllImport("ConvClassifier.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void TestDoubleCoTaskMemAlloc(out int size, out IntPtr ppArray);

        #endregion // Dll Imports
    }
}
