using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Diagnostics;
using System.Collections;

using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;

using Point = System.Drawing.Point;
using Forms = System.Windows.Forms;

namespace Signrider
{
    /// <summary>
    /// Interaction logic for HomeMenuView.xaml
    /// </summary>
    public partial class HomeMenuView : UserControl
    {
        public HomeMenuView()
        {
            InitializeComponent();
        }

        private void helloWorldTestButton_Click(object sender, RoutedEventArgs e)
        {
            //Create an image of 400x200 of Blue color
            using (Image<Bgr, Byte> img = new Image<Bgr, byte>(400, 200, new Bgr(255, 0, 0)))
            {
                //Create the font
                MCvFont f = new MCvFont(FONT.CV_FONT_HERSHEY_COMPLEX, 1.0, 1.0);

                //Draw "Hello, world." on the image using the specific font
                img.Draw("Hello, world", ref f, new Point(10, 80), new Bgr(0, 255, 0));

                //Show the image using ImageViewer from Emgu.CV.UI
                CvInvoke.cvShowImage("Hello World Test Window", img.Ptr);
            }

            // Test crop
            using (Image<Gray, Byte> img = new Image<Gray, byte>(400, 200, new Gray(0)))
            {
                MCvFont f = new MCvFont(FONT.CV_FONT_HERSHEY_COMPLEX, 1.0, 1.0);
                img.Draw("Hello, world", ref f, new Point(10, 80), new Gray(255));
                using (Image<Gray, Byte> img2 = Utilities.stripBorder(img, new Gray(100)))
                    CvInvoke.cvShowImage("After crop", img2.Ptr);
            }
        }

        private void colourSegmentiseTestButton_Click(object sender, RoutedEventArgs e)
        {
            ColourSegmenter segmenter = new ColourSegmenter();

            //-> to load and save multiple images from and to file:
            Forms.FolderBrowserDialog folderBrowserDialog = new Forms.FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == Forms.DialogResult.OK)
            {
                string inputDir = folderBrowserDialog.SelectedPath;
                string outputDir = System.IO.Path.Combine(inputDir, "Colour Segmentation Results");
                string[] files = Utilities.GetFiles(inputDir, "*.jpg|*.jpeg|*.png|*.ppm", SearchOption.TopDirectoryOnly);
                segmenter.explodePhotosToSegmentFiles(files, outputDir);
            }

            //-> one image at a time:
            //ColourSegmenter segmenter = new ColourSegmenter();
            //List<ColourSegment> segments = segmenter.determineColourSegments(BGR_img);
            //segments[i].rgbCrop to get RGB Cropped image
            //segments[i].binaryCrop to get binary Cropped image
            //segments[i].contour to get segment contour image
            //segments[i].colour to get sign colour image
        }

        private void shapeClassifierTestButton_Click(object sender, RoutedEventArgs e)
        {
            /****
             * Note:
             ****
             * Before clicking the button, copy everything from
             *   \\143.160.9.0\vbsucknet\ShapeTestData
             *   (\\192.168.0.62\ on Kiber network)
             * to
             *   C:\Users\%USERNAME%\Source\Repos\signrider\SignRider\SignRider\bin\TestData\
             ****/

            string trainDirectory = "..\\ShapeTestData\\train\\";
            string testDirectory = "..\\ShapeTestData\\test\\";

            ShapeClassifier classifier = new ShapeClassifier();

            // Retrieve training images
            List<ShapeExample> trainExamples = ShapeClassifier.extractExamplesFromDirectory(trainDirectory);

            if (trainExamples.Count() == 0)
            {
                System.Windows.MessageBox.Show("No training examples found! Please put training images in " + trainDirectory);
                return;
            }

            // Perform training
            classifier.train(trainExamples);

            // Perform testing
            int numTested = 0;
            int numSuccess = 0;
            int numGarbageTested = 0;
            int numGarbageSuccess = 0;
            List<ShapeExample> testExamples = ShapeClassifier.extractExamplesFromDirectory(testDirectory);

            foreach (ShapeExample example in testExamples)
            {
                SignShape expectedShape = example.shape;
                SignShape recognizedShape = classifier.classify(example.image, example.colour);

                Debug.WriteLine(
                    String.Format(
                        "{0}: Expected: {1}; Recognized: {2}",
                        example.name,
                        expectedShape,
                        recognizedShape)
                    );
                Debug.Flush();

                if (expectedShape != SignShape.Garbage)
                {
                    numTested++;
                    if (recognizedShape == expectedShape) numSuccess++;
                }
                else
                {
                    numGarbageTested++;
                    if (recognizedShape == expectedShape) numGarbageSuccess++;
                }
            }

            string message = "";

            message += String.Format("Trained from {0} examples\n", trainExamples.Count());

            if (numTested!=0)
                message += String.Format("Non-garbage success rate: {0}\n", numSuccess*100/numTested);
            else
                message += "No non-garbage images specified";

            if (numGarbageTested!=0)
                message += String.Format("Garbage success rate: {0}\n", numGarbageSuccess*100/numGarbageTested);
            else
                message += "No garbage images specified";

            Debug.Write(message);
            System.Windows.MessageBox.Show(message);
        }

        private void trainFeatureRecognizerButton_Click(object sender, RoutedEventArgs e)
        {
            string inputDir = "";
            ArrayList arrayList = new ArrayList();
            Forms.FolderBrowserDialog folderBrowserDialog1 = new Forms.FolderBrowserDialog();
            if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                inputDir = folderBrowserDialog1.SelectedPath + "/";
            }
        }
        FeatureRecognizer featureRecognizer;
        private void featureRecognizerTrainButton_Click(object sender, RoutedEventArgs e)
        {
            //List<FeatureExample> examples = FeatureRecognizer.extractExamplesFromDirectory("..\\ShapeTestData\\train\\");
            //List<FeatureExample> examples = FeatureRecognizer.extractExamplesFromDirectory("C:\\Users\\Hendrik\\Downloads\\New train and test\\TrainingSet\\Features");
            List<FeatureExample> examples = FeatureRecognizer.extractExamplesFromDirectory("C:\\Users\\Hendrik\\Desktop\\TrainingSet2 reduced2\\Features");
            featureRecognizer = new FeatureRecognizer();
            featureRecognizer.train(examples);
        }

        private void featureRecognizerTestButton_Click(object sender, RoutedEventArgs e)
        {
            //List<FeatureExample> examples = FeatureRecognizer.extractExamplesFromDirectory("C:\\Users\\Hendrik\\Downloads\\New train and test\\TestSet\\Features");
            List<FeatureExample> examples = FeatureRecognizer.extractExamplesFromDirectory("C:\\Users\\Hendrik\\Desktop\\TrainingSet2 reduced2\\Features");
            featureRecognizer.test(examples);
        }
    }
}
