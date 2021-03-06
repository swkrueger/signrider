﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Forms;
using System.ComponentModel;
using System.Data;
using System.Drawing;

using System.Numerics;

using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;

using Emgu.CV.ML;
using Emgu.CV.ML.Structure;
using System.IO;

using BGRImage = Emgu.CV.Image<Emgu.CV.Structure.Bgr, System.Byte>;
using GrayImage = Emgu.CV.Image<Emgu.CV.Structure.Gray, System.Byte>;

namespace Signrider
{
    //TODO:
    public enum SignType {
        Garbage,

        // Circles
        MiscCircle,
        HawkersProhibited,
        KeepLeft,
        LeftTurnAheadProhibited,
        MinibusesOnly,
        MinimumSpeedLimit40,
        MotorCarsOnly,
        NoEntry,
        SpeedLimit40,
        SpeedLimit60,
        SpeedLimit80,
        SpeedLimit100,
        SpeedLimit120,
        StoppingProhibited,
        UTurnProhibited,

        // Octagon
        Stop,

        // Rectangle
        Countdown1,
        Countdown2,
        Countdown3,
        DualCarriagewayFreewayBegins,
        LimitedParkingReservation,

        // TraingleDown
        Yield,
        YieldAtMinicircle,

        // TriangleUp
        Crossroad,
        GeneralWarning,
        GentleCurveLeft,
        GentleCurveRight,
        PedestrianCrossing,
        PriorityCrossing,
        SideRoadJunctionLeft,
        SideRoadJunctionRight,
        SpeedHumps,
        TemporaryRoadWorks,
        TwowayTraffic,
        MiscTriangleUp
    };



    public struct FeatureExample
    {
        public string name;
        public GrayImage grayImage;
        public BGRImage rgbImage;
        public SignShape shape;
        public SignType type;
        public SignColour color;
        public string directory;
    }

    public struct FeatureExampleElement
    {
        public Matrix<float> parameter;
        public SignType type;
    }

    //TODO: templateting
    public class DebugImage
    {
        public IImage Image { get; set; }
        public string Name { get; set; }
        public DebugImage() { }
        public DebugImage(IImage image, string name)
        {
            this.Image = (IImage)image;
            this.Name = name;
        }
    }

    public class FeatureRecognizer
    {
        public bool[] SignTypeColor = new bool[]
    {
        //0 false //1 true
        false,

        // Circles
        false, //MiscCircle,
        false, //HawkersProhibited,
        true, //KeepLeft,
        false, //LeftTurnAheadProhibited,
        true, //MinibusesOnly,
        true, //MinimumSpeedLimit40,
        true, //MotorCarsOnly,
        false, //NoEntry,
        false, //SpeedLimit40,
        false, //SpeedLimit60,
        false, //SpeedLimit80,
        false, //SpeedLimit100,
        false, //SpeedLimit120,
        false, //StoppingProhibited,
        false, //UTurnProhibited,

        // Octagon
        false, //Stop,

        // Rectangle
        true, //Countdown1,
        true, //Countdown2,
        true, //Countdown3,
        true, //DualCarriagewayFreewayBegins,
        true, //LimitedParkingReservation,

        // TraingleDown
        false, //Yield,
        false, //YieldAtMinicircle,

        // TriangleUp
        false, //Crossroad,
        false, //GeneralWarning,
        false, //GentleCurveLeft,
        false, //GentleCurveRight,
        false, //PedestrianCrossing,
        false, //PriorityCrossing,
        false, //SideRoadJunctionLeft,
        false, //SideRoadJunctionRight,
        false, //SpeedHumps,
        false, //TemporaryRoadWorks,
        false, //TwowayTraffic,
        false, //MiscTriangleUp
    };


        // Constants
        private const int featureVectorDimension = 128;
        private const int calculateParametersImageSize = 300;

