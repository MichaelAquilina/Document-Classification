using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DocumentClassification.Classification
{
    public class Winnow : Perceptron
    {
        protected override double Mult(VectorN v1, VectorN v2)
        {
            return Math.Exp(v1 * v2);
        }

        public override string ToString()
        {
            return "Winnow";
        }
    }
}
