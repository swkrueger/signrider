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
        private int minimumAspectRatio = 3; //1:??

        public List<ColourSegment> determineColourSegments(Image<Bgr, byte> image)
        {
            List<ColourSegment> colourSegmentList = new List<ColourSegment>();
            //image._GammaCorrect(2.2);
            foreach (SignColour colour in Enum.GetValues(typeof(SignColour)))
            {
                Image<Gray, byte> fullBinaryImage = GetPixelMask("HSV", colour, image);
                Image<Gray, byte> mask = fullBinaryImage.CopyBlank();
                Image<Gray, byte> binaryCrop = null;
                Image<Bgr, byte> rgbCrop = null;

                // TODO: Check FindContour parameters
                using (MemStorage storage = new MemStorage())
                {
                    for (var contour = fullBinaryImage.FindContours(CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE, RETR_TYPE.CV_RETR_CCOMP, storage); contour != null; contour = contour.HNext)
                    {
                        if (contour.Area > minimumContourArea)
                        {
                            Rectangle rect1 = contour.BoundingRectangle;
                            Rectangle rect = rect1;

                            if ((rect1.X - 1) > 0 && ((rect1.X + (rect1.Width + 1)) < image.Width) && (rect1.Y - 2) > 0 && ((rect1.Y + (rect1.Height + 2)) < image.Height))
                                rect = new Rectangle(rect1.X - 1, rect1.Y - 1, rect1.Width + 2, rect1.Height + 2);

                            int rWidth = rect.Width;
                            int rHeight = rect.Height;
                            double aspectRatio = (double)rWidth / (double)rHeight;

                            if (rWidth > minimumSegmentWidth && rHeight > minimumSegmentHeight && aspectRatio > 1 / (double)minimumAspectRatio && aspectRatio < minimumAspectRatio)//
                            {
                                mask.Draw(contour, new Gray(255), -1);
                                binaryCrop = mask.Copy(rect);
                                rgbCrop = image.Copy(rect);
                                colourSegmentList.Add(new ColourSegment(rgbCrop, binaryCrop, colour));
                            }
                        }
                    }
                }

                // Free memory
                fullBinaryImage.Dispose();
                mask.Dispose();
                binaryCrop.Dispose();
                rgbCrop.Dispose();
            }
            return colourSegmentList;
        }

        //-> function returning the binary image with white the segment and black not
        private Image<Gray, Byte> GetPixelMask(String ColourSpace, SignColour Colour, Image<Bgr, byte> image)
        {
            //Image<Lab, Byte> lab = image.Convert<Lab, Byte>();
            //Image<Ycc, Byte> ycc = image.Convert<Ycc, Byte>();
            if (ColourSpace == "RGB")
            {
                Image<Gray, Byte>[] channels = image.Split();
                Image<Gray, Byte> filterB = null;
                Image<Gray, Byte> filterG = null;
                Image<Gray, Byte> filterR = null;
                int StartRangeB = 0;
                int EndRangeB = 0;
                int StartRangeG = 0;
                int EndRangeG = 0;
                int StartRangeR = 0;
                int EndRangeR = 0;

                if (Colour == SignColour.BLUE)
                {
                    StartRangeB = 107;
                    EndRangeB = 200;
                    StartRangeG = 0;
                    EndRangeG = 148;
                    StartRangeR = 0;
                    EndRangeR = 85;
                }

                // if (Colour == SignColour.GREEN)
                // {
                //     StartRangeB = 0;
                //     EndRangeB = 178;
                //     StartRangeG = 123;
                //     EndRangeG = 255;
                //     StartRangeR = 0;
                //     EndRangeR = 101;
                // }

                if (Colour == SignColour.RED)
                {
                    StartRangeB = 0;
                    EndRangeB = 96;
                    StartRangeG = 0;
                    EndRangeG = 98;
                    StartRangeR = 100;
                    EndRangeR = 255;
                }
                filterB = channels[0].InRange(new Gray(StartRangeB), new Gray(EndRangeB));
                filterG = channels[1].InRange(new Gray(StartRangeG), new Gray(EndRangeG));
                filterR = channels[2].InRange(new Gray(StartRangeR), new Gray(EndRangeR));
                Image<Gray, byte> mask = channels[0].CopyBlank();
                CvInvoke.cvAnd(filterB, filterG, mask, IntPtr.Zero);
                CvInvoke.cvAnd(mask, filterR, mask, IntPtr.Zero);

                // Free memory
                filterB.Dispose();
                filterG.Dispose();
                filterR.Dispose();
                for (int i = 0; i < 3; ++i) channels[i].Dispose();

                return mask;
            }
            else
            {
                int StartRange = 0;
                int EndRange = 0;
                if (Colour == SignColour.BLUE)
                {
                    StartRange = 100;
                    EndRange = 135;
                }

                // if (Colour == SignColour.GREEN)
                // {
                //     StartRange = 40;
                //     EndRange = 99;
                // }

                if (Colour == SignColour.RED)
                {
                    StartRange = 10;
                    EndRange = 175;
                }
                Image<Hsv, Byte> hsv = image.Convert<Hsv, Byte>();
                Image<Gray, Byte>[] channels = hsv.Split();
                
                Image<Gray, Byte> mask = channels[0].InRange(new Gray(StartRange), new Gray(EndRange));

                if (Colour == SignColour.RED)
                {
                    mask._Not();
                }
                channels[1]._ThresholdBinary(new Gray(100), new Gray(255.0));

                mask._And(channels[1]);

                // Free memory
                hsv.Dispose();
                for (int i = 0; i < 3; i++) channels[i].Dispose();

                return mask;
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
        public SignColour colour { get; set; }

        public ColourSegment(Image<Bgr, byte> rgbCrop, Image<Gray, byte> binaryCrop, SignColour colour)
        {
            this.rgbCrop = rgbCrop;
            this.binaryCrop = binaryCrop;
            this.colour = colour;
        }

        public void Dispose()
        {
            rgbCrop.Dispose();
            binaryCrop.Dispose();
        }
    }
}