        // Preprocessor settings
        private bool preprocessorResize = true;
        private bool preprocessorRemoveBackground = true;
        private bool preprocessorRemoveSaturation = true;
        private bool preprocessorGammaCorrection = true;
        private bool preprocessorLightnessCorrection = true;
        private bool preprocessorRemoveGrading = true;
        private bool preprocessorRemoveBorder = true;
        private bool preprocessorCrop = true;

        private bool calculateParametersSmoothGaussian = false;

        

        //DebugImage
        private bool debugImageHistogram = true;
        private bool debugImageEdgeImprovements = true;

        // Debugging output settings
        private bool debugHistogram = false;
        private bool debugPlotFeatureVector = false;
        private bool debugPauseOnImageShow = true;
        private bool debugPreprocessor = false;

        // Private class variables
        //private SVM SvmModel = new SVM();
        //private SVMParams SvmParameters;
        List<SVM> SVMModels = new List<SVM>();
        MCvSVMParams paramsm;// = new MCvSVMParams();
        SVMParams SVMParameters;
        List<DebugImage> debugImages;
        string debugFolder;

        private int signShapeCount = Enum.GetValues(typeof(SignShape)).Length;

        public bool isTrained { get; private set; }
        public bool isTraining { get; private set; }

        private List<ConvolutionKernelF> cKernelList = new List<ConvolutionKernelF>();
        private Image<Gray, double> GWOutputImage = new Image<Gray, double>(1, 1);

        public FeatureRecognizer()
        {
            isTrained = false;

            SVMParameters = new SVMParams();
            SVMParameters.SVMType = Emgu.CV.ML.MlEnum.SVM_TYPE.C_SVC;
            //SVMParameters.SVMType = Emgu.CV.ML.MlEnum.SVM_TYPE.NU_SVC;
            //p.KernelType = Emgu.CV.ML.MlEnum.SVM_KERNEL_TYPE.POLY;
            //SVMParameters.KernelType = Emgu.CV.ML.MlEnum.SVM_KERNEL_TYPE.LINEAR;
            SVMParameters.KernelType = Emgu.CV.ML.MlEnum.SVM_KERNEL_TYPE.POLY;
            //SVMParameters.KernelType = Emgu.CV.ML.MlEnum.SVM_KERNEL_TYPE.RBF;
            SVMParameters.Gamma = 3;
            SVMParameters.Degree = 3;
            SVMParameters.C = 1;
            SVMParameters.TermCrit = new MCvTermCriteria(100, 0.00001);
            //SVMParameters.Nu
            //SVMParameters.P
            //SVMParameters.Coef0 = 
            paramsm = new MCvSVMParams();
            paramsm.svm_type = Emgu.CV.ML.MlEnum.SVM_TYPE.C_SVC;
            paramsm.kernel_type = Emgu.CV.ML.MlEnum.SVM_KERNEL_TYPE.LINEAR;
            paramsm.gamma = 2;
            paramsm.degree = 3;
            paramsm.C = 3;
            paramsm.term_crit = new MCvTermCriteria(100, 0.00001);
            //debugImages = new List<DebugImage>();
            debugImages = null;

            //SVMModel = new SVM();
           // foreach(SignShape shape in Enum.GetValues(typeof(SignShape)))
            for (int i = 0;i < signShapeCount;i++)
            {
                SVM model = new SVM();
                SVMModels.Add(model);
            }


            int R = 10;
            int C = 10;
            double v = 2;
            for (int i = 0; i < 8; i++)
            {
                GWOutputImage = GWOutputImage.ConcateHorizontal(GaborWavelet(R, C, i, v));
                Image<Gray, double> GW = GaborWavelet(R, C, i, v);
                ConvolutionKernelF ckernel = new ConvolutionKernelF(10, 10);
                for (int l = 0; l < 10; l++)
                    for (int k = 0; k < 10; k++)
                        ckernel[l, k] = (float)GW[l, k].Intensity / 10000;
                ckernel.Center = new Point(5, 5);
                cKernelList.Add(ckernel);
            }
        }

