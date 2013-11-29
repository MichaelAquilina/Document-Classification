using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DocumentClassification.Classification
{
    public class DecisionTree : Classifier
    {
        private DecisionNode RootNode;

        public DecisionTree()
        {          
        }

        public override void Train(List<LabeledDocumentVector> TrainingData)
        {
            RootNode = new DecisionNode(TrainingData);
        }

        public override string Classify(DocumentVector Vector)
        {
            return RootNode.Decision(Vector);
        }

        public override string ToString()
        {
            return "Decision Tree";
        }
    }
}
