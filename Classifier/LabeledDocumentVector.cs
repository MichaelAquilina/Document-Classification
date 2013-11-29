using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DocumentClassification
{
    public class LabeledDocumentVector
    {
        private String classification;
        private DocumentVector document;

        public String Classification
        {
            get { return classification; }
            set { classification = value; }
        }
        public DocumentVector Document
        {
            get { return document; }
            set { document = value; }
        }

        public LabeledDocumentVector(DocumentVector Document, String Classification)
        {
            this.Document = Document;
            this.Classification = Classification;
        }

        public override string ToString()
        {
            return "[" + Classification + "] " + Document;
        }
    }
}