        private void drawGrid(GrayImage image)
        {
            //TODO: remove constants (dynamic)
            image.Draw(new LineSegment2D(new Point(300 / 4, 0), new Point(300 / 4, 300)), new Gray(125), 1);
            image.Draw(new LineSegment2D(new Point(300 / 4 * 2, 0), new Point(300 / 4 * 2, 300)), new Gray(125), 1);
            image.Draw(new LineSegment2D(new Point(300 / 4 * 3, 0), new Point(300 / 4 * 3, 300)), new Gray(125), 1);
            image.Draw(new LineSegment2D(new Point(0, 300 / 4), new Point(300, 300 / 4)), new Gray(125), 1);
            image.Draw(new LineSegment2D(new Point(0, 300 / 4 * 2), new Point(300, 300 / 4 * 2)), new Gray(125), 1);
            image.Draw(new LineSegment2D(new Point(0, 300 / 4 * 3), new Point(300, 300 / 4 * 3)), new Gray(125), 1);
        }

        private Image<Gray, double> GaborWavelet(int R, int C, double u, double v)
        {
            double pi = 3.1415;
            double Kmax = pi / 2;
            double f = Math.Sqrt(2);
            double Delta = 2 * pi;
            double Delta2 = Delta * Delta;
            Image<Gray, double> GW = new Image<Gray, double>(R, C);
            Complex k = (Kmax / (Math.Pow(f, v))) * Complex.Exp(new Complex(0, 1) * u * pi / 8);
            double kn2 = Math.Pow(Complex.Abs(k), 2);

            for (int i = -R / 2; i < R / 2; i++)
            {
                for (int j = -C / 2; j < C / 2; j++)
                {
                    int y = i + R / 2;
                    int x = j + C / 2;
                    Complex a = (kn2 / Delta2) * Math.Exp(-0.5 * kn2 * (Math.Pow(i, 2) + Math.Pow(j, 2)) / Delta2) * (Complex.Exp(new Complex(0, 1) * (k.Real * i + k.Imaginary * j)) - Math.Exp(-0.5 * Delta2));
                    Gray color = GW[y, x];
                    double mag = a.Real;
                    color.Intensity = mag * 40;
                    GW[y, x] = color;
                }
            }
            return GW;
        }

        private GrayImage removeGradient(GrayImage image)
        {
            if (!preprocessorRemoveGrading)
                return image;

            //TODO: Test if morf improve
            GrayImage grayImage = image.Copy();
            StructuringElementEx element = new StructuringElementEx(1, 1, 0, 0, CV_ELEMENT_SHAPE.CV_SHAPE_ELLIPSE);
            element = new StructuringElementEx(2, 2, 0, 0, CV_ELEMENT_SHAPE.CV_SHAPE_ELLIPSE);
            grayImage._MorphologyEx(element, CV_MORPH_OP.CV_MOP_CLOSE, 1);
            saveDebugImage(grayImage, "after Morph");
            element.Dispose();


            //grayImage._ThresholdBinary(new Gray(120), new Gray(255));
            GrayImage paddedImage = new GrayImage(350, 350);
            CvInvoke.cvCopyMakeBorder(grayImage, paddedImage, new Point(25, 25), BORDER_TYPE.CONSTANT, new MCvScalar(0));
            //grayImage.Dispose();
            grayImage = paddedImage;

            saveDebugImage(grayImage, "Binary image (larger)");

            for (var contour = grayImage.FindContours(CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE, RETR_TYPE.CV_RETR_CCOMP); contour != null; contour = contour.HNext)
            {
                Point[] points = contour.ToArray();
                MCvBox2D box = PointCollection.MinAreaRect(Array.ConvertAll(points, item => (PointF)item));

                if (box.size.Height < 5 || box.size.Width < 5)
                    grayImage.FillConvexPoly(points, new Gray(0));
            }

            grayImage._ThresholdBinaryInv(new Gray(128), new Gray(255));
            for (var contour = grayImage.FindContours(CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE, RETR_TYPE.CV_RETR_CCOMP); contour != null; contour = contour.HNext)
            {
                Point[] points = contour.ToArray();
                MCvBox2D box = PointCollection.MinAreaRect(Array.ConvertAll(points, item => (PointF)item));

                //Console.WriteLine("hight: " + box.size.Height.ToString() + " Witdh: " + box.size.Width.ToString());
                if (box.size.Height < 5 || box.size.Width < 5)
                    grayImage.FillConvexPoly(points, new Gray(0));
            }
            grayImage._ThresholdBinaryInv(new Gray(128), new Gray(255));
            saveDebugImage(grayImage, "Grading removed");

            return grayImage.GetSubRect(new Rectangle(25, 25, 300, 300));
        }
        

