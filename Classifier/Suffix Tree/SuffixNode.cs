using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DocumentClassification
{
    struct IntegerPair
    {
        public int X;
        public int Y;

        public IntegerPair(int X, int Y)
        {
            this.X = X;
            this.Y = Y;
        }
    }

    public partial class SuffixNode
    {
        #region Static Methods

        //returns whether the given character is accepted by suffix trees
        public static bool AcceptedCharacter(char c)
        {
            //the suffix tree is case insensitive so it should not care about wheter it is receiving an upper or lowercase
            return ( MapToIndex(Char.ToLower(c)) != -1 ) && c!='$';
        }

        //maps the given character to an index in the neighbours list
        public static int MapToIndex(char c)
        {
            if (c >= 'a' && c <= 'z')
                return (c - 'a');

            if (c >= '0' && c <= '9' )
                return (c - '0') + 26;

            if (c == '$')
                return 36;

            return -1;
        }

        #endregion

        #region Global Variables

        public int index;  //index value used for when saving the suffix tree to a file

        public SuffixNode[] Neighbours = new SuffixNode[36];    //26 for a-z, 10 for 0-9, 1 for $
        public SuffixLeafNode LeafNode;

        public char Value;

        //returns the neighbour count of this node, including any leaf node
        public int NeighbourCount
        {
            get
            {
                int total = (LeafNode==null)? 0 : 1;
                for (int i = 0; i < Neighbours.Length; i++)
                    total += (Neighbours[i] == null) ? 0 : 1;
                return total;
            }
        }

        //returns the count of all the nodes (excluding leaf nodes) (recursive)
        public int NodeCount
        {
            get
            {
                int total = 1;
                foreach (SuffixNode node in Neighbours)
                {
                    if (node != null)
                        total += node.NodeCount;
                }

                return total;
            }
        }

        //returns the count of all leaf nodes (recursive)
        public int LeafNodeCount
        {
            get
            {
                int total = (LeafNode != null )? 1 : 0;
                foreach(SuffixNode node in Neighbours )
                {
                    if( node!=null )
                        total += node.LeafNodeCount;
                }

                return total;
            }
        }

        #endregion

        #region Constructors

        //constructs a new suffix node with a given value
        public SuffixNode(char value)
        {
            this.Value = value;
        }

        //constructs a new suffix node with no specific value
        public SuffixNode()
        {
            this.Value = ' ';
        }

        #endregion

        #region Public Methods

        //removes a word and all its information from the suffix tree
        public void Remove(String word)
        {
            if (word.Length == 0)
            {
                LeafNode = null;
                return;
            }

            char c = Char.ToLower(word[0]);

            SuffixNode Node = Neighbours[MapToIndex(c)];
            if (Node == null)
                return;

            Node.Remove(word.Substring(1));     //continue trimming
        }

        //adds a word without any document specific information
        public void Add(String word)
        {
            Add(word, null, 1);
        }

        //adds a word with document specific information
        public void Add(String word, DocumentIndex? index, int weight )
        {
            if (word.Length == 0)
            {
                if (LeafNode == null)
                    LeafNode = new SuffixLeafNode();

                if (index != null)
                    LeafNode.AddDocumentIndex(index.Value,weight);
                return;
            }

            char c = Char.ToLower(word[0]);

            SuffixNode node = Neighbours[SuffixNode.MapToIndex(c)];

            if (node == null)
            {
                Neighbours[SuffixNode.MapToIndex(c)] = new SuffixNode(c);
                node = Neighbours[SuffixNode.MapToIndex(c)];
            }

            node.Add(word.Substring(1),index,weight);
        }

        //returns each words and its document frequency
        public Dictionary<String, int> GetWordInfo( String word )
        {
            Dictionary<String, int> Output = new Dictionary<string, int>();

            if (LeafNode != null)
                Output.Add(word, LeafNode.DocumentNodeList.Count);

            for (int i = 0; i < Neighbours.Length; i++)
            {
                if (Neighbours[i] != null)
                {
                    Dictionary<String, int> Result = Neighbours[i].GetWordInfo(word + Neighbours[i].Value);
                    foreach (KeyValuePair<String, int> pair in Result)
                        Output.Add(pair.Key, pair.Value);
                }
            }

            return Output;
        }

        //gets all words in the tree as a list of simple strings
        public List<String> GetAllWords( String word )
        {
            List<String> words = new List<String>();

            if (LeafNode != null)
                words.Add(word);

            for (int i = 0; i < Neighbours.Length; i++)
            {
                if (Neighbours[i] != null)
                    words.AddRange(Neighbours[i].GetAllWords( word + Neighbours[i].Value));
            }

            return words;
        }

        //gets all inner words of the range inputted
        public List<String> GetInnerWords(int MinDocFrequency, int MaxDocFrequency, String word)
        {
            List<String> words = new List<String>();

            if (LeafNode != null)
            {
                if (LeafNode.DocumentNodeList.Count >= MinDocFrequency &&
                    LeafNode.DocumentNodeList.Count <= MaxDocFrequency)
                    words.Add(word);
            }

            for (int i = 0; i < Neighbours.Length; i++)
            {
                if (Neighbours[i] != null)
                    words.AddRange(Neighbours[i].GetInnerWords(MinDocFrequency, MaxDocFrequency, word + Neighbours[i].Value));
            }

            return words;
        }

        //gets all outer words of the range inputted
        public List<String> GetOuterWords(int MinDocFrequency, int MaxDocFrequency, String word)
        {
            List<String> words = new List<String>();

            if (LeafNode != null)
            {
                if( LeafNode.DocumentNodeList.Count < MinDocFrequency ||
                    LeafNode.DocumentNodeList.Count > MaxDocFrequency      )
                        words.Add(word);
            }

            for (int i = 0; i < Neighbours.Length; i++)
            {
                if( Neighbours[i] != null )
                    words.AddRange(Neighbours[i].GetOuterWords(MinDocFrequency,MaxDocFrequency, word+Neighbours[i].Value));
            }

            return words;
        }

        //trims all the words which our outside the range specified
        public List<String> TrimWords(int MinDocumentFrequency, int MaxDocumentFrequency)
        {
            List<String> Stopwords = GetOuterWords(MinDocumentFrequency, MaxDocumentFrequency, "");

            foreach (String word in Stopwords)
                Remove(word);

            return Stopwords;
        }

        //searches for the given word in the suffix tree and returns any information found
        public Dictionary<string,int> GetWord(String Word)
        {
            if (Word.Length == 0)
                return (LeafNode==null)? null : LeafNode.DocumentNodeList;

            char c = Char.ToLower(Word[0]);

            SuffixNode node = Neighbours[SuffixNode.MapToIndex(c)];

            if (node == null)
                return null;
            else
                return node.GetWord(Word.Substring(1));
        }

        //searches for the given word and returns whether the search was succesful
        public bool HasWord(String Word)
        {
            return GetWord(Word) != null;
        }

        //clears all the neighbours for this given suffix node
        public void ClearNeigbours()
        {
            for (int i = 0; i < Neighbours.Length; i++)
                Neighbours[i] = null;       //any attatched object will be cleaned up by the garbage collecter

            LeafNode = null;
        }

        #endregion
    }
}
