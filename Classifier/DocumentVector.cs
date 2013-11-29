using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DocumentClassification
{
    public class DocumentVector
    {
        private VectorN vector;
        private String headLine;
        private String dateLine;
        private String id;
        private long location;

        public VectorN Vector
        {
            get { return vector; }
            set { vector = value; }
        }
        public String HeadLine
        {
            get { return headLine; }
            set { headLine = value; }
        }
        public String DateLine
        {
            get { return dateLine; }
            set { dateLine = value; }
        }
        public String Id
        {
            get { return id; }
            set { id = value; }
        }
        public long Location
        {
            get { return location; }
            set { location = value; }
        }

        public DocumentVector(String id, String HeadLine, String DateLine, long Location)
        {
            this.Id = id;
            this.HeadLine = HeadLine;
            this.DateLine = DateLine;
            this.Location = Location;
        }

        public DocumentVector( String id, String HeadLine, String DateLine )
        {
            this.Id = id;
            this.HeadLine = HeadLine;
            this.DateLine = DateLine;
        }

        public override string ToString()
        {
            return Id + ": " + HeadLine;
        }
    }
}
