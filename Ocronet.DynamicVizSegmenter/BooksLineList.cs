using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Ocronet.Dynamic.Utils;

namespace DynamicVizSegmenter
{
    public class BookLineList : ObservableCollection<BookLine>
    {

        public BookLineList()
        {
            //Add(new BookLine(999, 888, 1000));
        }

        public void FromLineSource(LineSource lineSource)
        {
            foreach (var bline in lineSource)
            {
                bline.SetLineSource(lineSource);
                Add(bline);
            }
        }
    }
}
