using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using DocumentClassification.Classification;

namespace DocumentClassification
{
    public delegate void CompletedEventHandler( object sender );
    public delegate void IterationEventHandler( object sender );

    public class DocumentClassifier
    {
        #region Global Variables

        /// <summary>
        /// Classification data and objects which provide information
        /// about how the classification process should be performed
        /// </summary>
        public Classifier Classifier;                       //type of classifier to use
        public List<String> Features;                       //features to be used by the classifier
        public List<LabeledDocumentVector> TrainingData;    //training data to use

        /// <summary>
        /// Index trees used for quick access and writing to 
        /// important data in classification
        /// </summary>
        public SgmlParser Parser;
        public SuffixNode StopWordsIndex;
        public SuffixNode InvertedIndex;

        /// <summary>
        /// Meta data about words in the corpus when it is being
        /// parsed and weighted
        /// </summary>
        public Hashtable WordTable = new Hashtable();
        public Hashtable Documents = new Hashtable();
        public Hashtable DocumentLengths = new Hashtable();

        /// <summary>
        /// Diagnostics about the amount of time each method
        /// took during classification
        /// </summary>
        private Stopwatch parseWatch = new Stopwatch();
        private Stopwatch featureSelectionWatch = new Stopwatch();
        private Stopwatch classificationWatch = new Stopwatch();
        private Stopwatch featureWeightingWatch = new Stopwatch();
        private Stopwatch invertedIndexWatch = new Stopwatch();

        public TimeSpan TotalTime
        {
            get
            {
                return ParseTime +
                       FeatureSelectionTime +
                       FeatureWeightingTime +
                       ClassificationTime;
            }
        }
        public TimeSpan ParseTime
        {
            get { return parseWatch.Elapsed; }
        }
        public TimeSpan FeatureSelectionTime
        {
            get { return featureSelectionWatch.Elapsed; }
        }
        public TimeSpan ClassificationTime
        {
            get { return classificationWatch.Elapsed; }
        }
        public TimeSpan FeatureWeightingTime
        {
            get { return featureWeightingWatch.Elapsed; }
        }
        public TimeSpan InvertedIndexTime
        {
            get{ return invertedIndexWatch.Elapsed; }
        }

        /// <summary>
        /// Event handlers for users to plug methods into
        /// when coding their user interfaces
        /// </summary>
        public event CompletedEventHandler ParseCompleted;
        public event CompletedEventHandler FeatureSelectionCompleted;
        public event CompletedEventHandler FeatureWeightingCompleted;
        public event CompletedEventHandler ClassifyCompleted;

        public event IterationEventHandler ParseIteration;

        #endregion

        public DocumentClassifier()
        {
            BuildStopWordsIndex();
        }

        public void ParseAsync(String DocumentPath)
        {
            ThreadStart ts = delegate() { ParseThreaded(DocumentPath); };
            Thread thread = new Thread(ts);
            thread.Start();
        }

        public void FeatureSelectionAysnc(double min, double max)
        {
            ThreadStart ts = delegate() { FeatureSelectionThreaded(min, max); };
            Thread thread = new Thread(ts);
            thread.Start();
        }

        public void FeatureWeightingAsync()
        {
            ThreadStart ts = delegate() { FeatureWeightingThreaded(); };
            Thread thread = new Thread(ts);
            thread.Start();
        }

        public void ClassifyAsync( Classifier classifier )
        {
            ThreadStart ts = delegate() { ClassifyThreaded( classifier ); };
            Thread thread = new Thread(ts);
            thread.Start();
        }

        #region Threaded Methods

        private void ParseThreaded(String DocumentPath)
        {
            parseWatch.Reset();
            parseWatch.Start();

            Parse(DocumentPath);

            parseWatch.Stop();

            if (ParseCompleted != null)
                ParseCompleted(this);
        }

        private void FeatureSelectionThreaded(double min, double max)
        {
            featureSelectionWatch.Reset();
            featureSelectionWatch.Start();

            FeatureSelection(min, max);

            featureSelectionWatch.Stop();

            if (FeatureSelectionCompleted != null)
                FeatureSelectionCompleted(this);
        }

        private void FeatureWeightingThreaded()
        {
            featureWeightingWatch.Reset();
            featureWeightingWatch.Start();

            TraverseInvertedIndex(InvertedIndex, "");

            featureWeightingWatch.Stop();

            if (FeatureWeightingCompleted != null)
                FeatureWeightingCompleted(this);
        }

        private void ClassifyThreaded( Classifier classifier )
        {
            classificationWatch.Reset();
            classificationWatch.Start();

            Classify(classifier);

            classificationWatch.Stop();

            if (ClassifyCompleted != null)
                ClassifyCompleted(this);
        }

        #endregion

        #region Private Methods

