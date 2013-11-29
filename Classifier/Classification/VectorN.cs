using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DocumentClassification
{
    //try changing this to a struct
    public class VectorN
    {
        private double[] values;

        public double Size
        {
            get
            {
                double total = 0;
                for (int i = 0; i < values.Length; i++)
                {
                    total += values[i] * values[i];
                }
                return Math.Sqrt(total);
            }
        }
        public int Length
        {
            get { return values.Length; }
        }
        public double this[int index]
        {
            get { return values[index]; }
            set { values[index] = value; }
        }

        public VectorN(int dimensions)
        {
            values = new double[dimensions];
        }

        public static double operator *(VectorN v1, VectorN v2 )
        {
            double total = 0;
            for (int i = 0; i < v1.Length; i++)
            {
                total += v1[i] * v2[i];
            }
            return total;
        }

        public static VectorN operator *(double scalar, VectorN vector)
        {
            VectorN output = new VectorN(vector.Length);
            for (int i = 0; i < output.Length; i++)
                output[i] = vector[i] * scalar;

            return output;
        }

        public static VectorN operator +(VectorN vector1, VectorN vector2)
        {
            VectorN output = new VectorN(vector1.Length);
            for (int i = 0; i < vector1.Length; i++)
                output[i] = vector1[i] + vector2[i];
            
            return output;
        }

        public static VectorN operator -(VectorN vector1, VectorN vector2)
        {
            return vector1 + (-1 * vector2);
        }
    }
}
