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
using System.Windows.Shapes;
using DocumentClassification;
using CodeBoxControl.Decorations;

namespace HLTAssignment
{
    /// <summary>
    /// Interaction logic for DocumentWindow.xaml
    /// </summary>
    public partial class DocumentWindow : Window
    {
        SgmlParser Parser;
        List<String> Features;
        Dictionary<string, double> FeatureWeights;

        String DocumentContent
        {
            get { return ContentTextBox.Text; }
            set { ContentTextBox.Text = value; }
        }
        String HeadLine
        {
            get { return HeadLineTextBlock.Text; }
            set { HeadLineTextBlock.Text = value; }
        }
        String DateLine
        {
            get { return DateLineTextBlock.Text; }
            set { DateLineTextBlock.Text = value; }
        }
        String Id
        {
            get { return DocIdTextBox.Text; }
            set { DocIdTextBox.Text = value; }
        }

        public DocumentWindow( LabeledDocumentVector Document, String CorpusPath, List<String> Features )
        {
            InitializeComponent();

            Parser = new SgmlParser(CorpusPath);
            Parser.FilePosition = Document.Document.Location;

            this.Features = Features;
            HeadLine = Document.Document.HeadLine;
            DateLine = Document.Document.DateLine;
            Id = Document.Document.Id;

            StringBuilder builder = new StringBuilder();
            String Value;

            while ( (Value=Parser.NextParagraph()) != null )
            {
                if (Parser.DocID != Id)
                    break;

                builder.Append(Value);
            }
            Parser.Close();

            DocumentContent = builder.ToString();

            FeatureWeights = new Dictionary<string, double>();

            for (int i = 0; i < Features.Count; i++)
                FeatureWeights.Add(Features[i], Document.Document.Vector[i]);

            VectorDataListView.ItemsSource = FeatureWeights;

            this.Title = Document.Document.Id + " Details";
        }

        private void AddHilight(int start, int length, Color color)
        {
            ExplicitDecoration ed = new ExplicitDecoration();
            ed.Start = start;
            ed.Length = length;
            ed.Brush = new SolidColorBrush(color);
            ed.DecorationType = EDecorationType.Hilight;
            ContentTextBox.Decorations.Add(ed);
        }

        private void ShowFeaturesButton_Click(object sender, RoutedEventArgs e)
        {
            if( ContentTextBox.Decorations.Count > 0 )
            {
                ShowFeaturesButton.Content = "Show All Features";
                ContentTextBox.Decorations.Clear();
            }
            else
            {
                ShowFeaturesButton.Content = "Hide All Features";
                int Length = DocumentContent.Length;

                foreach (String Feature in Features)
                {
                    int i = 0;
                    while (i < Length && i>=0)
                    {
                        int start = DocumentContent.IndexOf(Feature, i, StringComparison.OrdinalIgnoreCase);
                        if (start == -1)
                            break;
                    
                        AddHilight(start, Feature.Length, Colors.Yellow);
                        i = start + Feature.Length;
                    }
                }
            }

            ContentTextBox.InvalidateVisual();
        }
    }
}
