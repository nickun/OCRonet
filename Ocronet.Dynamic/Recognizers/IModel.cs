using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ocronet.Dynamic.Interfaces;
using Ocronet.Dynamic.IOData;
using System.Reflection;
using Ocronet.Dynamic.Component;
using Ocronet.Dynamic.ImgLib;

namespace Ocronet.Dynamic.Recognizers
{
    public abstract class IModel : IComponent
    {
        public bool DisableJunk = false;
        protected ComponentContainerExtractor _extractor;
        public event TrainEventHandler TrainRound;

        public IModel()
        {
            _extractor = new ComponentContainerExtractor(null);
            PDef("extractor", "none", "feature extractor");
            Persist(_extractor, "extractor");
        }

        public override string Name
        {
	        get { return "IModel"; }
        }

        public override string Interface
        {
            get { return "IModel"; }
        }

        public virtual bool HigherOutputIsBetter
        {
            get { return true; }
        }

        public void OnTrainRound(object sender, TrainEventArgs args)
        {
            if (TrainRound != null)
                TrainRound(this, args);
        }


        /// <summary>
        /// Создает экземпляр класса, наследованного от IModel
        /// </summary>
        /// <param name="name">имя класа</param>
        public static IModel MakeModel(string name)
        {
            if (name.ToLower() == "none")
                return null;
            return ComponentCreator.MakeComponent<IModel>(name);
        }

        public virtual void SetExtractor(string name)
        {
            if (name.ToLower() == "none")
                _extractor.SetComponent(null);
            else
            {
                _extractor.SetComponent(ComponentCreator.MakeComponent(name));
            }
            PSet("extractor", name);
        }

        public IExtractor GetExtractor()
        {
            return _extractor.Object;
        }

        public void XAdd(Floatarray v, int c)
        {
            if (!_extractor.IsEmpty)
            {
                Floatarray temp = new Floatarray();
                _extractor.Object.Extract(temp, v);
                Add(temp, c);
            }
            else
                Add(v, c);
        }

        /// <summary>
        /// Run recognize
        /// </summary>
        /// <param name="ov">network output</param>
        /// <param name="v">values can be 0..1 or 0..255, see RequireUByteInput property</param>
        /// <returns></returns>
        public float XOutputs(OutputVector ov, Floatarray v)
        {
            if (!_extractor.IsEmpty)
            {
                Floatarray temp = new Floatarray();
                _extractor.Object.Extract(temp, v);
                return Outputs(ov, temp);
            }
            else
                return Outputs(ov, v);
        }

        public void XTrain(IDataset ds)
        {
            if(_extractor.IsEmpty) {
                Train(ds);
            } else {
                ExtractedDataset eds = new ExtractedDataset(ds, _extractor.Object);
                Train(eds);
            }
        }

        public virtual void UpdateModel()
        {
            throw new NotImplementedException();
        }


        protected virtual void Add(Floatarray v, int c)
        {
            throw new NotImplementedException();
        }

        protected virtual float Outputs(OutputVector ov, Floatarray temp)
        {
            throw new NotImplementedException();
        }

        protected virtual void Train(IDataset ds)
        {
            Floatarray v = new Floatarray();
            for (int i = 0; i < ds.nSamples(); i++)
            {
                ds.Input(v, i);
                Add(v, ds.Cls(i));
            }
        }

        #region special inquiry functions

        public virtual int nClasses()
        {
            return -1;
        }

        public virtual int nFeatures()
        {
            return -1;
        }

        public virtual void Copy(IModel model)
        {
            throw new NotImplementedException();
        }

        public virtual int nProtos()
        {
            return 0;
        }

        public virtual void GetProto(Floatarray v, int i, int variant)
        {
            throw new NotImplementedException();
        }

        public virtual int nModels()
        {
            return 0;
        }

        public virtual void SetModel(IModel model, int i)
        {
            throw new Exception(String.Format("[{0}.SetModel] no submodels", Name));
        }

        public virtual IComponent GetModel(int i)
        {
            throw new Exception(String.Format("[{0}.GetModel] no submodels", Name));
        }

        #endregion // special inquiry functions

        #region convenience functions

        public float Outputs(Floatarray p, Floatarray x)
        {
            OutputVector ov = new OutputVector();
            float cost = XOutputs(ov, x);
            p.Clear();
            p.Copy(ov.AsArray());
            return cost;
        }

        public int Classify(Floatarray v)
        {
            OutputVector p = new OutputVector();
            XOutputs(p, v);
            return p.ArgMax();
        }

        #endregion // convenience functions

    }
}
