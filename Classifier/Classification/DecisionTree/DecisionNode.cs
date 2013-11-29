using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DocumentClassification.Classification
{
    public class DecisionNode : DNode
    {
        public int Index;
        public double Value;
        public DNode LeftChild;
        public DNode RightChild;

        public DecisionNode( int Index, double Value )
        {
            this.Index = Index;
            this.Value = Value;
        }

        public DecisionNode(List<LabeledDocumentVector> TrainingData)
        {
            Dictionary<string, int> LabelCount = GetLabelCount(TrainingData);
            double Entropy = GetEntropy(LabelCount, TrainingData.Count );
            
            int features = TrainingData[0].Document.Vector.Length;
            int samples = 100;  //for 0 and 1 only, choose 1 sample

            int feature = -1;
            double max = double.MinValue;
            double value = -1;
            //cached values to prevent multiple calculations on the same object
            List<LabeledDocumentVector> LeftCandidateData = null;
            List<LabeledDocumentVector> RightCandidateData = null;
            double left_cand_entropy = -1;
            double right_cand_entropy = -1;

            //calculate maximum information gain
            for (int i = 0; i < features; i++)
            {
                for (int j = 1; j < samples; j++)
                {
                    double val = (double) j / (samples*10);
                    List<LabeledDocumentVector>[] Children = Filter(TrainingData, val, i);

                    List<LabeledDocumentVector> Left = Children[0];
                    List<LabeledDocumentVector> Right = Children[1];

                    double left_entropy = GetEntropy(Left);
                    double right_entropy = GetEntropy(Right);

                    double expect_entropy = (Left.Count / TrainingData.Count) * left_entropy +
                                            (Right.Count / TrainingData.Count) * right_entropy;

                    double info_gain = Entropy - expect_entropy;
                    if (info_gain > max)
                    {
                        max = info_gain;
                        feature = i;
                        value = val;
                        LeftCandidateData = Left;
                        RightCandidateData = Right;
                        left_cand_entropy = left_entropy;
                        right_cand_entropy = right_entropy;
                    }
                }
            }

            this.Index = feature;
            this.Value = value;

            if (left_cand_entropy > 0)
                LeftChild = new DecisionNode(LeftCandidateData);
            else
                LeftChild = new DLeafNode(LeftCandidateData[0].Classification);
            
            if (right_cand_entropy > 0)
                RightChild = new DecisionNode(RightCandidateData);
            else
                RightChild = new DLeafNode(RightCandidateData[0].Classification);
        }

        public string Decision(DocumentVector vector)
        {
            if (vector.Vector[Index] < Value)
                return LeftChild.Decision(vector);
            else
                return RightChild.Decision(vector);
        }

        private List<LabeledDocumentVector>[] Filter(List<LabeledDocumentVector> TrainingData, double value, int feature)
        {
            List<LabeledDocumentVector> Left = new List<LabeledDocumentVector>();
            List<LabeledDocumentVector> Right = new List<LabeledDocumentVector>();

            foreach (LabeledDocumentVector Document in TrainingData)
            {
                if (Document.Document.Vector[feature] < value)
                    Left.Add(Document);
                else
                    Right.Add(Document);
            }

            return new List<LabeledDocumentVector>[2]{ Left, Right };
        }

        private double GetEntropy(Dictionary<string, int> LabelCount, int Documents )
        {
            double total = 0;
            foreach( KeyValuePair<string,int> pair in LabelCount )
            {
                double p = (double) pair.Value/Documents;
                total += p * Math.Log(p);
            }

            return -1 * total;
        }

        private double GetEntropy( List<LabeledDocumentVector> Data )
        {
            Dictionary<string, int> LabelCount = GetLabelCount(Data);
            return GetEntropy(LabelCount, Data.Count);
        }

        private Dictionary<string, int> GetLabelCount(List<LabeledDocumentVector> Data)
        {
            Dictionary<string, int> LabelCount = new Dictionary<string, int>();

            for (int i = 0; i < Data.Count; i++)
            {
                if (!LabelCount.ContainsKey(Data[i].Classification))
                    LabelCount[Data[i].Classification] = 0;

                LabelCount[Data[i].Classification]++;
            }

            return LabelCount;
        }
    }
}
