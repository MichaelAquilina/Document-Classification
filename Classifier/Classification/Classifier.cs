using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace DocumentClassification.Classification
{
    public abstract class Classifier
    {
        //documents currently in the classifier
        public List<LabeledDocumentVector> Documents;

        public Classifier()
        {
            this.Documents = new List<LabeledDocumentVector>();
        }

        public Classifier(List<LabeledDocumentVector> TrainingData )
        {
            this.Documents = TrainingData;
        }

        public String Add(DocumentVector Vector)
        {
            String Label = Classify(Vector);
            Documents.Add( new LabeledDocumentVector(Vector,Label) );
            return Label;
        }

        public abstract void Train(List<LabeledDocumentVector> TrainingData);

        public abstract String Classify(DocumentVector Vector);
    }
}
