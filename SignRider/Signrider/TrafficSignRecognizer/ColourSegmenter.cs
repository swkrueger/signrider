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

using BGRImage = Emgu.CV.Image<Emgu.CV.Structure.Bgr, System.Byte>;
using GrayImage = Emgu.CV.Image<Emgu.CV.Structure.Gray, System.Byte>;

namespace Signrider
{
    //-> enum containing the road sign colours
    public enum SignColour { RED, BLUE }; // TODO: Add yellow and/or white

    //-> class executing colour segmentation
    public class ColourSegmenter
    {
        private int minimumContourArea = 1000;
        private int minimumSegmentWidth = 30;
        private int minimumSegmentHeight = 30;
        private int minimumAspectRatio = 2; //1:??
        private enum SignNotFound { HSV, tryGammaCorrect};


        public List<ColourSegment> determineColourSegments(Image<Bgr, byte> image)
        {
            Boolean isSignFound = false;
            SignNotFound signNotFound = SignNotFound.HSV;
            List<ColourSegment> colourSegmentList = new List<ColourSegment>();
            foreach (SignColour colour in Enum.GetValues(typeof(SignColour)))
            {
                isSignFound = false;
                signNotFound = SignNotFound.HSV;
                do
                {
                    Image<Gray, byte> fullBinaryImage = null;
                    if (signNotFound == SignNotFound.HSV)
                    {
                        fullBinaryImage = GetPixelMask("HSV", colour, image);
                    }
                    else if (signNotFound == SignNotFound.tryGammaCorrect)
                    {
                        Image<Bgr, byte> imageGamma = new Image<Bgr, byte>(image.Width, image.Height);
                        image.CopyTo(imageGamma);
                        imageGamma._GammaCorrect(2.2);
                        fullBinaryImage = GetPixelMask("HSV", colour, imageGamma);
                    }

   
                    Image<Gray, byte> binaryCrop = null;
                    Image<Bgr, byte> rgbCrop = null;

                    // TODO: Check FindContour parameters
                    using (MemStorage storage = new MemStorage())
                    {
                        for (var contour = fullBinaryImage.FindContours(CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE, RETR_TYPE.CV_RETR_CCOMP); contour != null; contour = contour.HNext)
                        {
                            if (contour.Area > minimumContourArea)
                            {
                                isSignFound = true;
                                Rectangle rect1 = contour.BoundingRectangle;
                                Rectangle rect = rect1;

                                if ((rect1.X - 1) > 0 && ((rect1.X + (rect1.Width + 1)) < image.Width) && (rect1.Y - 2) > 0 && ((rect1.Y + (rect1.Height + 2)) < image.Height))
                                    rect = new Rectangle(rect1.X - 1, rect1.Y - 1, rect1.Width + 2, rect1.Height + 2);

                                int rWidth = rect.Width;
                                int rHeight = rect.Height;
                                double aspectRatio = (double)rWidth / (double)rHeight;

                                if (rWidth > minimumSegmentWidth && rHeight > minimumSegmentHeight && aspectRatio > 1 / (double)minimumAspectRatio && aspectRatio < minimumAspectRatio)//
                                {
                                    Image<Gray, byte> mask = fullBinaryImage.CopyBlank();
                                    mask.Draw(contour, new Gray(255), -1);
                                    binaryCrop = mask.Copy(rect);
                                    rgbCrop = image.Copy(rect);

                                    colourSegmentList.Add(new ColourSegment(rgbCrop, binaryCrop, contour.ToArray(), colour));
                                }
                            }
                        }
                    }


                    if (!isSignFound && signNotFound == SignNotFound.HSV)
                    {
                        signNotFound = SignNotFound.tryGammaCorrect;
                    }
                    else if (!isSignFound && signNotFound == SignNotFound.tryGammaCorrect)
                    {
                        isSignFound = true;
                    }


                    // Free memory
                    //fullBinaryImage.Dispose();
                    //mask.Dispose();
                    //binaryCrop.Dispose();
                    //rgbCrop.Dispose();

                } while (!isSignFound);
            }
            return colourSegmentList;
        }

