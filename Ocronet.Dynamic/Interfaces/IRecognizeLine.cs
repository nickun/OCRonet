using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Ocronet.Dynamic.Interfaces
{
    /// <summary>
    /// A generic interface for text line recognition.
    /// </summary>
    public abstract class IRecognizeLine : IComponent, IDisposable
    {
        public override string Interface
        {
            get { return "IRecognizeLine"; }
        }


        /// <summary>
        /// Start training of the given type.
        /// </summary>
        /// <param name="type">
        /// "adaptation" means temporary adaptation of the classifier
        /// to all the lines between startTraining and finishTraining
        /// other types of training are recognizer-dependent
        /// </param>
        public virtual void StartTraining(string type = "adaptation")
        {
            throw new NotImplementedException("IRecognizeLine:StartTraining: unimplemented");
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
        public virtual void AddTrainingLine(Bytearray image, string transcription)
        {
            throw new NotImplementedException("IRecognizeLine:AddTrainingLine: unimplemented");
        }

        /// <summary>
        /// Train on a text line, given a segmentation.
        /// <remarks>This is analogous to addTrainingLine(bytearray,nustring) except that
        /// it takes the "ground truth" line segmentation.</remarks>
        /// </summary>
        public virtual bool AddTrainingLine(Intarray segmentation, Bytearray image_grayscale, string transcription)
        {
            throw new NotImplementedException("IRecognizeLine:AddTrainingLine: unimplemented");
        }

        /// <summary>
        /// Align a lattice with a transcription.
        /// </summary>
        /// <param name="chars">Characters along the best path.
        /// Currently, every character in chars must have a corresponding
        /// region in seg and the characters must be in reading order.
        /// Eventually, chars may contain characters (e.g., spaces) that
        /// do not correspond to any region.  Note that chars may not
        /// correspond to any string allowed/suggested by the transcription.</param>
        /// <param name="seg">Aligned segmentation, colors correspond to chars (starting at 1)</param>
        /// <param name="costs">Costs corresponding to chars</param>
        /// <param name="image">Input grayscale image</param>
        /// <param name="transcription">The "ground truth" lattice to align</param>
        public virtual void Align(string chars, Intarray seg, Floatarray costs,
                           Bytearray image, IGenericFst transcription)
        {
            throw new NotImplementedException("IRecognizeLine:Align: unimplemented");
        }

        /// <summary>
        /// Finish training, possibly making complex calculations.
        /// <remarks>Call this when training is done and the system should switch back to recognition;
        /// this method may take a long time to complete.</remarks>
        /// </summary>
        public virtual void FinishTraining() { throw new NotImplementedException("IRecognizeLine:FinishTraining: unimplemented"); }

        /// <summary>
        /// Notify the recognizer of the start of a new epoch
        /// (i.e., if n>0, then we have seen the data before).
        /// </summary>
        /// <param name="n"></param>
        public virtual void Epoch(int n) { }

        /// <summary>
        /// Recognize a text line and return a lattice representing
        /// the recognition alternatives.
        /// </summary>
        public abstract double RecognizeLine(IGenericFst result, Bytearray image);

        /// <summary>
        /// This is a weird, optional method that exposes character segmentation
        /// for those line recognizers that have it segmentation contains colored pixels,
        /// and a transition in the transducer of the form * --- 1/eps --> * --- 2/a --> *
        /// means that pixels with color 1 and 2 together form the letter "a"
        /// </summary>
        public virtual double RecognizeLine(Intarray segmentation, IGenericFst result, Bytearray image) 
        {
            throw new NotImplementedException("IRecognizeLine:RecognizeLine: unimplemented"); 
        }

        /// <summary>
        /// recognize a line with or without a given segmentation
        /// if useit is set to true, the given segmentation is just displayed in loggers, but not used,
        /// the segmenter computes the segmentation and the recognition uses its output
        /// if useit is set to false, the segmenter is still launched for the loggers, but the given
        /// segmentation is really used for the recognition
        /// </summary>
        public virtual double RecognizeLineSeg(IGenericFst result, Intarray segmentation, Bytearray image)
        {
            return this.RecognizeLine(segmentation, result, image);
        }

        /// <summary>
        /// Clean up here.
        /// </summary>
        public virtual void Dispose() { }
    }
}
