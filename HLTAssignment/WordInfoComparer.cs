using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace HLTAssignment
{
    class WordInfoComparer : IComparer<KeyValuePair<string,int>>
    {
        public int Compare(KeyValuePair<string,int> x, KeyValuePair<string,int> y)
        {
            return y.Value - x.Value;
        }
    }
}
