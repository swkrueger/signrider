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

namespace SignRider
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void loadAndStoreColourResults()
        {
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
            }
            
        }

        private void colourSegmentiseTestButton_Click(object sender, EventArgs e)
        {
            //-> to load and save multiple images from and to file:
            loadAndStoreColourResults();

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
            FeatureRecognizer featureRecognizer = new FeatureRecognizer();
            featureRecognizer.helloTest();
            featureRecognizer.doTest();
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

    }
}
