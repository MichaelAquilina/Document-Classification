using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DocumentClassification.Classification
{
    public class DLeafNode : DNode
    {
        public String Classification;

        public DLeafNode(String Classification)
        {
            this.Classification = Classification;
        }

        public string Decision(DocumentVector vector)
        {
            return Classification;
        }
    }
}
