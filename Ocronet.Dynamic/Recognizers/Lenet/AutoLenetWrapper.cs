using System;
using Ocronet.Dynamic.IOData;
using Ocronet.Dynamic.Utils;
using System.Diagnostics;

namespace Ocronet.Dynamic.Recognizers.Lenet
{
    public class AutoLenetWrapper : LenetWrapper
    {
        int netsize;
        double[] netstate;

        public override TrainInfo TrainBatch(IDataset ds, IDataset ts, int epochs)
        {
            // лучший резальтат на тестовой выборке
            float bestError = 1e30f;
            double bestEnergy = 1e30;
            // вначале надо бы запустить тест и уточнить его
            TrainInfo bestinfo = TestDense(ts);
            bestError = bestinfo.terror / (float)bestinfo.tsize;
            bestEnergy = bestinfo.tenergy;
            Global.Debugf("info", "     BEST errors={0:0.00#%} energy={1:0.#####}", bestError, bestEnergy);

            // сначала сохраним состояние нейросети
            SaveNetworkToBuffer(out netsize, out netstate);

            // запуск эпох тренинга
            for (int epoch = 0; epoch < epochs; epoch++)
            {
                Stopwatch swRound = Stopwatch.StartNew();
                // запустим тренинг
                TrainInfo trinfo = base.TrainBatch(ds, ts, 1);
                float err = trinfo.terror / (float)trinfo.tsize;
                if (err < bestError || (bestError == 0 && err == 0 && trinfo.tenergy < bestEnergy))
                {
                    bestError = err;
                    bestEnergy = trinfo.tenergy;
                    bestinfo = trinfo;
                    // пересохраним состояние улучшенной нейросети
                    SaveNetworkToBuffer(out netsize, out netstate);
                    Global.Debugf("info", "     ==>  best selected");
                }

                swRound.Stop();
                OnTrainRound(this, new TrainEventArgs(
                    epoch, trinfo.tenergy, trinfo.tcorrect, trinfo.tsize, bestEnergy, swRound.Elapsed, TimeSpan.Zero
                    ));
            }

            // восстановим состояние наилучшей сети
            LoadNetworkFromBuffer(netstate, netsize);
            return bestinfo;
        }
    }
}
