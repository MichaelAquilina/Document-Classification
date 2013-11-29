using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DocumentClassification.Classification
{
    public interface DNode
    {
        string Decision(DocumentVector vector);
    }
}
