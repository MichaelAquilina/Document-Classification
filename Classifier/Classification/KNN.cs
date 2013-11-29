using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace DocumentClassification.Classification
{
    //K Nearest Neighbours Algorithm
    public class KNN : Classifier
    {
        public int K;
        public List<LabeledDocumentVector> TrainingData;
        public IDocumentComparer Comparer;

        public KNN()
        {
            K = 1;
        }

        public KNN(List<LabeledDocumentVector> Documents, IDocumentComparer Comparer, int K )
            : base(Documents)
        {
            this.K = K;
            this.Comparer = Comparer;
        }

        public override void Train(List<LabeledDocumentVector> TrainingData)
        {
            this.TrainingData = TrainingData;
        }

        //optimised bubble sort
        //has O(Kn) time, ie linear O(n)
        private List<KeyValuePair<LabeledDocumentVector,double>> Sort( DocumentVector item )
        {
            //first calculate comparasin values
            List<KeyValuePair<LabeledDocumentVector,double>> Values = new List<KeyValuePair<LabeledDocumentVector,double>>();
            for (int i = 0; i < TrainingData.Count; i++)
            {
                Values.Add( new KeyValuePair<LabeledDocumentVector,double>(
                                TrainingData[i], Comparer.Compare(TrainingData[i].Document, item)));
            }

            int j = 0;
            bool flag = false;

            do
            {
                j++;
                flag = false;
                for (int i = 0; i < Values.Count - j; i++)
                {
                    if ( Values[i].Value > Values[i+1].Value  )
                    {
                        KeyValuePair<LabeledDocumentVector,double> dummy = Values[i];
                        Values[i] = Values[i + 1];
                        Values[i + 1] = dummy;
                        flag = true;
                    }
                }

            } while (flag && j<K);
            
            return Values;
        }

        public override string Classify(DocumentVector Vector)
        {
            List<KeyValuePair<LabeledDocumentVector,double>> Sorted = Sort(Vector);

            Dictionary<String, double> LabelCount = new Dictionary<string, double>();
            int startIndex = Sorted.Count - 1;

            //should compare to K of the labeled documents!
            for (int i = 0; i < K; i++)
            {
                String Classification = Sorted[startIndex - i].Key.Classification;
                if (!LabelCount.ContainsKey(Classification))
                    LabelCount.Add(Classification, 0);
                
                //improvement over the standard count model (add the similarity measures)
                LabelCount[Classification]++;
            }

            double Max = Int32.MinValue;
            String Output = null;
            foreach (KeyValuePair<String, double> pair in LabelCount)
            {
                if (pair.Value > Max)
                {
                    Max = pair.Value;
                    Output = pair.Key;
                }
            }

            return Output;
        }

        public override string ToString()
        {
            return "K Nearest Neighbours";
        }
    }
}
