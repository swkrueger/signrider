using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Collections;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using System.Diagnostics;

using GrayImage = Emgu.CV.Image<Emgu.CV.Structure.Gray, System.Byte>;
using BGRImage = Emgu.CV.Image<Emgu.CV.Structure.Bgr, System.Byte>;

namespace Signrider
{
    struct TestImage    //TODO: BETER naam kry
    {
        public string name;
        public GrayImage grayImage;
        public BGRImage rgbImage;
        public SignShape shape;
        public SignType type;
        public SignColour color;
    }


    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            // Setup debugging output
            Debug.AutoFlush = true;
        }

        private void loadAndStoreColourResults()
        {
            colourSegmentiseTestButton.Text = "Running";
            string inputDir = "";
            string outputDir = "";
            ArrayList arrayList = new ArrayList();
            FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                inputDir = folderBrowserDialog1.SelectedPath + "/";
            }
            outputDir = inputDir + "Colour Segmentation Results/";
            
            string tempdir = inputDir;
            string path = @tempdir;
            string[] filter = { ".bmp", ".jpg", ".jpeg", ".png", ".JPG", ".ppm" };
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            FileInfo[] fileInfo = directoryInfo.GetFiles();
            foreach (FileInfo fi in fileInfo)
                foreach (string s in filter)
                    if (s == fi.Extension)
                        arrayList.Add(fi.FullName);

            for (int k = 0; k < arrayList.Count; k++)
            {
                string pictureName = (string)arrayList[k];
                tempdir = tempdir.Replace("/", "\\");
                pictureName = pictureName.Replace(tempdir, "");
                foreach (string imageType in filter)
                {
                    pictureName = pictureName.Replace(imageType, "");
                }

                if (!Directory.Exists(outputDir + pictureName))
                {
                    Directory.CreateDirectory(outputDir + pictureName);
                }
                else
                {
                    System.IO.DirectoryInfo myDirInfo = new DirectoryInfo(outputDir + pictureName);
                    foreach (FileInfo file in myDirInfo.GetFiles())
                    {
                        file.Delete();
                    }
                }

                try 
                {
                    using (Image<Bgr, Byte> image = new Image<Bgr, Byte>(arrayList[k].ToString()))
                    {
                        ColourSegmenter segmenter = new ColourSegmenter();
                        List<ColourSegment> segments = segmenter.determineColourSegments(image);

                        for (int i = 0; i < segments.Count; i++)
                        {
                            segments[i].rgbCrop.Save(outputDir + pictureName + "/" + pictureName + i.ToString() + "_" + segments[i].colour + "_RGB"  + ".png");
                            segments[i].binaryCrop.Save(outputDir + pictureName + "/" + pictureName + i.ToString() + "_" + segments[i].colour + "_BW" + ".png");
                        }
                    }
                } 
                catch (OutOfMemoryException oome)
                {
                  GC.Collect();
                }
            }
            
        }

        private void colourSegmentiseTestButton_Click(object sender, EventArgs e)
        {
            //-> to load and save multiple images from and to file:
            loadAndStoreColourResults();
            colourSegmentiseTestButton.Text = "Bulk Segmentise Images in Directory";
            //-> one image at a time:
            //ColourSegmenter segmenter = new ColourSegmenter();
            //List<ColourSegment> segments = segmenter.determineColourSegments(BGR_img);
            //segments[i].rgbCrop to get RGB Cropped image
            //segments[i].binaryCrop to get binary Cropped image
            //segments[i].contour to get segment contour image
            //segments[i].colour to get sign colour image
        }

        private void helloWorldTestButton_Click(object sender, EventArgs e)
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
        }

        private void FeutureRecognizerTestButton_Click(object sender, EventArgs e)
        {
            List<TestImage> images = loadTestDirectory("..\\ShapeTestData\\train3\\");
            FeatureRecognizer featureRecognizer = new FeatureRecognizer();
            featureRecognizer.trainImage(images[15]);
        }

        private void TrainFeutureRecognizerButton(object sender, EventArgs e)
        {
            TrainFeutureRecognizer();
        }

        private void TrainFeutureRecognizer()
        {
            string inputDir = "";
            ArrayList arrayList = new ArrayList();
            FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                inputDir = folderBrowserDialog1.SelectedPath + "/";
            }
            /*
            string path = inputDir;
            string[] filter = { ".bmp", ".jpg", ".jpeg", ".png", ".JPG" };
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            FileInfo[] fileInfo = directoryInfo.GetFiles();
            foreach (FileInfo fi in fileInfo)
                foreach (string s in filter)
                    if (s == fi.Extension)
                        arrayList.Add(fi.FullName);

            for (int k = 0; k < arrayList.Count; k++)
            {
                string pictureName = (string)arrayList[k];
                tempdir = tempdir.Replace("/", "\\");
                pictureName = pictureName.Replace(tempdir, "");
                foreach (string imageType in filter)
                {
                    pictureName = pictureName.Replace(imageType, "");
                }

                if (!Directory.Exists(outputDir + pictureName))
                {
                    Directory.CreateDirectory(outputDir + pictureName);
                }
                else
                {
                    System.IO.DirectoryInfo myDirInfo = new DirectoryInfo(outputDir + pictureName);
                    foreach (FileInfo file in myDirInfo.GetFiles())
                    {
                        file.Delete();
                    }
                }

                try
                {
                    using (Image<Bgr, Byte> image = new Image<Bgr, Byte>(arrayList[k].ToString()))
                    {
                        ColourSegmenter segmenter = new ColourSegmenter();
                        List<ColourSegment> segments = segmenter.determineColourSegments(image);

                        for (int i = 0; i < segments.Count; i++)
                        {
                            segments[i].rgbCrop.Save(outputDir + pictureName + "/" + segments[i].colour + "_RGB_" + i.ToString() + ".png");
                            segments[i].binaryCrop.Save(outputDir + pictureName + "/" + segments[i].colour + "_Binary_" + i.ToString() + ".png");
                        }
                    }
                }
                catch (OutOfMemoryException oome)
                {
                    GC.Collect();
                }
            }*/
        }

        private static string[] GetFiles(string sourceFolder, string filters, System.IO.SearchOption searchOption)
        {
            return filters.Split('|').SelectMany(filter => System.IO.Directory.GetFiles(sourceFolder, filter, searchOption)).ToArray();
        }

        private  List<TestImage> loadTestDirectory(string dir)      //TODO: beter naam kry
        {
            Debug.WriteLine("Loading test Directory example " + dir);
            List<TestImage> testImages = new List<TestImage>();
              if (System.IO.Directory.Exists(dir))
                {
                    string[] files =  GetFiles(dir, "*BW.jpg|*BW.png", SearchOption.AllDirectories);

                    foreach (string file in files)
                    {
                        string rgbFile = file.Replace("_BW", "_RGB");
                        if (!System.IO.File.Exists(rgbFile))
                            continue;

                        Debug.WriteLine("Loaded image: " + file);
                        Debug.Flush();

                        //TODO: Beter manier van opslit hier is ugly finnig metode:
                        string[] filename = file.Split('\\');
                        int filenameCount = filename.Count();
                        if (filenameCount < 3)
                            continue;

                        string typeString = filename[filenameCount - 2];
                        string shapeString = filename[filenameCount - 3];
                        SignShape shape;
                        SignType type;

                        if (Enum.IsDefined(typeof(SignShape), shapeString))
                            shape = (SignShape)Enum.Parse(typeof(SignShape), shapeString);
                        else
                            continue;

                        if (Enum.IsDefined(typeof(SignType), typeString))
                            type = (SignType)Enum.Parse(typeof(SignType), typeString);
                        else
                            continue;

                        string[] filetype = file.Split('\\').Last().Split('_');
                        int dashCount = filetype.Count();
                        if (dashCount < 3)
                            continue;

                        string colorString = filetype[dashCount - 2];
                        SignColour color;

                        if (Enum.IsDefined(typeof(SignColour), colorString))
                            color = (SignColour)Enum.Parse(typeof(SignColour), colorString);
                        else
                            continue;

                        TestImage img = new TestImage();
                        GrayImage grayImg = new GrayImage(file);
                        BGRImage rgbImg = new BGRImage(rgbFile);
                        img.color = color;
                        img.shape = shape;
                        img.type = type;
                        img.grayImage = grayImg;
                        img.rgbImage = rgbImg;

                        testImages.Add(img);

                        //Debug.WriteLine(shapeString + " " + typeString + " " + colorString);
                    }
                }
              return testImages;
        }

        private void shapeClassifierTestButton_Click(object sender, EventArgs e)
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

            List<TrainingExample> examples = new List<TrainingExample>();

            int numTrainingExamples = 0;

            // Retrieve training images
            foreach (SignShape shape in Enum.GetValues(typeof(SignShape)))
            {
                string dir = trainDirectory + shape.ToString() + "\\";

                if (System.IO.Directory.Exists(dir))
                {
                    string[] files = GetFiles(dir, ".jpg|*.png", SearchOption.TopDirectoryOnly);

                    foreach (string file in files)
                    {
                        TrainingExample example = new TrainingExample();

                        // TODO: Catch exception
                        example.image = new Image<Gray, Byte>(file);
                        example.shape = shape;

                        Debug.WriteLine("Loaded training example " + file);
                        Debug.Flush();

                        examples.Add(example);

                        numTrainingExamples++;
                    }
                }
            }

            if (numTrainingExamples == 0)
            {
                MessageBox.Show("No training examples found! Please put training images in " + trainDirectory);
                return;
            }

            // Perform training
            classifier.train(examples);

            // Perform testing
            int numTested = 0;
            int numSuccess = 0;
            int numGarbageTested = 0;
            int numGarbageSuccess = 0;

            foreach (SignShape expectedShape in Enum.GetValues(typeof(SignShape)))
            {
                string dir = testDirectory + expectedShape.ToString() + "\\";

                if (System.IO.Directory.Exists(dir))
                {
                    string[] files = GetFiles(dir, ".jpg|*.png", SearchOption.TopDirectoryOnly);

                    foreach (string file in files)
                    {
                        using (Image<Gray, Byte> image = new Image<Gray, Byte>(file))
                        {
                            SignShape shape = classifier.classify(image);
                            //SignShape shape = SignShape.Garbage;
                            Debug.WriteLine(String.Format("{0}: Expected: {1}; Got: {2}", file, expectedShape, shape));
                            Debug.Flush();

                            if (expectedShape != SignShape.Garbage)
                            {
                                numTested++;
                                if (shape == expectedShape) numSuccess++;
                            }
                            else
                            {
                                numGarbageTested++;
                                if (shape == expectedShape) numGarbageSuccess++;
                            }
                        }
                    }
                }
            }

            string message = "";

            message += String.Format("Trained from {0} examples\n", numTrainingExamples);

            if (numTested!=0)
                message += String.Format("Non-garbage success rate: {0}\n", numSuccess*100/numTested);
            else
                message += "No non-garbage images specified";

            if (numGarbageTested!=0)
                message += String.Format("Garbage success rate: {0}\n", numGarbageSuccess*100/numGarbageTested);
            else
                message += "No garbage images specified";

            Debug.Write(message);
            MessageBox.Show(message);
        }
    }
}
