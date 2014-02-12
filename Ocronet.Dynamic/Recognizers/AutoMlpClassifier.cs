using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ocronet.Dynamic.IOData;
using Ocronet.Dynamic.Utils;
using System.Diagnostics;

namespace Ocronet.Dynamic.Recognizers
{
    /// <summary>
    /// MLP classifier with automatic rate adaptation, cross
    /// validation and parallel training
    /// </summary>
    public class AutoMlpClassifier : MlpClassifier
    {
        public AutoMlpClassifier()
        {
            PDef("parallel", 1, "Allows to parallel loop's iteration train in separate threads");
        }

        public override void TrainDense(IDataset ds)
        {
            //PSet("%nsamples", ds.nSamples());
            float split = PGetf("cv_split");
            int mlp_cv_max = PGeti("cv_max");
            if (crossvalidate)
            {
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
                Global.Debugf("cvdetail", "#training {0} #testing {1}",
                       training.Length(), testing.Length());
                PSet("%ntraining", training.Length());
                PSet("%ntesting", testing.Length());
                Datasubset trs = new Datasubset(ds, training);
                Datasubset tss = new Datasubset(ds, testing);
                TrainBatch(trs, tss);
            }
            else
            {
                TrainBatch(ds, ds);
            }
        }

        public virtual void TrainBatch(IDataset ds, IDataset ts)
        {
            Stopwatch sw = Stopwatch.StartNew();
            bool parallel = PGetb("parallel");
            float eta_init = PGetf("eta_init"); // 0.5
            float eta_varlog = PGetf("eta_varlog"); // 1.5
            float hidden_varlog = PGetf("hidden_varlog"); // 1.2
            int hidden_lo = PGeti("hidden_lo");
            int hidden_hi = PGeti("hidden_hi");
            int rounds = PGeti("rounds");
            int mlp_noopt = PGeti("noopt");
            int hidden_min = PGeti("hidden_min");
            int hidden_max = PGeti("hidden_max");
            CHECK_ARG(hidden_min > 1 && hidden_max < 1000000, "hidden_min > 1 && hidden_max < 1000000");
            CHECK_ARG(hidden_hi >= hidden_lo, "hidden_hi >= hidden_lo");
            CHECK_ARG(hidden_max >= hidden_min, "hidden_max >= hidden_min");
            CHECK_ARG(hidden_lo >= hidden_min && hidden_hi <= hidden_max, "hidden_lo >= hidden_min && hidden_hi <= hidden_max");
            int nn = PGeti("nensemble");
            ObjList<MlpClassifier> nets = new ObjList<MlpClassifier>();
            nets.Resize(nn);
            for (int i = 0; i < nn; i++)
                nets[i] = new MlpClassifier(i);
            Floatarray errs = new Floatarray(nn);
            Floatarray etas = new Floatarray(nn);
            Intarray index = new Intarray();
            float best = 1e30f;
            if (PExists("%error"))
                best = PGetf("%error");
            int nclasses = ds.nClasses();

            /*Floatarray v = new Floatarray();
            for (int i = 0; i < ds.nSamples(); i++)
            {
                ds.Input1d(v, i);
                CHECK_ARG(NarrayUtil.Min(v) > -100 && NarrayUtil.Max(v) < 100, "min(v)>-100 && max(v)<100");
            }*/
            CHECK_ARG(ds.nSamples() >= 10 && ds.nSamples() < 100000000, "ds.nSamples() >= 10 && ds.nSamples() < 100000000");

            for (int i = 0; i < nn; i++)
            {
                // nets(i).init(data.dim(1),logspace(i,nn,hidden_lo,hidden_hi),nclasses);
                if (w1.Length() > 0)
                {
                    nets[i].Copy(this);
                    etas[i] = ClassifierUtil.rLogNormal(eta_init, eta_varlog);
                }
                else
                {
                    nets[i].InitData(ds, (int)(logspace(i, nn, hidden_lo, hidden_hi)), c2i, i2c);
                    etas[i] = PGetf("eta");
                }
            }
            etas[0] = PGetf("eta");     // zero position is identical to itself

            Global.Debugf("info", "mlp training n {0} nc {1}", ds.nSamples(), nclasses);
            for (int round = 0; round < rounds; round++)
            {
                Stopwatch swRound = Stopwatch.StartNew();
                errs.Fill(-1);
                if (parallel)   
                {
                    // For each network i
                    Parallel.For(0, nn, i =>
                    {
                        nets[i].PSet("eta", etas[i]);
                        nets[i].TrainDense(ds);     // было XTrain
                        errs[i] = ClassifierUtil.estimate_errors(nets[i], ts);
                    });
                }
                else
                {
                    for (int i = 0; i < nn; i++)
                    {
                        nets[i].PSet("eta", etas[i]);
                        nets[i].TrainDense(ds);     // было XTrain
                        errs[i] = ClassifierUtil.estimate_errors(nets[i], ts);
                        //Global.Debugf("detail", "net({0}) {1} {2} {3}", i,
                        //       errs[i], nets[i].Complexity(), etas[i]);
                    }
                }
                NarrayUtil.Quicksort(index, errs);
                if (errs[index[0]] < best)
                {
                    best = errs[index[0]];
                    cv_error = best;
                    this.Copy(nets[index[0]]);
                    this.PSet("eta", etas[index[0]]);
                    Global.Debugf("info", "  best mlp[{0}] update errors={1} {2}", index[0], best, crossvalidate ? "cv" : "");
                }
                if (mlp_noopt == 0)
                {
                    for (int i = 0; i < nn / 2; i++)
                    {
                        int j = i + nn / 2;
                        nets[index[j]].Copy(nets[index[i]]);
                        int n = nets[index[j]].nHidden();
                        int nm = Math.Min(Math.Max(hidden_min, (int)(ClassifierUtil.rLogNormal(n, hidden_varlog))), hidden_max);
                        nets[index[j]].ChangeHidden(nm);
                        etas[index[j]] = ClassifierUtil.rLogNormal(etas[index[i]], eta_varlog);
                    }
                }
                Global.Debugf("info", " end mlp round {0} err {1} nHidden {2}", round, best, nHidden());
                swRound.Stop();
                int totalTest= ts.nSamples();
                int errCnt = Convert.ToInt32(best * totalTest);
                OnTrainRound(this, new TrainEventArgs(
                    round, best, totalTest - errCnt, totalTest, best, swRound.Elapsed, TimeSpan.Zero
                    ));
            }

            sw.Stop();
            Global.Debugf("info", String.Format("          training time: {0} minutes, {1} seconds",
                (int)sw.Elapsed.TotalMinutes, sw.Elapsed.Seconds));
            PSet("%error", best);
            int nsamples = ds.nSamples() * rounds;
            if (PExists("%nsamples"))
                nsamples += PGeti("%nsamples");
            PSet("%nsamples", nsamples);
        }

    }
}
