using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DocumentClassification
{
    public class CosineComparer : IDocumentComparer
    {
        public double Compare(DocumentVector x, DocumentVector y)
        {
            DocumentVector vector1 = (DocumentVector)x;
            DocumentVector vector2 = (DocumentVector)y;

            double total = 0;
            for (int i = 0; i < vector1.Vector.Length; i++)
                total += vector1.Vector[i] * vector2.Vector[i];

            double den = vector1.Vector.Size * vector2.Vector.Size;

            return (den==0)? 0 : total/den;
        }

        public override string ToString()
        {
            return "Cosine Similarity";
        }
    }
}
