
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DocumentClassification
{
    public class ManhattenComparer : IDocumentComparer
    {
        public double Compare(DocumentVector vector1, DocumentVector vector2)
        {
            double total = 0;

            for (int i = 0; i < vector1.Vector.Length; i++)
            {
                total += Math.Abs(vector1.Vector[i] - vector2.Vector[i]);
            }

            return -1*total;
        }

        public override string ToString()
        {
            return "Manhatten Distance";
        }
    }
}
