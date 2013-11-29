using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DocumentClassification
{
    public class DocumentNode
    {
        public String DocumentName;
        public List<int> Occurrances;

        public int Count
        {
            get
            {
                return Occurrances.Count;
            }
        }

        public DocumentNode()
        {
        }

        public DocumentNode(String DocumentName)
        {
            this.DocumentName = DocumentName;
            Occurrances = new List<int>();
        }

        public override string ToString()
        {
            return "[ " + DocumentName + " = " + Count + " Occurances ]";
        }
    }
}
