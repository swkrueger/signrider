using System;
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
        MinimumSpeedLimit50,
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
            this.Image = image;
            this.Name = name;
        }
    }

    public class FeatureRecognizer
    {
        // Constants
        private const int featureVectorDimension = 128;

        // Preprocessor settings
/*        private bool preprocessorResize = true;
        private bool preprocessorAddBorder = true;
        private bool preprocessorOpenFilter = false;
        private bool preprocessorPyr = true;
        private bool preprocessorBlur = true;
        private bool preprocessorStripBorder = true;
        private bool preprocessorConvexHullFill = false;

        // Debugging output settings
        private bool debugPlotFeatureVector = false;
        private bool debugPauseOnImageShow = true;
        private bool debugPreprocessor = false;*/

        // Private class variables
        //private SVM SvmModel = new SVM();
        //private SVMParams SvmParameters;
        List<SVM> SVMModels = new List<SVM>();
        MCvSVMParams paramsm;// = new MCvSVMParams();
        SVMParams SVMParameters;
        List<DebugImage> debugImages;
        string debugFolder;

        private int signShapeCount = Enum.GetValues(typeof(SignShape)).Length - 1;

        public bool isTrained { get; private set; }

        public FeatureRecognizer()
        {
            isTrained = false;

            SVMParameters = new SVMParams();
            SVMParameters.SVMType = Emgu.CV.ML.MlEnum.SVM_TYPE.C_SVC;
            //SVMParameters.SVMType = Emgu.CV.ML.MlEnum.SVM_TYPE.NU_SVC;
            //p.KernelType = Emgu.CV.ML.MlEnum.SVM_KERNEL_TYPE.POLY;
            //SVMParameters.KernelType = Emgu.CV.ML.MlEnum.SVM_KERNEL_TYPE.LINEAR;
            SVMParameters.KernelType = Emgu.CV.ML.MlEnum.SVM_KERNEL_TYPE.POLY;
            SVMParameters.Gamma = 3;
            SVMParameters.Degree = 3;
            SVMParameters.C = 1;
            SVMParameters.TermCrit = new MCvTermCriteria(100, 0.00001);
            paramsm = new MCvSVMParams();
            paramsm.kernel_type = Emgu.CV.ML.MlEnum.SVM_KERNEL_TYPE.POLY;
            //debugImages = new List<DebugImage>();
            debugImages = null;

            //SVMModel = new SVM();
           // foreach(SignShape shape in Enum.GetValues(typeof(SignShape)))
            for (int i = 0;i < signShapeCount;i++)
            {
                SVM model = new SVM();
                SVMModels.Add(model);
//                MessageBox.Show(shape.ToString() + " " + ((int)shape).ToString());
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

        private GrayImage preprocess(BGRImage aBGRImage, GrayImage aGrayImage)
        {
            BGRImage rgbiamge = aBGRImage.Resize(300, 300, INTER.CV_INTER_LINEAR);
            GrayImage grayiamge = aGrayImage.Resize(300, 300, INTER.CV_INTER_LINEAR);
            saveDebugImage(rgbiamge, "Original RGB Image");
            saveDebugImage(grayiamge, "Original Gray Image");
            saveDebugImage(rgbiamge.Canny(500, 300),"Original Edge");

            rgbiamge = rgbiamge.And(rgbiamge, grayiamge);
            saveDebugImage(rgbiamge,"After And");

            Image<Hsv, Byte> hlk = rgbiamge.Convert<Hsv, Byte>();
            Image<Gray, Byte>[] splitimg = hlk.Split();
            saveDebugImage(splitimg[2],"Lightness");
            saveDebugImage(splitimg[1], "Saturation");

            splitimg[1]._ThresholdBinaryInv(new Gray(100), new Gray(255));
            splitimg[2]._And(splitimg[1]);
            splitimg[1]._ThresholdBinary(new Gray(255), new Gray(255));
            splitimg[2]._GammaCorrect(1.5);


            int ijk = 10;
            Point point2 = new Point(0, 0);
            Image<Gray, Byte> histogram = new Image<Gray, Byte>(700,600);
            double som;
            double scaler = 1;
 
            Image<Gray, Byte> temp = splitimg[2];
            double count = 0;
            MCvScalar gemiddeld = CvInvoke.cvSum(temp);
            int gem = (int)(gemiddeld.v0 / 255 * 0.2);

            for (int n = 0; n <= 250; n+=5)
            {
                using (Image<Gray, Byte> ttemp = temp.ThresholdBinary(new Gray(250 - n), new Gray(255)))
                {
                    MCvScalar sum = CvInvoke.cvSum(ttemp);
                    som = sum.v0 / 255 - count;
                    //Debug.WriteLine("n: " + n.ToString() + " som: " + som.ToString() + " count: " + count);
                    count += som;

                    if (n > 100 && count < gem)
                        scaler = (double)n / (double)75;
                    
                    if (n != 0)
                        histogram.Draw(new LineSegment2D(point2, new Point(ijk * 10, 500 - (int)som / 20)), new Gray(128), 1);
                    point2 = new Point(ijk * 10, 500 - (int)som / 20);
                    ijk++;
                }
            }
            som = 300 * 300 - count;
            //Debug.WriteLine("som: " + som.ToString() + "count: " + count);
            histogram.Draw(new LineSegment2D(point2, new Point(ijk * 10, 500 - (int)som / 20)), new Gray(128), 1);
            saveDebugImage(histogram, "Lightness histogram");

            Debug.WriteLine("scaler: " + scaler + " gemiddeld: " + gem.ToString());

            splitimg[2]._Mul(scaler);
            
            if (scaler > 4)
                scaler = 4;
            if (scaler > 1)
            {
                //splitimg[2]._Mul(scaler);

                count = 0;
                ijk = 10;
                point2 = new Point(0, 0);
                histogram = new Image<Gray, Byte>(700, 600);
                for (int n = 0; n <= 250; n += 5)
                {
                    using (Image<Gray, Byte> ttemp = splitimg[2].ThresholdBinary(new Gray(250 - n), new Gray(255)))
                    {
                        MCvScalar sum = CvInvoke.cvSum(ttemp);
                        som = sum.v0 / 255 - count;
                        //Debug.WriteLine("n: " + n.ToString() + " som: " + som.ToString() + " count: " + count);
                        count += som;
                        if (n != 0)
                            histogram.Draw(new LineSegment2D(point2, new Point(ijk * 10, 500 - (int)som / 20)), new Gray(128), 1);
                        point2 = new Point(ijk * 10, 500 - (int)som / 20);
                        ijk++;
                    }
                }
                som = 300 * 300 - count;
                Debug.WriteLine("som: " + som.ToString() + "count: " + count);
                histogram.Draw(new LineSegment2D(point2, new Point(ijk * 10, 500 - (int)som / 20)), new Gray(128), 1);
                saveDebugImage(histogram,"Corrected ligntess histogram");
            }
            
            saveDebugImage(splitimg[2],"Corrected lightness");
            CvInvoke.cvMerge(splitimg[0], splitimg[1], splitimg[2], IntPtr.Zero, hlk);
            saveDebugImage( hlk.Convert<Bgr, Byte>(),"RGB image after color correction");

            StructuringElementEx element = new StructuringElementEx(1, 1, 0, 0, CV_ELEMENT_SHAPE.CV_SHAPE_ELLIPSE);
            element = new StructuringElementEx(2, 2, 0, 0, CV_ELEMENT_SHAPE.CV_SHAPE_ELLIPSE);
            splitimg[2]._MorphologyEx(element, CV_MORPH_OP.CV_MOP_CLOSE, 1);
            saveDebugImage(splitimg[2], "after Morph");
            saveDebugImage(splitimg[2].Canny(500, 300), "Edge after morph");

            Image<Gray, byte> grayImage = splitimg[2];
 
            Image<Gray, byte> paddedIamge= new Image<Gray, byte>(350,350);
            

            CvInvoke.cvCopyMakeBorder(grayImage, paddedIamge, new Point(25, 25), BORDER_TYPE.CONSTANT,new MCvScalar(0));
            paddedIamge._ThresholdBinary(new Gray(120), new Gray(255));
            saveDebugImage(paddedIamge, "Binary image (larger)");
            grayImage = paddedIamge.Copy();

            Image<Gray, byte> maskImage = new Image<Gray, byte>(350, 350);
             for (var contour = paddedIamge.FindContours(CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE, RETR_TYPE.CV_RETR_CCOMP); contour != null; contour = contour.HNext)
            {
                Seq<Point> pointsSeq = contour.GetConvexHull(ORIENTATION.CV_CLOCKWISE);
                Point[] points = pointsSeq.ToArray();
//                filledImage.FillConvexPoly(points, new Gray(255));
                MCvBox2D box = PointCollection.MinAreaRect(Array.ConvertAll(points,item => (PointF)item));
                
                
                //Console.WriteLine("hight: " + box.size.Height.ToString() + " Witdh: " + box.size.Width.ToString() + box.center.X.ToString());
                //Console.WriteLine("hight: " + box.size.Height.ToString() + " Witdh: " + box.size.Width.ToString());
                if (box.size.Height < 5 || box.size.Width < 5)
                    maskImage.FillConvexPoly(points, new Gray(255));

                    //grayImage.Draw(box, new Gray(0), -1);
                //if (box.size.Width < 5)
                //    grayImage.Draw(box, new Gray(0), -1);
                //grayImage.Draw(box, new Gray(255), 1);
            }

            maskImage._Not();
            paddedIamge._And(maskImage);
            grayImage._And(maskImage);
            maskImage._Not();
            paddedIamge._ThresholdBinaryInv(new Gray(128), new Gray(255));
                        for (var contour = paddedIamge.FindContours(CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE, RETR_TYPE.CV_RETR_CCOMP); contour != null; contour = contour.HNext)
             {
                 Seq<Point> pointsSeq = contour.GetConvexHull(ORIENTATION.CV_CLOCKWISE);
                 Point[] points = pointsSeq.ToArray();
                 //                filledImage.FillConvexPoly(points, new Gray(255));
                 MCvBox2D box = PointCollection.MinAreaRect(Array.ConvertAll(points, item => (PointF)item));

                 //Console.WriteLine("hight: " + box.size.Height.ToString() + " Witdh: " + box.size.Width.ToString());
                 if (box.size.Height < 5 || box.size.Width < 5)
                 {
                     paddedIamge.FillConvexPoly(points, new Gray(0));
                     maskImage.FillConvexPoly(points, new Gray(255));//TODO: ander mask maak
                 }

              ///   else filledImage.Draw(box, new Gray(0), 1);
             }
                        paddedIamge._ThresholdBinaryInv(new Gray(128), new Gray(255));
            //  BGRImage rgbiamge2 = hlk.Convert<Bgr, Byte>();

        saveDebugImage(paddedIamge, "Grading removed");
        //CvInvoke.cvShowImage("After hull2 mask", maskImage);
        //CvInvoke.cvShowImage("After hull2 canny", grayImage.Canny(400, 300));
        saveDebugImage(paddedIamge.Canny(400, 300), "Edge After bw Grading removed BW");
        saveDebugImage(grayImage.Canny(400, 300), "Edge After bw Grading removed Gray");

        rgbiamge.Dispose();
        grayiamge.Dispose();
        hlk.Dispose();
        maskImage.Dispose();
        for (int i = 0; i < 3; ++i) splitimg[i].Dispose();

        return paddedIamge.Canny(400, 300);
        }

        private Matrix<float> calculateParameters(GrayImage image)
        {
            double[] pointts = new double[128];

            Matrix<float> returnMatrix = new Matrix<float>(1, 128);

            int ijk = 0;
            Image<Gray, double> GW = new Image<Gray, double>(1, 1);
            int R = 10;
            int C = 10;
            // double u = 2;
            double v = 2;
            for (int i = 0; i < 8; i++)
            {
                GW = GW.ConcateHorizontal(GaborWavelet(R, C, i, v));
            }
            saveDebugImage(GW.Convert<Gray, Byte>(), "Gabor wavelet");

            //img.SmoothGaussian(5, 5, 1.5, 1.5);
            Image<Gray, Byte> img = image.Copy(new Rectangle(25,25,300,300));
            img = img.Resize(300, 300, INTER.CV_INTER_LINEAR);
            Image<Gray, byte> edge = image.Resize(300,300, INTER.CV_INTER_LINEAR);

            //Size size = img.Size;
            Size size = edge.Size;
            int N = 300 / 4;
            Image<Gray, byte> divImg = new Image<Gray, byte>(300, 300);
            Image<Gray, float> divImg2 = new Image<Gray, float>(3000, 300);

            Image<Gray, float> histogram = new Image<Gray, float>(640, 200);
            Point ppoint = new Point(0, 0);

            for (int r = 0; r < size.Height; r += N)
            {
                for (int c = 0; c < size.Width; c += N)
                {
                    Rectangle rect = new Rectangle(c, r, N, N);
                    Image<Gray, byte> testCrop = new Image<Gray, byte>(rect.Size);
                    //testCrop = img.Copy(rect).Convert<Gray, byte>();
                    testCrop = edge.Copy(rect);
                    //divImg = divImg.Copy(new Image<Gray, byte>(20, 300/4, new Gray(125)));
                    divImg.ROI = rect;
                    // divImg.Add(testCrop);
                    CvInvoke.cvCopy(testCrop, divImg, IntPtr.Zero);
                    divImg.ROI = Rectangle.Empty;
                    //CvInvoke.cvShowImage(r.ToString(), testCrop);

                    Image<Gray, float> testCrop2 = testCrop.Convert<Gray, float>();
                    for (int n = 0; n < 8; n++)
                    {
                        //MessageBox.Show(testCrop2.Data.GetType().ToString());
                        ConvolutionKernelF ckernel = new ConvolutionKernelF(10, 10);
                        for (int l = 0; l < 10; l++)
                            for (int k = 0; k < 10; k++)
                                ckernel[l, k] = (float)GW[l, k + n * 10].Intensity / 10000;
                        ckernel.Center = new Point(5, 5);
                        // MessageBox.Show(((float)GW[10, 30].Intensity).ToString());
                        //for (int l = 0; l < 10; l++)
                        //   for (int k = 0; k < 10; k++)
                        //      testCrop2[l, k].Intensity = (Gray)GW[l, k + 30].Intensity;

                        //testCrop = new Image<Gray, byte>(200, 200);
                        //Image<Gray, float> testCrop21 = new Image<Gray, float>(10, 10);
                        //Image<Gray, float> testCrop3 = new  Image<Gray, float>(75, 75);
                        Image<Gray, float> testCrop3 = testCrop.Convolution(ckernel);

                        testCrop3._ThresholdBinary(new Gray(0.1), new Gray(1));
                        //CvInvoke.cvShowImage("Testcrop", testCrop);
                        //CvInvoke.cvShowImage("Testcrop2", testCrop2);
                        //CvInvoke.cvShowImage("Testcrop3", testCrop3);
                        rect = new Rectangle(c + n * 300, r, N, N);
                        divImg2.ROI = rect;
                        CvInvoke.cvCopy(testCrop3, divImg2, IntPtr.Zero);
                        divImg2.ROI = Rectangle.Empty;

                        Image<Gray, float> one = new Image<Gray, float>(75, 75, new Gray(1));
                        double dotprot = testCrop3.DotProduct(one);
                        // Debug.WriteLine(dotprot);


                        //Contour<Point> points;// = new Point[10];
                        Point[] points = new Point[128];
                        points[ijk] = new Point(ijk * 2, 1000 - (int)dotprot);
                        pointts[ijk] = dotprot;
                        returnMatrix[0, ijk] = (float)pointts[ijk];
 ///                       trainData[T, ijk] = (float)dotprot;
                        //points[ijk * 2 + 1] = new Point(points[ijk * 2 + 1]);
                        int intValue = 1;
                        bool result = intValue == 1;
                        //histogram.DrawPolyline(points, result, new Gray(1), 1);
                        histogram.Draw(new LineSegment2D(ppoint, new Point(ijk * 5, 200 - (int)dotprot / 2)), new Gray(1), 1);
                        ppoint = new Point(ijk * 5, 200 - (int)dotprot / 2);
                        ijk++;

                        one.Dispose();
                        testCrop3.Dispose();
                    }

                    testCrop.Dispose();
                }
            }

            drawGrid(img);

            saveDebugImage(img, "Test Window3");
            saveDebugImage(divImg2, "Test Window4");
            //divImg2 = divImg2.Resize(0.5, INTER.CV_INTER_LINEAR);
            saveDebugImage(histogram, "LESH histogram");

            drawGrid(edge);
            //CvInvoke.cvShowImage("edge", edge);
            saveDebugImage(edge, "edge");

            divImg2.Dispose();
            histogram.Dispose();
            img.Dispose();
            edge.Dispose();

            // FIXME: This can cause problems!!
            GC.Collect();

            return returnMatrix;

        }

        private void saveDebugImage(IImage image, string name)
        {
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
            debugImage.Image = image;
            debugImage.Name = name;
            debugImages.Add(debugImage);
        }

        
        //TODO: LIST??
        public void train(List<FeatureExample> images)
        {
            int imagesCount = images.Count();
            if (imagesCount < 1)
                return;
            Console.WriteLine("Start to train " + imagesCount + " images");

            List<FeatureExampleElement>[] trainlist = new List<FeatureExampleElement>[signShapeCount];
            for (int i = 0; i < trainlist.Count(); i++)
                trainlist[i] = new List<FeatureExampleElement>();

            for (int i = 0; i < imagesCount; i++)
            {
                FeatureExample image = images[i];
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
               
            }
            trainSVM(trainlist);
       /*     Console.WriteLine("Start SVM training");
            Console.WriteLine(trainData.Rows.ToString() + " " + trainData.Cols.ToString() + " " + trainClasses.Rows.ToString() + " " + trainClasses.Cols.ToString());
            trainSVM(trainData, trainClasses);*/
            Console.WriteLine("DONE");
        }

        public void test(List<FeatureExample> images)
        {
  /*          int imagesCount = images.Count();
            Matrix<SignShape, float> trainClasses = new Matrix<float>(1, 128);
            for (int i = 0; i < imagesCount; i++)
            {
                FeatureExample image = images[i];
                //GrayImage preprosessedImage = preprocess(image);
                GrayImage preprosessedImage = preprocess(image.rgbImage, image.grayImage);
                Matrix<float> parameter = calculateParameters(preprosessedImage);
                //for (int j = 0; j < 128; j++)
                //    trainClasses[0,j] = parameter[0, j];

                //SVMModel.Predict(trainClasses);
                MessageBox.Show("Image " + i.ToString() + ": " + classifySign(parameter, image.shape).ToString());
                Console.WriteLine("DONE");
            }*/
        }

        private void trainSVM(List<FeatureExampleElement>[] trainlist)
        {
            isTrained = true;
            //foreach (List<FeatureExampleElement> featureExampleElementList in trainlist)
            for (int i = 0; i < trainlist.Count(); i++)
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
                //foreach (FeatureExampleElement featureExampleElement in featureExampleElementList)
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
                isTrained = isTrained & trained;
                //trainData.Add(entry.Value.First().parameter);
            }
            /*
            bool trained = SVMModel.Train(para, signType, null, null, SVMParameters);
            //bool trained = SVMModel.TrainAuto(para, signType, null, null, paramsm, 3);*/
            Console.WriteLine("Trained: " + isTrained.ToString());
        }

        public SignType recognizeSign(BGRImage BGRimage, GrayImage grayImage, SignShape shape, List<DebugImage> aDebugImage = null)
        {
            debugImages = aDebugImage;
            GrayImage preprosessedImage = preprocess(BGRimage, grayImage);
            Matrix<float> parameter = calculateParameters(preprosessedImage);

            return classifySign(parameter, shape);
        }

        private SignType classifySign(Matrix<float> parameters, SignShape shape)
        {
            return (SignType)(int)SVMModels[(int)shape].Predict(parameters);
            //return SignType.Garbage;
        }

        public static List<FeatureExample> extractExamplesFromDirectory(string dir)
        {
            Debug.WriteLine("Loading test Directory example " + dir);
            //debugFolder = new string(dir);
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

                    Debug.WriteLine("Loaded image: " + bwFile);
                    Debug.Flush();

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
                    example.grayImage = new GrayImage(bwFile);
                    example.rgbImage = new BGRImage(rgbFile);
                    example.color = color;
                    example.shape = shape;
                    example.type = type;

                    examples.Add(example);

                    //Debug.WriteLine(shapeString + " " + typeString + " " + colorString);
                }
            }
            return examples;
        }
    }
}