        //parses the given corpus and places it in the inverted index
        private void Parse( String DocumentPath )
        {
            InvertedIndex = new SuffixNode(' ');
            invertedIndexWatch.Reset();

            StringBuilder builder = new StringBuilder();
            Parser = new SgmlParser(DocumentPath);
            String value;
            String prevDocId = "";

            while ((value = Parser.Next()) != null)
            {
                if (!StopWordsIndex.HasWord(value) && !isNumber(value) )
                {
                    value = Stem(value);

                    int weight = (isCapital(value[0]))? 2 : 1;

                    invertedIndexWatch.Start();
                    InvertedIndex.Add(value, new DocumentIndex(Parser.DocID, 0),weight);
                    invertedIndexWatch.Stop();
                }

                if (!Documents.Contains(Parser.DocID))
                {
                    Documents[Parser.DocID] = new DocumentVector(Parser.DocID, Parser.HeadLine, Parser.DateLine,Parser.DocumentPosition);
                    DocumentLengths[Parser.DocID] = 0;
                }

                DocumentLengths[Parser.DocID] = ((int)DocumentLengths[Parser.DocID]) + 1;
                
                //fire an event out to any attatched methods
                if ( prevDocId != Parser.DocID && ParseIteration != null)
                    ParseIteration(this);

                prevDocId = Parser.DocID;
            }
            Parser.Close();
        }

        //classifies the extracted documents
        private void Classify( Classifier classifier )
        {
            this.Classifier = classifier;

            foreach (String Key in Documents.Keys)
                Classifier.Add((DocumentVector)Documents[Key]);
        }

        //performs feature selection on the inverted index
        private void FeatureSelection( double min, double max )
        {
            //trim index to include only words with a document frequency between 10-80% that of the document count
            InvertedIndex.TrimWords((int)(min * Documents.Keys.Count), (int)(max * Documents.Keys.Count));
            Features = InvertedIndex.GetAllWords("");

            //initialise the document vector sizes
            foreach (String Key in Documents.Keys)
            {
                DocumentVector document = (DocumentVector)Documents[Key];
                document.Vector = new VectorN(Features.Count);
            }
        }

        //perform feature weighting by traversing the inverted index
        private void TraverseInvertedIndex(SuffixNode Node, String word )
        {
            if (Node.LeafNode != null)
            {
                foreach (KeyValuePair<string,int> pair in Node.LeafNode.DocumentNodeList)
                {
                    DocumentVector document = (DocumentVector)Documents[pair.Key];
                    int index = Features.IndexOf(word);
                    document.Vector[index] = TFIDF( pair.Value, 
                                                    Node.LeafNode.DocumentNodeList.Count,
                                                    (int) DocumentLengths[pair.Key], 
                                                    Documents.Count     );  //tfidf weighting
                }
            }

            for (int i = 0; i < Node.Neighbours.Length; i++)
            {
                if (Node.Neighbours[i] != null)
                    TraverseInvertedIndex(Node.Neighbours[i], word + Node.Neighbours[i].Value );
            }
        }

        //calculate tfidf value with given input parameters
        private double TFIDF(int TermFrequency, int DocumentFrequency, int NormalizationValue, int NumberOfDocuments)
        {
            //it is important to specify that a divide is a double otherwise the compiler will assume its an integer since the paramaters are integers
            return ((double)TermFrequency / NormalizationValue) * Math.Log(((double)NumberOfDocuments / DocumentFrequency), 2);
        }

        //perform very weak stemming on a word
        private String Stem(String word)
        {
            //if (word.EndsWith("es") && word.Length > 2)
            //    return word.Remove(word.Length - 2);

            //if (word.EndsWith("ed") && word.Length > 2)
            //    return word.Remove(word.Length - 2);

            if (word.EndsWith("s") && word.Length > 1)
                return word.Remove(word.Length - 1);

            return word;
        }

        //returns if the string is a complete number or not
        private Boolean isNumber(String word)
        {
            foreach (char c in word)
            {
                if (c < '0' || c > '9')
                    return false;
            }
            return true;
        }

        private Boolean isCapital(char c)
        {
            return (c >= 'A' && c <= 'Z');
        }

        //builds a stopword index from the given items in stopwords.txt
        private void BuildStopWordsIndex()
        {
            StopWordsIndex = new SuffixNode(' ');

            FileStream stream = new FileStream("stop_words.txt", FileMode.Open);
            int nobytes;
            byte[] buffer = new byte[4096];
            StringBuilder StopWordStrings = new StringBuilder();
            int words = 0;

            while ((nobytes = stream.Read(buffer, 0, 4096)) > 0)
            {
                StopWordStrings.Append(Encoding.UTF8.GetString(buffer, 0, nobytes));
            }

            String word = "";
            String list = StopWordStrings.ToString();
            foreach (char c in list)
            {
                if (c == '\n')
                {
                    if (word.Length > 0)
                    {
                        StopWordsIndex.Add(word);
                        words++;
                        word = "";
                    }
                }
                else
                    if (SuffixNode.AcceptedCharacter(c))
                        word += c;
            }
            if (word.Length > 0) StopWordsIndex.Add(word);

            stream.Close();
        }

        #endregion
    }
}