        private double determineScaler(GrayImage image)
        {
            int xAxisCurser = 10;
            Point point2 = new Point(0, 0);
            Image<Gray, Byte> histogram = new Image<Gray, Byte>(700, 600);
            double som;
            double count = 0;
            MCvScalar averageScalar = CvInvoke.cvSum(image);
            int  average = (int)(averageScalar.v0 / 255 * 0.2);
            double scaler = 1;
            for (int n = 0; n <= 250; n += 5)
            {
                using (Image<Gray, Byte> ttemp = image.ThresholdBinary(new Gray(250 - n), new Gray(255)))
                {
                    MCvScalar sum = CvInvoke.cvSum(ttemp);
                    som = sum.v0 / 255 - count;
                    if (debugHistogram)
                        Debug.WriteLine("n: " + n.ToString() + " som: " + som.ToString() + " count: " + count);
                    count += som;

                    if (n > 100 && count < average)
                        scaler = (double)n / (double)75;
                    if (!isTraining)
                    {
                        if (n != 0)
                            histogram.Draw(new LineSegment2D(point2, new Point(xAxisCurser * 10, 500 - (int)som / 20)), new Gray(128), 1);
                        point2 = new Point(xAxisCurser * 10, 500 - (int)som / 20);
                    }
                    xAxisCurser++;
                }
            }
            som = 300 * 300 - count;
            if (debugHistogram)
                Debug.WriteLine("som: " + som.ToString() + "count: " + count);
            histogram.Draw(new LineSegment2D(point2, new Point(xAxisCurser * 10, 500 - (int)som / 20)), new Gray(128), 1);
            saveDebugImage(histogram, "Lightness histogram");

            if (debugHistogram)
                Debug.WriteLine("scaler: " + scaler + " gemiddeld: " + average.ToString());

            return scaler;
        }

