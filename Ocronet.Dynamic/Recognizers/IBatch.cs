using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ocronet.Dynamic.IOData;
using Ocronet.Dynamic.Utils;
using Ocronet.Dynamic.Component;

namespace Ocronet.Dynamic.Recognizers
{
    public abstract class IBatch : IModel
    {
        // incremental training for batch models
        IExtDataset _ds;

        public IBatch()
        {
            PDef("cds", "rowdataset8", "default dataset buffer class");
        }

        public override string Name
        {
            get { return "IBatch"; }
        }

        public override string Interface
        {
            get { return "IBatch"; }
        }

        protected override void Train(IDataset dataset)
        {
            throw new NotImplementedException();
        }

        protected override void Add(Floatarray v, int c)
        {
            if (_ds == null)
            {
                Global.Debugf("info", "allocating {0} buffer for classifier", PGet("cds"));
                _ds = ComponentCreator.MakeComponent<IExtDataset>(PGet("cds"));
            }
            _ds.Add(v, c);
        }

        public override void UpdateModel()
        {
            if (_ds == null)
                return;
            Console.WriteLine();
            Global.Debugf("info", "UpdateModel {0} samples, {1} features, {2} classes",
                   _ds.nSamples(), _ds.nFeatures(), _ds.nClasses());
            Train(_ds);
            _ds = null;
        }
    }
}
