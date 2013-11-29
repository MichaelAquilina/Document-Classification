using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace DocumentClassification
{
    public class EuclidianComparer : IDocumentComparer
    {
        public double Compare(DocumentVector x, DocumentVector y)
        {
            DocumentVector vector1 = (DocumentVector)x;
            DocumentVector vector2 = (DocumentVector)y;
            double total = 0;

            for (int i = 0; i < vector1.Vector.Length; i++)
                total += Math.Pow(vector1.Vector[i] - vector2.Vector[i], 2);

            return -1*Math.Sqrt(total);
        }

        public override string ToString()
        {
            return "Euclidian Distance";
        }
    }
}
