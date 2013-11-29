using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DocumentClassification
{
    public struct DocumentIndex
    {
        public String DocumentName;
        public int Location;

        public DocumentIndex(String DocumentName, int Location)
        {
            this.DocumentName = DocumentName;
            this.Location = Location;
        }

        public override string ToString()
        {
            return "[ " + DocumentName + " , " + Location + " ]";
        }
    }
}
