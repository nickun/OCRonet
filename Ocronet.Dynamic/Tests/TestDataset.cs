using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ocronet.Dynamic.Debug;
using Ocronet.Dynamic.IOData;

namespace Ocronet.Dynamic.Tests
{
    public class TestDataset
    {
        string sClasses = "0123456789abcdef";
        int[] classes;
        Intarray aclasses;
        string mnistFileNamePrefix = "t10k";
        string dsExt = ".dsr8";

        public TestDataset()
        {
            classes = new int[sClasses.Length];
            aclasses = new Intarray(sClasses.Length);
            for (int i = 0; i < sClasses.Length; i++)
            {
                classes[i] = (int)sClasses[i];
                aclasses.Put1d(i, classes[i]);
            }
        }

        public void TestRowDataset()
        {
            DRandomizer.Default.init_drand(DateTime.Now.Millisecond);
            
            // load Mnist datasource
            MnistDatasource mds = new MnistDatasource();
            mds.LoadFromFile(mnistFileNamePrefix);

            // convert mnist to RowDataset8
            RowDataset8 ds8 = MnistDatasetConvert.GetRowDataset8(mds, classes);

            // show random sample to console
            Floatarray fa = new Floatarray();
            int isample = (int)DRandomizer.Default.drand(ds8.nSamples(), 0);
            ds8.Input(fa, isample);
            Console.WriteLine("Char is '{0}'", (char)ds8.Cls(isample));
            NarrayShow.ShowConsole(fa);

            // compare random float sample and original mnist
            StdInput inp1 = new StdInput(mds.ImagesData[isample], mds.ImgHeight, mds.ImgWidth);
            StdInput inp2 = new StdInput(fa);
            Console.WriteLine("Arrays is identical? {0}", Equals(inp1.GetDataBuffer(), inp2.GetDataBuffer()));

            // save RowDataset8 to file
            Console.WriteLine("Saving {0} samples..", ds8.nSamples());
            ds8.Save(mnistFileNamePrefix + dsExt);

            // load RowDataset8 from file
            RowDataset8 ds = new RowDataset8();
            ds.Load(mnistFileNamePrefix + dsExt);
            Console.WriteLine("Loaded {0} samples", ds.nSamples());
        }

        private bool Equals(byte[] b1, byte[] b2)
        {
            if (b1.Length != b2.Length)
                return false;
            for (int i = 0; i < b1.Length; i++)
                if (b1[i] != b2[i])
                    return false;
            return true;
        }

    }
}
