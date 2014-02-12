using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ocronet.Dynamic.Recognizers
{
    public interface ISpaceModel
    {
        /// <summary>
        /// Given a list of character recognition candidates and their
        /// classifications, and an image of the corresponding text line,
        /// compute a list of pairs of costs for putting/not putting a space
        /// after each of the candidate characters.
        /// </summary>
        List<List<float>> SpaceCosts(List<Candidate> candidates, Bytearray image);
    }
}
