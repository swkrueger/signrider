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
        private enum SignNotFound { HSV, tryGammaCorrect, tryCMYK };
        SignNotFound signNotFound = SignNotFound.HSV;
        private Boolean isSignFound = false;


        public List<ColourSegment> determineColourSegments(Image<Bgr, byte> image)
        {
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
                    else if (signNotFound == SignNotFound.tryCMYK)
                    {
                        //Image<Bgr, byte> imageGamma = new Image<Bgr, byte>(image.Width, image.Height);
                        //image.CopyTo(imageGamma);
                        //imageGamma._GammaCorrect(2.2);
                        fullBinaryImage = GetPixelMask("CMYK", colour, image);
                    }

                    Image<Gray, byte> mask = fullBinaryImage.CopyBlank();
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
                                    mask.Draw(contour, new Gray(255), -1);
                                    binaryCrop = mask.Copy(rect);
                                    rgbCrop = image.Copy(rect);
                                    
                                    colourSegmentList.Add(new ColourSegment(rgbCrop, binaryCrop, contour, colour));
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
                        signNotFound = SignNotFound.tryCMYK;
                        isSignFound = true;
                    }
                    //else if (!isSignFound && signNotFound == SignNotFound.tryCMYK)
                    //{
                    //    isSignFound = true;
                    //}

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
            if (ColourSpace == "HSV")
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
                    StartRangeV = 30;
                }
                Image<Hsv, Byte> hsv = image.Convert<Hsv, Byte>();
                Image<Gray, Byte>[] channels = hsv.Split();
                CvInvoke.cvInRangeS(channels[0], new MCvScalar(StartRangeH), new MCvScalar(EndRangeH), channels[0]);
                if (Colour == SignColour.RED)
                {
                    channels[0]._Not();
                }
                channels[1]._ThresholdBinary(new Gray(StartRangeS), new Gray(255.0));
                channels[2]._ThresholdBinary(new Gray(StartRangeV), new Gray(255.0));
                CvInvoke.cvAnd(channels[0], channels[1], channels[0], IntPtr.Zero);
                CvInvoke.cvAnd(channels[0], channels[2], channels[0], IntPtr.Zero);
                return channels[0];


                //-> om een of ander rede gee dit nie dieselfde resultate nie
                //Image<Gray, Byte> mask = channels[0].InRange(new Gray(StartRangeH), new Gray(EndRangeH));
                //if (Colour == SignColour.RED)
                //{
                //    mask._Not();
                //}
                //channels[1]._ThresholdBinary(new Gray(100), new Gray(255.0));
                //channels[2]._ThresholdBinary(new Gray(StartRangeV), new Gray(255.0));
                //mask._And(channels[1]);
                //mask._And(channels[2]);

                //return mask;
            }
            else
            {
                int StartRangeC = 0;
                int EndRangeC = 20;
                int StartRangeM = 0;
                int EndRangeM = 20;
                int StartRangeY = 0;
                int EndRangeY = 20;
                int StartRangeK = 0;
                int EndRangeK = 0;

                if (Colour == SignColour.RED)
                {
                    StartRangeC = 10;
                    EndRangeC = 255;
                    StartRangeY = 0;
                    EndRangeY = 20;
                }

                if (Colour == SignColour.BLUE)
                {
                    StartRangeC = 0;
                    EndRangeC = 20;
                    StartRangeY = 10;
                    EndRangeY = 255;
                    StartRangeK = 0;
                    EndRangeK = 0;
                }
                // Needs revision: unessary components
                Image<Bgr, Byte> bgr = image;
                Image<Gray, Byte>[] bgrChannels = bgr.Split();

                Image<Gray, Byte> filterK = bgrChannels[0].CopyBlank();
                Image<Gray, Byte> filterC = bgrChannels[0].CopyBlank();
                Image<Gray, Byte> filterM = bgrChannels[0].CopyBlank();
                Image<Gray, Byte> filterY = bgrChannels[0].CopyBlank();

                Image<Gray, Byte> oneMinBlue = bgrChannels[0];
                oneMinBlue._Not();
                Image<Gray, Byte> oneMinGreen = bgrChannels[1];
                oneMinGreen._Not();
                Image<Gray, Byte> oneMinRed = bgrChannels[2];
                oneMinRed._Not();

                CvInvoke.cvMin(oneMinBlue, oneMinGreen, filterK);
                CvInvoke.cvMin(filterK, oneMinRed, filterK);

                Image<Gray, Byte> oneMinRedMinBlack = bgrChannels[0].CopyBlank();
                CvInvoke.cvSub(oneMinRed, filterK, oneMinRedMinBlack, IntPtr.Zero);
                Image<Gray, Byte> oneMinGreenMinBlack = bgrChannels[0].CopyBlank();
                CvInvoke.cvSub(oneMinGreen, filterK, oneMinGreenMinBlack, IntPtr.Zero);
                Image<Gray, Byte> oneMinBlueMinBlack = bgrChannels[0].CopyBlank();
                CvInvoke.cvSub(oneMinBlue, filterK, oneMinBlueMinBlack, IntPtr.Zero);
                Image<Gray, Byte> oneMinBlack = filterK;
                oneMinBlack._Not();

                CvInvoke.cvDiv(oneMinRedMinBlack, oneMinBlack, filterC, 255);
                CvInvoke.cvDiv(oneMinGreenMinBlack, oneMinBlack, filterM, 255);
                CvInvoke.cvDiv(oneMinBlueMinBlack, oneMinBlack, filterY, 255);

                filterC = filterC.InRange(new Gray(StartRangeC), new Gray(EndRangeC));
                filterC._Not();
                filterM = filterM.InRange(new Gray(StartRangeM), new Gray(EndRangeM));
                filterM._Not();
                filterY = filterY.InRange(new Gray(StartRangeY), new Gray(EndRangeY));
                filterY._Not();
                filterK = filterK.InRange(new Gray(StartRangeK), new Gray(EndRangeK));
                filterK._Not();

                Image<Gray, byte> mask = filterC.CopyBlank();
                CvInvoke.cvAnd(filterC, filterM, mask, IntPtr.Zero);
                CvInvoke.cvAnd(mask, filterY, mask, IntPtr.Zero);
                CvInvoke.cvAnd(mask, filterK, mask, IntPtr.Zero);

                // Free memory
                //bgr.Dispose();
                //for (int i = 0; i < 3; i++) bgrChannels[i].Dispose();
                //oneMinBlue.Dispose();
                //oneMinGreen.Dispose();
                //oneMinRed.Dispose();
                //oneMinRedMinBlack.Dispose();
                //oneMinGreenMinBlack.Dispose();
                //oneMinBlueMinBlack.Dispose();
                //oneMinBlack.Dispose();
                //filterC.Dispose();
                //filterY.Dispose();
                //filterM.Dispose();
                //filterK.Dispose();

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
        public Contour<Point> contour { get; set; }
        public SignColour colour { get; set; }

        public ColourSegment(Image<Bgr, byte> rgbCrop, Image<Gray, byte> binaryCrop, Contour<Point> contour, SignColour colour)
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

    ////-> class managing a found segment --without contour
    //public class ColourSegment : IDisposable
    //{
    //    public Image<Bgr, byte> rgbCrop { get; set; }
    //    public Image<Gray, byte> binaryCrop { get; set; }
    //    public SignColour colour { get; set; }

    //    public ColourSegment(Image<Bgr, byte> rgbCrop, Image<Gray, byte> binaryCrop, SignColour colour)
    //    {
    //        this.rgbCrop = rgbCrop;
    //        this.binaryCrop = binaryCrop;
    //        this.colour = colour;
    //    }

    //    public void Dispose()
    //    {
    //        rgbCrop.Dispose();
    //        binaryCrop.Dispose();
    //    }
    //}
}