        //-> function returning the binary image with white the segment and black not
        private Image<Gray, Byte> GetPixelMask(String ColourSpace, SignColour Colour, Image<Bgr, byte> image)
        {
            int StartRangeH = 0;
            int EndRangeH = 0;
            int StartRangeV = 0;
            int StartRangeS = 0;
            if (Colour == SignColour.BLUE)
            {
                StartRangeH = 100;
                EndRangeH = 135;
                StartRangeS = 110;
                StartRangeV = 30;
            }

            if (Colour == SignColour.RED)
            {
                StartRangeH = 10;
                EndRangeH = 170;
                StartRangeS = 50;
                StartRangeV = 10;
            }
            using (Image<Hsv, Byte> hsv = image.Convert<Hsv, Byte>())
            {
                Image<Gray, Byte>[] channels = hsv.Split();
                try
                {
                    CvInvoke.cvInRangeS(channels[0], new MCvScalar(StartRangeH), new MCvScalar(EndRangeH), channels[0]);
                    if (Colour == SignColour.RED)
                    {
                        channels[0]._Not();
                    }
                    channels[1]._ThresholdBinary(new Gray(StartRangeS), new Gray(255.0));
                    channels[2]._ThresholdBinary(new Gray(StartRangeV), new Gray(255.0));
                    CvInvoke.cvAnd(channels[0], channels[1], channels[0], IntPtr.Zero);
                    CvInvoke.cvAnd(channels[0], channels[2], channels[0], IntPtr.Zero);
                }
                finally
                {
                    channels[1].Dispose();
                    channels[2].Dispose();
                }
                return channels[0];
            }
        }

        public void explodePhotoToSegmentFiles(string filePath, string outputDir)
        {
            string pictureName = Path.GetFileNameWithoutExtension(filePath);

            try
            {
                using (BGRImage image = new BGRImage(filePath))
                {
                    List<ColourSegment> segments = determineColourSegments(image);

                    for (int i = 0; i < segments.Count; i++)
                    {
                        string basePath =
                            Path.Combine(
                                outputDir,

                                string.Format(
                                    "{0}{1}_{2}",
                                    pictureName,
                                    i,
                                    segments[i].colour
                                )
                            );

                        segments[i].rgbCrop.Save(basePath + "_RGB" + ".png");
                        segments[i].binaryCrop.Save(basePath + "_BW" + ".png");
                    }

                    foreach (ColourSegment segment in segments)
                        segment.Dispose();
                }
            }
            catch (OutOfMemoryException)
            {
                GC.Collect();
            }

            // FIXME: This is not good practice
            GC.Collect();
        }

        public void explodePhotosToSegmentFiles(string[] files, string outputDir)
        {
            foreach (string filePath in files)
            {
                string pictureName = Path.GetFileNameWithoutExtension(filePath);
                string imageOutputDir = Path.Combine(outputDir, pictureName);

                if (!Directory.Exists(imageOutputDir))
                {
                    Directory.CreateDirectory(imageOutputDir);
                }
                // TODO: else delete files

                explodePhotoToSegmentFiles(filePath, imageOutputDir);
            }
        }

    }

    //-> class managing a found segment
    public class ColourSegment : IDisposable
    {
        public Image<Bgr, byte> rgbCrop { get; set; }
        public Image<Gray, byte> binaryCrop { get; set; }
        public Point[] contour { get; set; }
        public SignColour colour { get; set; }

        public ColourSegment(Image<Bgr, byte> rgbCrop, Image<Gray, byte> binaryCrop, Point[] contour, SignColour colour)
        {
            this.rgbCrop = rgbCrop;
            this.binaryCrop = binaryCrop;
            this.contour = contour;
            this.colour = colour;
        }

        public void Dispose()
        {
            rgbCrop.Dispose();
            binaryCrop.Dispose();
        }
    }
}
