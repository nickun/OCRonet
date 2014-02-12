using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ocronet.Dynamic.OcroFST
{
    public class FstFactory
    {
        public static CompositionFst MakeCompositionFst(OcroFST l1,
                                        OcroFST l2,
                                        int override_start = -1,
                                        int override_finish = -1)
        {
            return new CompositionFstImpl(l1, l2,
                                      override_start, override_finish);
        }

        public static OcroFST MakeOcroFST()
        {
            return new OcroFSTImpl();
        }
    }
}