        private GrayImage preprocess(BGRImage aBGRImage, GrayImage aGrayImage, bool restart = false)
        {
            saveDebugImage(aBGRImage, "Original RGB Image");
            saveDebugImage(aGrayImage, "Original Gray Image");
            if (debugImageEdgeImprovements)
                saveDebugImage(aBGRImage.Canny(500, 300), "Original Edge");
            BGRImage bgrImage;
            GrayImage grayImage;
            if (preprocessorResize)
            {
                bgrImage = aBGRImage.Resize(300, 300, INTER.CV_INTER_LINEAR);
                grayImage = aGrayImage.Resize(300, 300, INTER.CV_INTER_LINEAR);
            }
            else
            {
                bgrImage = aBGRImage.Clone();
                grayImage = aGrayImage.Clone();
            }

            if (preprocessorRemoveBackground)
            {
                bgrImage = bgrImage.And(bgrImage, grayImage);
                saveDebugImage(bgrImage, "After Remove Background");
            }

            Image<Gray, Byte>[] splitimg = bgrImage.Convert<Hsv, Byte>().Split();
            saveDebugImage(splitimg[2], "Lightness");
            saveDebugImage(splitimg[1], "Saturation");

            if (preprocessorRemoveSaturation)
            {

                splitimg[1]._ThresholdBinaryInv(new Gray(100), new Gray(255));
                //splitimg[1]._And(splitimg[2].ThresholdBinary(new Gray(100), new Gray(255))); //NEW
                saveDebugImage(splitimg[1], "Binary Saturation");
                splitimg[2]._And(splitimg[1]);
                saveDebugImage(splitimg[2], "Lightness After Saturation removal");//AND HALL

            }
            grayImage = splitimg[2];

            if (preprocessorGammaCorrection)
            {
                grayImage._GammaCorrect(1.5);
            }

            if (preprocessorLightnessCorrection)
            {
                double scaler = determineScaler(grayImage);
                if (scaler > 4)
                    scaler = 4;
                if (scaler > 1)
                {
                    grayImage._Mul(scaler);
                    determineScaler(grayImage);
                    saveDebugImage(grayImage, "Corrected lightness");
                }
            }

            if (preprocessorRemoveBorder && !restart)
            {
                GrayImage returnImage = removeBorder(grayImage, splitimg[1]);

            /*    saveDebugImage(returnImage, "without hall");
                GrayImage one = new GrayImage(300, 300, new Gray(255));
                double mindotprot = returnImage.DotProduct(one);

                if (mindotprot < 1e7 && !restart)
                {
                    Console.WriteLine("restart");
                    GrayImage filledImage = aGrayImage.Copy();
                    BGRImage bbgrImage = aBGRImage.Copy();
                    for (var contour = filledImage.FindContours(CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE, RETR_TYPE.CV_RETR_CCOMP); contour != null; contour = contour.HNext)
                    {
                        Seq<Point> pointsSeq = contour.GetConvexHull(ORIENTATION.CV_CLOCKWISE);
                        Point[] points = pointsSeq.ToArray();
                        filledImage.FillConvexPoly(points, new Gray(255));
                    }
                    returnImage = preprocess(bbgrImage, filledImage, true);
                    //saveDebugImage(returnImage, "wit hall");
                    return returnImage;
                }*/
                grayImage = returnImage;
            }
            else
            {
                grayImage._GammaCorrect(2);
                grayImage = grayImage.ThresholdBinary(new Gray(80), new Gray(255));
            }

            if (preprocessorCrop)
            {
                grayImage = Utilities.stripBorder(grayImage, new Gray(128));
                grayImage = grayImage.Resize(300, 300, INTER.CV_INTER_NN);
            }

            splitimg[0].Dispose();
            splitimg[1].Dispose();
            splitimg[2].Dispose();
            bgrImage.Dispose();
       
            return grayImage.Canny(400, 300);
        }
        

        private Matrix<float> calculateParameters(GrayImage aimage)
        {
            Matrix<float> returnMatrix = new Matrix<float>(1, 128);

            int xAxisCursor = 0;

            Image<Gray, Byte> img = aimage.Copy();
            img = img.Resize(300, 300, INTER.CV_INTER_LINEAR);

            drawGrid(img);
            saveDebugImage(img, "Edge");

            if (calculateParametersSmoothGaussian)
                img._SmoothGaussian(5, 5, 1.5, 1.5);

            Size size = img.Size;
            int N = 300 / 4;
            Image<Gray, float> divImg2 = new Image<Gray, float>(3000, 300);
            Image<Gray, byte> histogram = new Image<Gray, byte>(640, 200);
            Point ppoint = new Point(0, 0);

            for (int r = 0; r < size.Height; r += N)
            {
                for (int c = 0; c < size.Width; c += N)
                {
                    Rectangle rect = new Rectangle(c, r, N, N);
                    Image<Gray, byte> cropImage = new Image<Gray, byte>(rect.Size);
                    cropImage = img.Copy(rect);

                    for (int n = 0; n < 8; n++)
                    {
                        Image<Gray, float> convolutedImage = cropImage.Convolution(cKernelList[n]);

                        convolutedImage._ThresholdBinary(new Gray(0.1), new Gray(1));
                        rect = new Rectangle(c + n * 300, r, N, N);
                        divImg2.ROI = rect;
                        CvInvoke.cvCopy(convolutedImage.Mul(255), divImg2, IntPtr.Zero);
                        divImg2.ROI = Rectangle.Empty;

                        Image<Gray, float> one = new Image<Gray, float>(75, 75, new Gray(1));
                        double dotprot = convolutedImage.DotProduct(one);

                        returnMatrix[0, xAxisCursor] = (float)dotprot;
                        if (!isTraining)
                        {
                            histogram.Draw(new LineSegment2D(ppoint, new Point(xAxisCursor * 5, 200 - (int)dotprot / 2)), new Gray(255), 1);
                            ppoint = new Point(xAxisCursor * 5, 200 - (int)dotprot / 2);

                        }
                        xAxisCursor++;
                        one.Dispose();
                        convolutedImage.Dispose();
                    }
                    cropImage.Dispose();
                }
            }

            drawGrid(img);

            saveDebugImage(GWOutputImage, "Gabor wavelet");
            saveDebugImage(img, "Input Edge");
            saveDebugImage(divImg2, "Different directions");
            saveDebugImage(histogram, "LESH histogram");

            divImg2.Dispose();
            histogram.Dispose();
            img.Dispose();
            return returnMatrix;
        }

