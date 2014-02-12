using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ocronet.Dynamic.IOData
{
    public static class MnistDatasetConvert
    {
        public static RowDataset8 GetRowDataset8(MnistDatasource mds, int[] classes)
        {
            if (mds.NSamples() == 0)
                throw new Exception("MNIST database is empty!");

            // определим максимальный индекс
            byte maxLabel = 0;
            foreach (byte label in mds.Labels)
            {
                if (label > maxLabel)
                    maxLabel = label;
            }

            // проверим соответствие индексов Mnist указанному списку классов
            if (maxLabel >= classes.Length)
                throw new Exception("Classes do not correspond to the MNIST!");

            // создаем тренировочную базу
            RowDataset8 ds = new RowDataset8(mds.NSamples());
            // перебираем MNIST базу
            for (int i = 0; i < mds.NSamples(); i++)
            {
                int label = mds.Labels[i];
                StdInput stdInp = new StdInput(mds.ImagesData[i], mds.ImgHeight, mds.ImgWidth);
                ds.Add(stdInp.ToFloatarray(), classes[label]);
            }
            return ds;
        }
    }
}
