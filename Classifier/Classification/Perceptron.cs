using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DocumentClassification.Classification
{
    public class Perceptron : Classifier
    {
        //assuming b is 0
        public double LearningRate;
        public List<VectorN> W = new List<VectorN>();
        public Dictionary<string, int> LabelsDictionary = new Dictionary<string, int>();

        public Perceptron()
        {
            this.LearningRate = 1;
        }

        public Perceptron(double LearningRate)
        {
            this.LearningRate = LearningRate;
        }

        /// <summary>
        /// Adds a new w vector to the perceptron and initialises all 
        /// the values in the vector to 0
        /// </summary>
        /// <param name="size">size of the vector to add</param>
        private void AddNewLabel( int size )
        {
            VectorN w = new VectorN(size);
            for (int i = 0; i < w.Length; i++)
                w[i] = 0;

            W.Add(w);
        }

        /// <summary>
        /// Trains the classifier with an already labeled set of document vectors
        /// </summary>
        /// <param name="TrainingData">Input labeled document vectors</param>
        public override void Train(List<LabeledDocumentVector> TrainingData)
        {
            //build the labels index
            foreach (LabeledDocumentVector Document in TrainingData)
            {
                if (!LabelsDictionary.ContainsKey(Document.Classification))
                {
                    LabelsDictionary.Add(Document.Classification, W.Count);
                    AddNewLabel(Document.Document.Vector.Length);
                }
            }

            int mistakes;
            do
            {
                mistakes = 0;
                foreach (LabeledDocumentVector Document in TrainingData)
                {
                    //if string comparsin doesnt work, use the index to check values
                    string Prediction = Classify(Document.Document);
                    if (Prediction != Document.Classification)
                    {
                        mistakes++;
                        int index1 = LabelsDictionary[Document.Classification]; //desired result
                        int index2 = LabelsDictionary[Prediction];              //result of the perceptron

                        W[index1] = W[index1] + (LearningRate * Document.Document.Vector);
                        W[index2] = W[index2] - (LearningRate * Document.Document.Vector);
                    }
                }
            } while (mistakes > 0);
        }

        /// <summary>
        /// Multiplication technique to be used. Perceptron should use simple
        /// sclar vector multiplication while winnow and kernel methods should 
        /// use variants
        /// </summary>
        /// <param name="v1">First vector</param>
        /// <param name="v2">Second vector</param>
        /// <returns></returns>
        protected virtual double Mult(VectorN v1, VectorN v2)
        {
            return v1 * v2;
        }

        /// <summary>
        /// Takes an input document vector and returns the perceptrons
        /// guess on its classification using the training data provided
        /// </summary>
        /// <param name="Vector">Input document vector to classify</param>
        /// <returns>String label classification</returns>
        public override string Classify(DocumentVector Vector)
        {
            double Max = double.MinValue;
            string classification = null;

            foreach( KeyValuePair<string, int> pair in LabelsDictionary )
            {
                VectorN w = W[pair.Value];
                double result = Mult(w,Vector.Vector);

                if (result > Max)
                {
                    classification = pair.Key;
                    Max = result;
                }
            }

            return classification;
        }

        public override string ToString()
        {
            return "Perceptron";
        }
    }
}
