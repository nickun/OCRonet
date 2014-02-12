using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ocronet.Dynamic.Component
{
    public static class Init
    {
        /// <summary>
        /// Сопоставление псевдонимов именам класов,
        /// &lt;псевдоним, имя_класса&gt;
        /// </summary>
        public static Dictionary<string, string> ClassAliases = new Dictionary<string, string>();

        static Init()
        {
            // binarize
            ClassAliases.Add("BinarizeByRange", "BinarizeByRange");
            ClassAliases.Add("BinarizeByOtsu", "BinarizeByOtsu");
            ClassAliases.Add("BinarizeBySauvola", "BinarizeBySauvola");
            // segmenters
            ClassAliases.Add("DpSegmenter", "DpSegmenter");
            ClassAliases.Add("dpseg", "DpSegmenter");
            ClassAliases.Add("CurvedCutSegmenter", "CurvedCutSegmenter");
            ClassAliases.Add("CurvedCutSegmenter1", "CurvedCutSegmentLine");
            ClassAliases.Add("CurvedCutSegmentLine", "CurvedCutSegmentLine");
            ClassAliases.Add("CurvedCutWithCcSegmenter", "CurvedCutWithCcSegmenter");
            ClassAliases.Add("SkelSegmenter", "SkelSegmenter");
            ClassAliases.Add("SegmentLineByCCS", "SegmentLineByCCS");
            ClassAliases.Add("SegmentLineByGCCS", "SegmentLineByGCCS");
            // fst
            ClassAliases.Add("OcroFST", "OcroFSTImpl");
            // groupers
            ClassAliases.Add("SimpleGrouper", "SimpleGrouper");
            ClassAliases.Add("simplegrouper", "SimpleGrouper");
            ClassAliases.Add("StandardGrouper", "SimpleGrouper");
            // base classifiers
            ClassAliases.Add("AutoMlpClassifier", "AutoMlpClassifier");
            ClassAliases.Add("mlp", "AutoMlpClassifier");
            ClassAliases.Add("mappedmlp", "AutoMlpClassifier");
            // classifier combination
            ClassAliases.Add("LatinClassifier", "LatinClassifier");
            ClassAliases.Add("latin", "LatinClassifier");
            ClassAliases.Add("LenetClassifier", "LenetClassifier");
            ClassAliases.Add("lenet", "LenetClassifier");
            // feature extractors
            ClassAliases.Add("StandardExtractor", "StandardExtractor");
            ClassAliases.Add("ScaledImageExtractor", "ScaledImageExtractor");
            ClassAliases.Add("scaledfe", "ScaledImageExtractor");
            ClassAliases.Add("BiggestCcExtractor", "BiggestCcExtractor");
            ClassAliases.Add("biggestcc", "BiggestCcExtractor");
            ClassAliases.Add("RaveledExtractor", "RaveledExtractor");
            ClassAliases.Add("raveledfe", "RaveledExtractor");
            // other components
            ClassAliases.Add("RowDataset8", "RowDataset8");
            ClassAliases.Add("rowdataset8", "RowDataset8");
            ClassAliases.Add("RaggedDataset8", "RaggedDataset8");
            ClassAliases.Add("raggeddataset8", "RaggedDataset8");
            ClassAliases.Add("Dataset8", "Dataset8");
            ClassAliases.Add("dataset8", "Dataset8");
            ClassAliases.Add("BookStore", "BookStore");
            ClassAliases.Add("OldBookStore", "OldBookStore");
            ClassAliases.Add("SmartBookStore", "SmartBookStore");
            //ClassAliases.Add("SimpleFeatureMap", "SimpleFeatureMap");
            //ClassAliases.Add("sfmap", "SimpleFeatureMap");
            // line recognizers
            ClassAliases.Add("Linerec", "Linerec");
            ClassAliases.Add("linerec", "Linerec");
        }
    }
}
