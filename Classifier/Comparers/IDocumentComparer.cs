using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DocumentClassification
{
    public interface IDocumentComparer
    {
        double Compare( DocumentVector vector1, DocumentVector vector2);
    }
}
