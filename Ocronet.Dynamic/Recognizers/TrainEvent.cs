using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ocronet.Dynamic.Recognizers
{
    public delegate void TrainEventHandler(object sender, TrainEventArgs e);

    public class TrainEventArgs : EventArgs
    {
        public string Description;  // e.g. Classifier name
        public int Round;           // current round
        public double Error;        // current error
        public int SuccessSamples;  // current success samples
        public int TotalSamples;    // total samples
        public double BestError;    // best selected error
        public TimeSpan TrainCycleDuration;
        public TimeSpan TestCycleDuration;

        public TrainEventArgs(int round, double error, int successSamples, int totalSamples, double bestError, string descr = "")
        {
            Round = round;
            Error = error;
            SuccessSamples = successSamples;
            TotalSamples = totalSamples;
            BestError = bestError;
            Description = descr;
        }

        public TrainEventArgs(int round, double error, int successSamples, int totalSamples, double bestError,
            TimeSpan trainDuration, TimeSpan testDuration, string descr = "")
            : this(round, error, successSamples, totalSamples, bestError, descr)
        {
            TrainCycleDuration = trainDuration;
            TestCycleDuration = testDuration;
        }
    }
}
