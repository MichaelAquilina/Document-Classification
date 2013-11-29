using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DocumentClassification;
using System.Threading;
using System.Windows.Threading;
using DocumentClassification.Classification;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;

namespace HLTAssignment
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Global Variables

        DocumentClassifier DocClassifier;

        KNN KNN_Classifier;
        Perceptron Perceptron_Classifier;
        DecisionTree DecisionTree_Classifier;
        Winnow Winnow_Classifier;

        Classifier SelectedClassifier;
        String CorpusPath
        {
            get { return CorpusTextBox.Text; }
        }
        String TrainingDataPath;
        double MinRange = 0.1;
        double MaxRange = 0.8;

        List<LabeledDocumentVector> TrainingData;

        #endregion

        public MainWindow()
        {
            InitializeComponent();
            
            GUITextWriter writer = new GUITextWriter(OutputTextBox);
            Console.SetOut(writer);

            DocClassifier = new DocumentClassifier();
            DocClassifier.ParseCompleted += Classifier_ParseCompleted;
            DocClassifier.FeatureSelectionCompleted += Classifier_FeatureSelectionCompleted;
            DocClassifier.FeatureWeightingCompleted += Classifier_FeatureWeightingCompleted;
            DocClassifier.ClassifyCompleted += Classifier_ClassifyCompleted;

            DocClassifier.ParseIteration += DocClassifier_ParseIteration;

            KNN_Classifier = new KNN();
            Perceptron_Classifier = new Perceptron();
            Winnow_Classifier = new Winnow();
            DecisionTree_Classifier = new DecisionTree();

            ClassifierComboBox.ItemsSource = new Classifier[] { KNN_Classifier, Perceptron_Classifier, Winnow_Classifier, DecisionTree_Classifier };
            ClassifierComboBox.SelectedIndex = 1;

            SimilarityMeasureComboBox.ItemsSource = new IDocumentComparer[] { new CosineComparer(), new EuclidianComparer(), new ManhattenComparer() };
            SimilarityMeasureComboBox.SelectedIndex = 0;

            BuildTrainingData(TrainingDataTextBox.Text);
        }

        #region Other Methods

        //train the documents with a given format (see documentation)
        private List<LabeledDocumentVector> LoadTrainingData(String TrainingPath)
        {
            if (!Directory.Exists(TrainingPath))
                return null;

            List<LabeledDocumentVector> TrainingData = new List<LabeledDocumentVector>();
            DirectoryInfo info = new DirectoryInfo(TrainingPath);
            FileInfo[] files = info.GetFiles();

            foreach (FileInfo file in files)
            {
                StreamReader reader = new StreamReader(file.FullName);
                String id;

                while ((id = reader.ReadLine()) != null)
                {
                    LabeledDocumentVector Document
                        = new LabeledDocumentVector(    (DocumentVector)DocClassifier.Documents[id], 
                                                        file.Name.Substring(0,file.Name.Length-4)       );   //remove the file extension
                    TrainingData.Add(Document);
                }
                reader.Close();
            }

            return TrainingData;
        }

        private void Write(String Text)
        {
            OutputTextBox.Dispatcher.BeginInvoke(new Action(
                delegate()
                {
                    Console.Out.WriteLine(Text);
                }), null);
        }

        private void SetProgress(long value)
        {
            SetupControlGrid.Dispatcher.BeginInvoke(new Action(
                delegate()
                {
                    double parseSpeed = (value / DocClassifier.ParseTime.TotalMilliseconds);
                    double estimated = (DocClassifier.Parser.FileLength - value )/ (parseSpeed*1000);

                    SetupProgressTextBlock.Text = value + "/" + DocClassifier.Parser.FileLength;
                                    
                    ParsingSpeedTextBlock.Text = parseSpeed.ToString("0.0") + " kb/second";
                    EstimatedTimeLeftTextBlock.Text = TimeSpan.FromSeconds(Math.Floor(estimated)).ToString();

                    SetupProgressBar.Value = value;
                    SetupProgressBar.Maximum = DocClassifier.Parser.FileLength;
                }), null);
        }

        private void SetCurrentTask(String Task)
        {
            SetupTaskTextBlock.Dispatcher.BeginInvoke(new Action(
                delegate
                {
                    SetupTaskTextBlock.Text = Task;
                }), null);
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

        private void BuildZipfGraph(Dictionary<string, int> Words)
        {
            Write("Building Tokens Graph");

            List<KeyValuePair<string,int>> WordsList = Words.ToList();
            WordsList.Sort(new WordInfoComparer());

            //need to reduce number of points
            List<Point> PointList = new List<Point>();
            int Interval = (int) WordsList.Count / 130;

            Point[] MinPointList = null;
            Point[] MaxPointList = null;

            for (int i = Interval; i < WordsList.Count; i+=Interval )
            {
                if (i >= (MinRange * DocClassifier.Documents.Count) && MinPointList == null )
                    MinPointList = new Point[] { new Point(i, 0), new Point(i, 150 ) };

                if (i >= (MaxRange * DocClassifier.Documents.Count) && MaxPointList == null )
                    MaxPointList = new Point[] { new Point(i, 0), new Point(i, 150 ) };

                PointList.Add( new Point(i,WordsList[i].Value) );
            }

            ZipfGraphChart.Dispatcher.BeginInvoke(new Action(
                delegate
                {
                    //RangeMaxGraphLineSeries.ItemsSource = MaxPointList;
                    //RangeMinGraphLineSeries.ItemsSource = MinPointList;
                    ZipfGraphAreaSeries.ItemsSource = PointList;
                }), null);
        }

        private void BuildTrainingData( String TrainingDataPath )
        {
            TrainingData = LoadTrainingData(TrainingDataPath);

            if (TrainingData != null)
            {
                Dictionary<string, int> LabelCount = GetLabelCount(TrainingData);
                ClassesListBox.ItemsSource = LabelCount;
            }
        }

        #endregion

        #region TO BE REMOVED
        private bool TestContainsId(List<LabeledDocumentVector> Data, string id)
        {
            foreach (LabeledDocumentVector Vector in Data)
            {
                if (Vector.Document.Id == id)
                    return true;
            }
            return false;
        }

        private List<LabeledDocumentVector> TestFilter(List<LabeledDocumentVector> Documents, List<LabeledDocumentVector> TrainingData)
        {
            List<LabeledDocumentVector> Output = new List<LabeledDocumentVector>();
            for (int i = 0; i < Documents.Count; i++)
            {
                if (TestContainsId(TrainingData, Documents[i].Document.Id))
                    Output.Add(Documents[i]);
            }
            return Output;
        }
        #endregion

        #region Classification Event Handlers

        private void DocClassifier_ParseIteration(object sender)
        {
            SetProgress(DocClassifier.Parser.FilePosition);
        }

        private void Classifier_ClassifyCompleted(object sender)
        {
            Write("Classification Time = " + DocClassifier.ClassificationTime);
            Write("Total Time = " + DocClassifier.TotalTime);
            SetCurrentTask("Completed");

            this.Dispatcher.BeginInvoke(new Action(
                delegate()
                {
                    ResultsListView.ItemsSource = DocClassifier.Classifier.Documents;
                    StartButton.IsEnabled = true;
                    CountButton.IsEnabled = true;
                    SetupTabItem.IsEnabled = true;
                }), null);
        }

        private void Classifier_FeatureWeightingCompleted(object sender)
        {
            Write("Feature Weighting Time = " + DocClassifier.FeatureWeightingTime);
            Write("Classification Technique = " + SelectedClassifier.ToString() );
            SetCurrentTask("Training");
            
            Stopwatch trainWatch = new Stopwatch();
            trainWatch.Start();

            TrainingData = LoadTrainingData(TrainingDataPath);
            SelectedClassifier.Train(TrainingData);
            trainWatch.Stop();

            Write("Classifier Training Time = " + trainWatch.Elapsed);
            SetCurrentTask("Classification");

            DocClassifier.ClassifyAsync( SelectedClassifier );
        }

        private void Classifier_FeatureSelectionCompleted(object sender)
        {
            Write( "Number of Features Selected = " + DocClassifier.Features.Count );
            Write( "Feature Selection Time = " + DocClassifier.FeatureSelectionTime );
            SetCurrentTask("Feature Weighting");

            FeaturesTabItem.Dispatcher.BeginInvoke(new Action(
                delegate
                {
                    NumberOfFeaturesTextBlock.Text = DocClassifier.Features.Count.ToString();
                    NumberOfDocumentsTextBlock.Text = DocClassifier.Documents.Count.ToString();
                    MinDocFrequencyTextBlock.Text = ((int) (MinRange * DocClassifier.Documents.Count)).ToString();
                    MaxDocFrequencyTextBlock.Text = ((int) (MaxRange * DocClassifier.Documents.Count)).ToString();
                    FeaturesListView.ItemsSource = DocClassifier.InvertedIndex.GetWordInfo("");
                }), null);

            DocClassifier.FeatureWeightingAsync();
        }

        private void Classifier_ParseCompleted(object sender)
        {
            Write("Number of Documents Found = " + DocClassifier.Documents.Count);
            Write("Parse Time = " + DocClassifier.ParseTime);
            Write("Inverted Index Time = " + DocClassifier.InvertedIndexTime);

            SetCurrentTask("Building Zipf Graph");
            BuildZipfGraph(DocClassifier.InvertedIndex.GetWordInfo(""));

            SetProgress(DocClassifier.Parser.FileLength);
            SetCurrentTask( "Feature Selection" );

            DocClassifier.FeatureSelectionAysnc(MinRange, MaxRange);
        }

        #endregion

        #region Control Event Handlers

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            StartButton.Content = "Restart Classification Process";
            StartButton.IsEnabled = false;
            SetupTabItem.IsEnabled = false;

            //clear all the values from the previous classification
            FeaturesListView.ItemsSource = null;
            ZipfGraphAreaSeries.ItemsSource = null;
            ResultsListView.ItemsSource = null;
            OutputTextBox.Text = "";

            RootTabControl.SelectedItem = ResultsTabItem;

            //setup all the classifier paramters here before starting
            KNN_Classifier.Comparer = (IDocumentComparer) SimilarityMeasureComboBox.SelectedItem;
            KNN_Classifier.K = Convert.ToInt32(KTextBox.Text);
            Perceptron_Classifier.LearningRate = Convert.ToDouble(LearningRateTextBox.Text);
            Winnow_Classifier.LearningRate = Convert.ToDouble(LearningRateTextBox.Text);

            MinRange = Convert.ToDouble(MinRangeTextBox.Text);
            MaxRange = Convert.ToDouble(MaxRangeTextBox.Text);
            
            SelectedClassifier = (Classifier)ClassifierComboBox.SelectedItem;
            TrainingDataPath = TrainingDataTextBox.Text;

            Write("Started Classification Process: "+DateTime.Now);
            Write("Began Sgml Parsing");
            SetCurrentTask( "Parsing" );
            DocClassifier.ParseAsync(CorpusPath);
        }

        private void CorpusButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Specify the Corpus File to use";

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK )
                CorpusTextBox.Text = dialog.FileName;
        }

        private void TrainingDataButton_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                TrainingDataTextBox.Text = dialog.SelectedPath;
                BuildTrainingData(TrainingDataTextBox.Text);
            }
        }

        private void ClassifierComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Classifier SelectedClassifier = (Classifier) ClassifierComboBox.SelectedItem;

            KNNControls.Visibility = (SelectedClassifier is KNN) ? Visibility.Visible : Visibility.Collapsed;
            PerceptronControls.Visibility = (SelectedClassifier is Perceptron ) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ResultsListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            LabeledDocumentVector Document = (LabeledDocumentVector) ResultsListView.SelectedValue;
            DocumentWindow Window = new DocumentWindow(Document, CorpusPath, DocClassifier.Features );
            Window.Show();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void CountButton_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<string, int> Labels = GetLabelCount(DocClassifier.Classifier.Documents);

            StringBuilder output = new StringBuilder();
            foreach (KeyValuePair<string, int> Label in Labels)
            {
                output.Append(Label);
                output.Append('\n');
            }

            String Text = output.ToString();
            System.Windows.Forms.MessageBox.Show(Text);
            Write(Text);
        }
        
        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {

        }

        #endregion
    }
}