        private void saveDebugImage(IImage image, string name)
        {
            if (isTraining)
                return;
            if (debugImages == null)
            {
                // if (!Directory.Exists(debugFolder))
                // {
                //     Directory.CreateDirectory(debugFolder);
                // }
                // image.Save(debugFolder + name + ".png");
                return;
            }
            DebugImage debugImage = new DebugImage();
            debugImage.Image = (IImage)image.Clone();
            debugImage.Name = name;
            debugImages.Add(debugImage);
        }

        
        //TODO: LIST??
        public void train(List<FeatureExample> images)
        {
            isTraining = true;
            int imagesCount = images.Count();
            if (imagesCount < 1)
                return;
            Console.WriteLine("Start to train " + imagesCount + " images");

            List<FeatureExampleElement>[] trainlist = new List<FeatureExampleElement>[signShapeCount];
            for (int i = 0; i < trainlist.Count(); i++)
                trainlist[i] = new List<FeatureExampleElement>();

            while(images.Count() > 0)
            {
                if (images.Count() % 20 == 0)
                    Console.WriteLine("Images left to train: " + images.Count().ToString());

                FeatureExample image = images[0];
                debugFolder = image.directory + image.name + "\\";
                //GrayImage preprosessedImage = preprocess(image);
                GrayImage preprosessedImage = preprocess(image.rgbImage, image.grayImage);
               //CvInvoke.cvShowImage("Preprossed Image", preprosessedImage);

               FeatureExampleElement element = new FeatureExampleElement();
               element.type = image.type;
               element.parameter = calculateParameters(preprosessedImage);

               SignShape shape = image.shape;
               //TODO: change??
               if (shape == SignShape.Garbage)
               {
                   for (int j = 0; j < trainlist.Count(); j++)
                       trainlist[j].Add(element);
               }
               else
                    trainlist[(int)shape].Add(element);

               if (shape == SignShape.Octagon)
                   trainlist[(int)SignShape.Circle].Add(element);

                images.Remove(image);
                image.rgbImage.Dispose();
                image.grayImage.Dispose();
            }
            trainSVM(trainlist);
            Console.WriteLine("Finised with Training");
            isTraining = false;
        }

        public void test(List<FeatureExample> images)
        {
            int imagesCount = images.Count();
            int garbageCorrect = 0;
            int garbageIncorrect = 0;
            int correct = 0;
            int incorrect = 0;
            while (images.Count() > 0)
            {
                if (images.Count() % 20 == 0)
                    Console.WriteLine("Images left to test: " + images.Count().ToString());

                FeatureExample image = images[0];
                SignType detectedSign = recognizeSign(image.rgbImage, image.grayImage, image.shape, image.color);
                if (image.type == SignType.Garbage)
                {
                    if (detectedSign == SignType.Garbage)
                        garbageCorrect++;
                    else
                        garbageIncorrect++;
                }
                else
                {
                    if (detectedSign == image.type)
                        correct++;
                    else
                        incorrect++;
                }
                images.Remove(image);
            }
            MessageBox.Show("Garbage Incorrect: " + garbageIncorrect.ToString() + "\n " + "Correct: " + correct.ToString() + "\n " + "Incorrect: " + incorrect.ToString());
        }

