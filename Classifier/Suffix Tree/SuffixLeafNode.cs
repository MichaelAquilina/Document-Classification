using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace DocumentClassification
{
    public class SuffixLeafNode
    {
        public int index;   //used for saving the suffix leaf node to the hard disk

        public Dictionary<string,int> DocumentNodeList = new Dictionary<string,int>();
        public int TotalTermFrequency = 0;

        public void AddDocumentIndex(DocumentIndex index, int weight )
        {

            if ( !HasDocument(index.DocumentName) )
                DocumentNodeList.Add(index.DocumentName, 0);

            DocumentNodeList[index.DocumentName]+=weight;
            TotalTermFrequency+=weight;        //used to calculate TFIDF in a quick manner
        }

        //adds the new item and sorts the array
        private void AddDocument(DocumentNode Node,int min, int max)
        {
            int pivot = (min - max) / 2;


        }

        //very primitive search, should be extended to binary search
        private bool HasDocument(String Name)
        {
            return DocumentNodeList.ContainsKey(Name);
        }
    }
}