        private void trainSVM(List<FeatureExampleElement>[] trainlist)
        {
            isTrained = true;
            for (int i = 1; i < trainlist.Count(); i++)
            {
                List<FeatureExampleElement> featureExampleElementList = trainlist.ElementAt(i);
                int count = featureExampleElementList.Count();
                Matrix<float> trainData = new Matrix<float>(count, 128);
                Matrix<float> trainClasses = new Matrix<float>(count, 1);

                if (featureExampleElementList.Count() < 1)
                {
                    MessageBox.Show("Please provide a complete training dataset. Dataset for " + ((SignShape)i).ToString() + " is missing");
                    isTrained = false;
                    return;
                }
                for (int j = 0; j < featureExampleElementList.Count(); j++)
                {
                    FeatureExampleElement featureExampleElement = featureExampleElementList.ElementAt(j);
                    Matrix<float> parameters = featureExampleElement.parameter;
                    //TODO: Beter fuksie kry
                    for (int k = 0; k < featureVectorDimension; k++)
                        trainData[j, k] = parameters[0, k];
                    trainClasses[j, 0] = (float)featureExampleElement.type;
                }
                bool trained = SVMModels[i].Train(trainData, trainClasses, null, null, SVMParameters);
                //bool trained = SVMModels[i].TrainAuto(trainData, trainClasses, null, null, paramsm, 100);
                isTrained = isTrained & trained;
                //trainData.Add(entry.Value.First().parameter);
            }
            //Console.WriteLine("Trained: " + isTrained.ToString());
        }

        public GrayImage removeBorder(GrayImage agrayImage, GrayImage splitimg, bool hull = false)
        {
            GrayImage grayImage = agrayImage.Copy();
            GrayImage tempslit = splitimg.Copy();
            GrayImage tempGray = grayImage.ThresholdBinary(new Gray(120), new Gray(255));
            saveDebugImage(tempGray, "Invers BW Gray");

            tempslit._ThresholdBinaryInv(new Gray(100), new Gray(255));
            GrayImage filledImage = tempslit.Copy();
            if (!hull)
            {
                for (var contour = tempslit.FindContours(CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE, RETR_TYPE.CV_RETR_CCOMP); contour != null; contour = contour.HNext)
                {
                    Point[] points = contour.ToArray();
                    filledImage.FillConvexPoly(points, new Gray(255));
                }
            }
            else
            {

            }
            filledImage._ThresholdBinaryInv(new Gray(128), new Gray(255));
            filledImage._Or(tempslit);
            tempGray._Or(filledImage);
            tempGray._ThresholdBinaryInv(new Gray(100), new Gray(255));
            saveDebugImage(tempGray, "White added before grading");
            tempGray = removeGradient(tempGray);
            saveDebugImage(tempGray, "White Added and Inverted");
            GrayImage one = new GrayImage(300, 300, new Gray(255));
            double mindotprot = tempGray.DotProduct(one);

            if (mindotprot < 1e8)
            {
                filledImage._ThresholdBinaryInv(new Gray(128), new Gray(255));
                saveDebugImage(filledImage, "filled image");
                grayImage._And(filledImage);
                grayImage = removeGradient(grayImage);
                tempGray.Dispose();
            }
            else
                grayImage = tempGray;

            saveDebugImage(grayImage, "Border Removed");
            filledImage.Dispose();
            one.Dispose();
            return grayImage;
        }

        public SignType recognizeSign(BGRImage BGRimage, GrayImage grayImage, SignShape shape, SignColour color, List<DebugImage> aDebugImage = null)
        {
            if (shape == SignShape.Garbage && aDebugImage == null)
                return SignType.Garbage;

            debugImages = aDebugImage;
            GrayImage preprosessedImage = preprocess(BGRimage, grayImage);
            Matrix<float> parameter = calculateParameters(preprosessedImage);

            SignType classifiedSign = classifySign(parameter, shape);

            bool boolColor;
            if (color == SignColour.RED)
                boolColor = false;
            else boolColor = true;

             if (isTrained && shape != SignShape.Garbage)
            {
                if (boolColor != SignTypeColor[(int)classifiedSign])
                    //Console.WriteLine("Color Mishaps");
                    return SignType.Garbage;
                return classifiedSign;
            }
            else
                return SignType.Garbage;
        }

        private SignType classifySign(Matrix<float> parameters, SignShape shape)
        {
            if (shape == SignShape.Garbage)
                return SignType.Garbage;

            return (SignType)(int)SVMModels[(int)shape].Predict(parameters);
        }

        public void exportModels(string exportDir)
        {
            foreach (SignShape shape in Enum.GetValues(typeof(SignShape)))
            {
                if (shape == SignShape.Garbage) continue;
                string modelPath = Path.Combine(exportDir, "SignType-" + shape.ToString() + ".xml");
                SVMModels[(int)shape].Save(modelPath);
            }
        }

        public void importModels(string importDir)
        {
            isTrained = true;

            foreach (SignShape shape in Enum.GetValues(typeof(SignShape)))
            {
                if (shape == SignShape.Garbage) continue;
                string modelPath = Path.Combine(importDir, "SignType-" + shape.ToString() + ".xml");
                if (File.Exists(modelPath))
                    SVMModels[(int)shape].Load(modelPath);
                else
                    isTrained = false;
            }
        }

        public static List<FeatureExample> extractExamplesFromDirectory(string dir)
        {
            Debug.WriteLine("Loading test Directory example " + dir);
            List<FeatureExample> examples = new List<FeatureExample>();

            if (System.IO.Directory.Exists(dir))
            {
                string[] files = Utilities.GetFiles(dir, "*BW.jpg|*BW.png", SearchOption.AllDirectories);
                int i = 0;
                foreach (string bwFile in files)
                {
                    string rgbFile = bwFile.Replace("_BW", "_RGB");

                    if (!System.IO.File.Exists(rgbFile))
                        continue;

                    string[] pathDirectories = bwFile.Split(Path.DirectorySeparatorChar);
                    int numPathDirectories = pathDirectories.Count();

                    if (numPathDirectories < 3)
                        continue;

                    string signTypeString = pathDirectories[numPathDirectories - 2];
                    string signShapeString = pathDirectories[numPathDirectories - 3];

                    SignShape shape;
                    if (Enum.IsDefined(typeof(SignShape), signShapeString))
                        shape = (SignShape)Enum.Parse(typeof(SignShape), signShapeString);
                    else
                        continue;

                    SignType type;
                    if (Enum.IsDefined(typeof(SignType), signTypeString))
                        type = (SignType)Enum.Parse(typeof(SignType), signTypeString);
                    else
                        continue;

                    string[] nameTokens = Path.GetFileNameWithoutExtension(bwFile).Split('_');
                    if (nameTokens.Count() < 3)
                        continue;

                    string signColorString = nameTokens[nameTokens.Count() - 2];
                    SignColour color;

                    if (Enum.IsDefined(typeof(SignColour), signColorString))
                        color = (SignColour)Enum.Parse(typeof(SignColour), signColorString);
                    else
                        continue;

                    FeatureExample example = new FeatureExample();
                    //TODO:
                    example.name = (i++).ToString();
                    example.directory = dir;
                    // example.grayImage = new GrayImage(bwFile).Resize(300, 300, INTER.CV_INTER_LINEAR, false);
                    // example.rgbImage = new Image<Bgra, Byte>(rgbFile).Convert<Bgr, Byte>().Resize(300, 300, INTER.CV_INTER_LINEAR, false);
                    example.grayImage = new GrayImage(bwFile);
                    example.rgbImage = new Image<Bgr, Byte>(rgbFile);
                    example.color = color;
                    example.shape = shape;
                    example.type = type;

                    GC.Collect();

                    examples.Add(example);

                    if (i % 20 == 0)
                        Console.WriteLine("Files loaded: " + i.ToString());
                }
            }
            return examples;
        }
    }
}
