﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

using System.Diagnostics;

using System.Windows.Forms;
using BGRImage = Emgu.CV.Image<Emgu.CV.Structure.Bgr, System.Byte>;
using GrayImage = Emgu.CV.Image<Emgu.CV.Structure.Gray, System.Byte>;


namespace Signrider
{
    //TODO:
    public enum SignType { SpeedLimit30, Stop };

    class FeatureRecognizer
    {
        public FeatureRecognizer()
        {
            p = new SVMParams();
            p.SVMType = Emgu.CV.ML.MlEnum.SVM_TYPE.C_SVC;
            //p.KernelType = Emgu.CV.ML.MlEnum.SVM_KERNEL_TYPE.POLY;
            p.KernelType = Emgu.CV.ML.MlEnum.SVM_KERNEL_TYPE.LINEAR;
            //p.Gamma = 3;
            p.C = 1;
            p.TermCrit = new MCvTermCriteria(100, 0.00001);
        }
        Image<Bgr, Byte> img2;
        public Image<Bgr, Byte> getImg()
        {
            return img2;
        }
        SVMParams p;

        public void helloTest()
        {
            using (Image<Bgr, Byte> img = new Image<Bgr, byte>(400, 200, new Bgr(255, 0, 255)))
            {
                //Create the font
                MCvFont f = new MCvFont(FONT.CV_FONT_HERSHEY_COMPLEX, 1.0, 1.0);

                //Draw "Hello, world." on the image using the specific font
                img.Draw("Hendri se: \"Hello, world\"", ref f, new Point(10, 80), new Bgr(255, 255, 0));

                //Show the image using ImageViewer from Emgu.CV.UI
                // CvInvoke.cvShowImage(img, "Test Window2");
                CvInvoke.cvShowImage("Hello World Test Window", img.Ptr);
            }
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
                    //Debug.WriteLine(a.ToString());
                    GW[y, x] = color;
                }
            }

            //Debug.WriteLine(k.ToString());
            return GW;
        }

        private void preFilter()
        {

        }

        private GrayImage preprocess(TestImage aimage)
        {
            BGRImage rgbiamge = aimage.rgbImage.Resize(300, 300, INTER.CV_INTER_LINEAR);
            GrayImage grayiamge = aimage.grayImage.Resize(300, 300, INTER.CV_INTER_LINEAR);
            //BGRImage rgbiamge = aimage.rgbImage.Resize(300, 300, INTER.CV_INTER_CUBIC);
            //GrayImage grayiamge = aimage.grayImage.Resize(300, 300, INTER.CV_INTER_CUBIC);

            GrayImage image = grayiamge;
            CvInvoke.cvShowImage("Original", rgbiamge);
            CvInvoke.cvShowImage("Original gray", grayiamge);
            CvInvoke.cvShowImage("BEFORE", rgbiamge.Canny(500, 300));
            CvInvoke.cvShowImage("Before hull", image);
            GrayImage filledImage = image;
            /*
            for (var contour = image.FindContours(CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE, RETR_TYPE.CV_RETR_CCOMP); contour != null; contour = contour.HNext)
            {
                Seq<Point> pointsSeq = contour.GetConvexHull(ORIENTATION.CV_CLOCKWISE);
                Point[] points = pointsSeq.ToArray();
                filledImage.FillConvexPoly(points, new Gray(255));
            }*/
            image = filledImage;
            CvInvoke.cvShowImage("After hull", image);
            //if (debugPreprocessor)
            //    showImage(image, "After hull");

            rgbiamge = rgbiamge.And(rgbiamge, grayiamge);
            int i = (int)(300 * 0.10);
            Rectangle rect = new Rectangle(i / 2, i / 2, 300 - i, 300 - i);
            rgbiamge = rgbiamge.GetSubRect(rect);
            CvInvoke.cvShowImage("Test Window1", rgbiamge);
            //rgbiamge._EqualizeHist();
            CvInvoke.cvShowImage("Test Window1 EQL", rgbiamge);
            CvInvoke.cvShowImage("Test Window2", rgbiamge.Canny(500, 300));

            //CvInvoke.cvShowImage("Test Window1", rgbiamge.Convert(.ThresholdAdaptive((new Rgb(250, 250, 250), new Rgb(200, 0, 0)));
            // CvInvoke.cvShowImage("Test Window3", rgbiamge.ThresholdToZero(new Bgr(220, 220, 220)));
            //Image<Hls, Byte> hlk = new Image<Hls, Byte>(new Size(300, 300));
            Image<Hsv, Byte> hlk = rgbiamge.Convert<Hsv, Byte>();
            //hlk = hlk.ThresholdToZero(new Hls(0, 150, 0));
            Image<Gray, Byte>[] splitimg = hlk.Split();
            CvInvoke.cvShowImage("Test light", splitimg[2]);
            CvInvoke.cvShowImage("Test sha", splitimg[1]);
            //TODO: avarage color insert
            //splitimg[2]._EqualizeHist();
            //splitimg[2]._GammaCorrect(0.5d);
            //Console.WriteLine("max ligtess: "  +splitimg[2].ma);
            //CvInvoke.cvShowImage("Test light after hosto", splitimg[2]);
            //splitimg[2] = splitimg[2].ThresholdAdaptive(new Gray(255), Emgu.CV.CvEnum.ADAPTIVE_THRESHOLD_TYPE.CV_ADAPTIVE_THRESH_MEAN_C,Emgu.CV.CvEnum.THRESH.CV_THRESH_BINARY,31, new Gray(180));

            splitimg[1] = splitimg[1].ThresholdBinaryInv(new Gray(150), new Gray(255));
            splitimg[2] = splitimg[2].And(splitimg[1]);
            splitimg[1] = splitimg[1].ThresholdBinary(new Gray(255), new Gray(255));

           // DenseHistogram histo = new DenseHistogram(255, new RangeF(0, 255));
           // histo.Calculate(new Image<Gray, Byte>[] { splitimg[2] }, true, null);
            //Console.WriteLine("histo dim: " + histo.BinDimension.Count().ToString);
            //Console.WriteLine("Average: "CvInvoke.cvAvg(splitimg[2]));

            //splitimg[2] = splitimg[2].ThresholdToZero(new Gray(150));

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
                Image<Gray, Byte> ttemp = temp.ThresholdBinary(new Gray(250 - n),new Gray(255));
                MCvScalar sum = CvInvoke.cvSum(ttemp);
                som = sum.v0 / 255 - count;
                Debug.WriteLine("n: " + n.ToString() + " som: " + som.ToString() + " count: " + count);
                count += som;

                if (n > 100 && count < gem)
                    scaler = (double)n / (double)70;
                
               // Image<Gray, float> one = new Image<Gray, float>(75, 75, new Gray(1));

                //Contour<Point> points;// = new Point[10];
                //Point[] points = new Point[128];
               // points[ijk] = new Point(ijk * 2, 1000 - (int)som / 10);
                //pointts[ijk] = dotprot;
                //trainData[T, ijk] = (float)dotprot;
                //points[ijk * 2 + 1] = new Point(points[ijk * 2 + 1]);
                int intValue = 1;
                bool result = intValue == 1;
                //histogram.DrawPolyline(points, result, new Gray(1), 1);
                if (n != 0)
                    histogram.Draw(new LineSegment2D(point2, new Point(ijk * 10, 500 - (int)som / 20)), new Gray(128), 1);
                point2 = new Point(ijk * 10, 500 - (int)som / 20);
                ijk++;
            }
            som = 300 * 300 - count;
            Debug.WriteLine("som: " + som.ToString() + "count: " + count);
            histogram.Draw(new LineSegment2D(point2, new Point(ijk * 10, 500 - (int)som / 20)), new Gray(128), 1);
            CvInvoke.cvShowImage("histogram", histogram);

            Debug.WriteLine("scaler: " + scaler + " gemiddeld: " + gem.ToString());

            if (scaler > 4)
                scaler = 4;
            if (scaler > 1)
            {
                splitimg[2]._Mul(scaler);

                count = 0;
                ijk = 10;
                point2 = new Point(0, 0);
                histogram = new Image<Gray, Byte>(700, 600);
                for (int n = 0; n <= 250; n += 5)
                {
                    Image<Gray, Byte> ttemp = temp.ThresholdBinary(new Gray(250 - n), new Gray(255));
                    MCvScalar sum = CvInvoke.cvSum(ttemp);
                    som = sum.v0 / 255 - count;
                    Debug.WriteLine("n: " + n.ToString() + " som: " + som.ToString() + " count: " + count);
                    count += som;


                    // Image<Gray, float> one = new Image<Gray, float>(75, 75, new Gray(1));

                    //Contour<Point> points;// = new Point[10];
                    //Point[] points = new Point[128];
                    // points[ijk] = new Point(ijk * 2, 1000 - (int)som / 10);
                    //pointts[ijk] = dotprot;
                    //trainData[T, ijk] = (float)dotprot;
                    //points[ijk * 2 + 1] = new Point(points[ijk * 2 + 1]);
                    int intValue = 1;
                    bool result = intValue == 1;
                    //histogram.DrawPolyline(points, result, new Gray(1), 1);
                    if (n != 0)
                        histogram.Draw(new LineSegment2D(point2, new Point(ijk * 10, 500 - (int)som / 20)), new Gray(128), 1);
                    point2 = new Point(ijk * 10, 500 - (int)som / 20);
                    ijk++;
                }
                som = 300 * 300 - count;
                Debug.WriteLine("som: " + som.ToString() + "count: " + count);
                histogram.Draw(new LineSegment2D(point2, new Point(ijk * 10, 500 - (int)som / 20)), new Gray(128), 1);
                CvInvoke.cvShowImage("histogram2", histogram);
            }
            
            //splitimg[1] = splitimg[1].ThresholdToZeroInv(new Gray(150));
            // splitimg[2]._Not();

            //splitimg[2] = splitimg[2].ThresholdBinary(new Gray(150), new Gray(255));
            CvInvoke.cvShowImage("Test light2", splitimg[2]);
            CvInvoke.cvShowImage("Test sha2", splitimg[1]);
            CvInvoke.cvMerge(splitimg[0], splitimg[1], splitimg[2], IntPtr.Zero, hlk);
            CvInvoke.cvShowImage("Test Window3", hlk.Convert<Bgr, Byte>());
            //hlk._MorphologyEx();
            //StructuringElementEx element = new StructuringElementEx(5, 5, 0, 0, CV_ELEMENT_SHAPE.CV_SHAPE_ELLIPSE);
            StructuringElementEx element = new StructuringElementEx(1, 1, 0, 0, CV_ELEMENT_SHAPE.CV_SHAPE_ELLIPSE);
            //hlk._MorphologyEx(element, CV_MORPH_OP.CV_MOP_OPEN, 2);
            //hlk._Erode(1);
            element = new StructuringElementEx(2, 2, 0, 0, CV_ELEMENT_SHAPE.CV_SHAPE_ELLIPSE);
            //hlk._MorphologyEx(element, CV_MORPH_OP.CV_MOP_OPEN, 1);
            hlk._MorphologyEx(element, CV_MORPH_OP.CV_MOP_CLOSE, 1);
            splitimg[2]._MorphologyEx(element, CV_MORPH_OP.CV_MOP_CLOSE, 1);

           
           //  hlk._Dilate(1);
            //TODO: FILL small holes
            // ._close
            //TODO: Remove tidy dots (better than holrs)
            //TODO:GEEL
            // TODO:crop
            //TODO: adaptive white threshold

            //CvInvoke.cvShowImage("hlk canny", hlk.Convert<Bgr, Byte>().Canny(20, 10));
            CvInvoke.cvShowImage("hlk canny", hlk.Convert<Bgr, Byte>().Canny(500, 300));

            CvInvoke.cvShowImage("Test Window4", hlk.Convert<Bgr, Byte>());

            Image<Gray, byte> grayImage = hlk.Convert<Gray, byte>();
            grayImage = splitimg[2];
            CvInvoke.cvShowImage("Test Window5", grayImage);

            Image<Gray, byte> paddedIamge= new Image<Gray, byte>(350,350);

            /*paddedIamge.ROI = new Rectangle(0, 0,300 , 300);
            //paddedIamge = paddedIamge.Add(grayImage);
            CvInvoke.cvCopy(grayImage,paddedIamge, IntPtr.Zero);
            paddedIamge.ROI = Rectangle.Empty;*/
            CvInvoke.cvCopyMakeBorder(grayImage, paddedIamge, new Point(25, 25), BORDER_TYPE.CONSTANT,new MCvScalar(0));
            grayImage = paddedIamge.Copy();

            //grayImage.CopyTo(
            paddedIamge._ThresholdBinary(new Gray(120), new Gray(255));
            //CvInvoke.cvShowImage("To B&W", paddedIamge);

            Image<Gray, byte> maskImage = new Image<Gray, byte>(350, 350);
             for (var contour = paddedIamge.FindContours(CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE, RETR_TYPE.CV_RETR_CCOMP); contour != null; contour = contour.HNext)
            {
                Seq<Point> pointsSeq = contour.GetConvexHull(ORIENTATION.CV_CLOCKWISE);
                Point[] points = pointsSeq.ToArray();
//                filledImage.FillConvexPoly(points, new Gray(255));
                MCvBox2D box = PointCollection.MinAreaRect(Array.ConvertAll(points,item => (PointF)item));
                
                
                //Console.WriteLine("hight: " + box.size.Height.ToString() + " Witdh: " + box.size.Width.ToString() + box.center.X.ToString());
                Console.WriteLine("hight: " + box.size.Height.ToString() + " Witdh: " + box.size.Width.ToString());
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

                 Console.WriteLine("hight: " + box.size.Height.ToString() + " Witdh: " + box.size.Width.ToString());
                 if (box.size.Height < 5 || box.size.Width < 5)
                 {
                     paddedIamge.FillConvexPoly(points, new Gray(0));
                     maskImage.FillConvexPoly(points, new Gray(255));//TODO: ander mask maak
                 }

              ///   else filledImage.Draw(box, new Gray(0), 1);
             }
                        paddedIamge._ThresholdBinaryInv(new Gray(128), new Gray(255));
            //  BGRImage rgbiamge2 = hlk.Convert<Bgr, Byte>();

        CvInvoke.cvShowImage("After hull2", grayImage);
        CvInvoke.cvShowImage("After hull2 paddd", paddedIamge);
        CvInvoke.cvShowImage("After hull2 mask", maskImage);
        CvInvoke.cvShowImage("After hull2 canny", grayImage.Canny(400, 300));
        CvInvoke.cvShowImage("After bw canny", paddedIamge.Canny(400, 300));
/*
        rgbiamge[0] = rgbiamge[0].And(grayImage);
        rgbiamge[1] = rgbiamge[1].And(grayImage);
        rgbiamge[2] = rgbiamge[2].And(grayImage);
            CvInvoke.cvShowImage("Final", rgbiamge);
            CvInvoke.cvShowImage("AFTER", rgbiamge.Canny(500, 300));*/

            return rgbiamge.Canny(500, 300);
            //         hlk[0]._And(;

            /*
                Contour<Point> contoura = grayImage.FindContours(CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE, RETR_TYPE.CV_RETR_EXTERNAL);
                {
                    Rectangle rect1 = contoura.BoundingRectangle;
                    //MessageBox.Show(rect1.ToString());
                    grayImage = grayImage.GetSubRect(rect1);
                    //grayImage.Draw(rect1, new Gray(128), -1);
                   // MessageBox.Show(contoura.First().ToString());

                }
                CvInvoke.cvShowImage("Test Windo6", grayImage);
                CvInvoke.cvShowImage("Test Window5", grayImage.Canny(500, 300));*/
            //     grayImage.Laplace(3);
            //    CvInvoke.cvShowImage("Test Windo7", grayImage);
            //CvInvoke.cvCvtColor(rgbiamge,hlk,

            
            /*
            switch (caseSwitch)
            {
                case 1:
                    Console.WriteLine("Case 1");
                    break;
                case 2:
                    Console.WriteLine("Case 2");
                    break;
                default:
                    Console.WriteLine("Default case");
                    break;
            }*/
        }

        private void calculateParameters()
        {

        }

        public void trainImage(TestImage image)
        {
            GrayImage preprosessedImage = preprocess(image);
            CvInvoke.cvShowImage("Preprossed Image", preprosessedImage);
            
        }

        //showCalulated parameters
        public void doTest()
        {

            return;

            string[] filenames = new string[20];
            filenames[0] = "C:\\Users\\Hendrik\\Dropbox\\VB(suck)net\\Test\\Test images\\Reconition\\uk30_1_bw.png";
            filenames[1] = "C:\\Users\\Hendrik\\Dropbox\\VB(suck)net\\Test\\Test images\\Reconition\\uk30_2_bw.png";
            filenames[2] = "C:\\Users\\Hendrik\\Dropbox\\VB(suck)net\\Test\\Test images\\Reconition\\uk30_3_bw.png";
            filenames[3] = "C:\\Users\\Hendrik\\Dropbox\\VB(suck)net\\Test\\Test images\\Reconition\\uk30_4_bw.png";
            filenames[4] = "C:\\Users\\Hendrik\\Dropbox\\VB(suck)net\\Test\\Test images\\Reconition\\uk40_1_bw.png";
            filenames[5] = "C:\\Users\\Hendrik\\Dropbox\\VB(suck)net\\Test\\Test images\\Reconition\\uk40_2_bw.png";
            filenames[6] = "C:\\Users\\Hendrik\\Dropbox\\VB(suck)net\\Test\\Test images\\Reconition\\uk40_3_bw.png";

            //TEST IMAGES!!
            filenames[7] = "C:\\Users\\Hendrik\\Dropbox\\VB(suck)net\\Test\\Test images\\Reconition\\uk30_5_bw.png";
            filenames[8] = "C:\\Users\\Hendrik\\Dropbox\\VB(suck)net\\Test\\Test images\\Reconition\\20_3_bw_f.png";
            //filenames[8] = "C:\\Users\\Hendrik\\Dropbox\\VB(suck)net\\Test\\Test images\\Reconition\\20_3_bw.png";
            //filenames[8] = "C:\\Users\\Hendrik\\Dropbox\\VB(suck)net\\Test\\Test images\\Reconition\\uk40_4_bw.png";

            int Tmax = 9;

            //int trainSampleCount = 128;
            double[] pointts = new double[128];
            Matrix<float> trainData = new Matrix<float>(Tmax, 128);
            for (int T = 0; T < Tmax; T++)
            {
                int ijk = 0;
                //The name of the window
                string win1 = "Test Window";
                // CvInvoke.cvNamedWindow(win1);
                Image<Gray, double> GW = new Image<Gray, double>(1, 1);
                int R = 10;
                int C = 10;
                double u = 2;
                double v = 2;
                for (int i = 0; i < 8; i++)
                {
                    GW = GW.ConcateHorizontal(GaborWavelet(R, C, i, v));
                }


                //img.SmoothGaussian(5, 5, 1.5, 1.5);
                //Image<Bgr, Byte> img2 = new Image<Bgr, byte>(400, 200, new Bgr(255, 0, 0));
                Image<Gray, double> GWOutput = GW.Resize(10, INTER.CV_INTER_LINEAR);

                //Image<Bgr, Byte> img = new Image<Bgr, byte>(400, 200, new Bgr(255, 0, 0));
                // Image<Gray, Byte> img = new Image<Gray, Byte>("C:\\Users\\Hendrik\\Dropbox\\VB(suck)net\\Test\\Test images\\Reconition\\20_3_bw_f.png");
                Image<Gray, Byte> img = new Image<Gray, Byte>(filenames[T]);
                img = img.Resize(300, 300, INTER.CV_INTER_LINEAR);
                //Create the font
                MCvFont ftest = new MCvFont(Emgu.CV.CvEnum.FONT.CV_FONT_HERSHEY_SIMPLEX, 1.0, 1.0);
                //MCvFont f = new MCvFont();
                //Draw "Hello, world." on the image using the specific font
                // img.Draw("Hello, world", ref ftest, new Point(10, 80), new Bgr(0, 255, 0));
                /* img.Draw(new LineSegment2D(new Point(300 / 4, 0), new Point(300 / 4, 300)), new Bgr(0, 255, 0), 1);
                 img.Draw(new LineSegment2D(new Point(300 / 4 * 2, 0), new Point(300 / 4 * 2, 300)), new Bgr(0, 255, 0), 1);
                 img.Draw(new LineSegment2D(new Point(300 / 4 * 3, 0), new Point(300 / 4 * 3, 300)), new Bgr(0, 255, 0), 1);
                 img.Draw(new LineSegment2D(new Point(0, 300 / 4), new Point(300,300 / 4)), new Bgr(0, 255, 0), 1);
                 img.Draw(new LineSegment2D(new Point(0,300 / 4 * 2), new Point(300,300 / 4 * 2)), new Bgr(0, 255, 0), 1);
                 img.Draw(new LineSegment2D(new Point(0, 300 / 4 * 3), new Point(300,300 / 4 * 3)), new Bgr(0, 255, 0), 1);*/

                // CvInvoke.cvShowImage("Test Window", img);
                CvInvoke.cvShowImage("Test Window12", GW);
                CvInvoke.cvShowImage("Test Window2", GWOutput);

                Image<Gray, byte> edge;
                edge = img.Canny(125, 80);


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
                            
                            testCrop3 = testCrop3.ThresholdBinary(new Gray(0.1), new Gray(1));
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
                            trainData[T, ijk] = (float)dotprot;
                            //points[ijk * 2 + 1] = new Point(points[ijk * 2 + 1]);
                            int intValue = 1;
                            bool result = intValue == 1;
                            //histogram.DrawPolyline(points, result, new Gray(1), 1);
                            histogram.Draw(new LineSegment2D(ppoint, new Point(ijk * 5, 200 - (int)dotprot / 2)), new Gray(1), 1);
                            ppoint = new Point(ijk * 5, 200 - (int)dotprot / 2);
                            ijk++;

                        }
                    }
                }


                img.Draw(new LineSegment2D(new Point(300 / 4, 0), new Point(300 / 4, 300)), new Gray(125), 1);
                img.Draw(new LineSegment2D(new Point(300 / 4 * 2, 0), new Point(300 / 4 * 2, 300)), new Gray(125), 1);
                img.Draw(new LineSegment2D(new Point(300 / 4 * 3, 0), new Point(300 / 4 * 3, 300)), new Gray(125), 1);
                img.Draw(new LineSegment2D(new Point(0, 300 / 4), new Point(300, 300 / 4)), new Gray(125), 1);
                img.Draw(new LineSegment2D(new Point(0, 300 / 4 * 2), new Point(300, 300 / 4 * 2)), new Gray(125), 1);
                img.Draw(new LineSegment2D(new Point(0, 300 / 4 * 3), new Point(300, 300 / 4 * 3)), new Gray(125), 1);
                CvInvoke.cvShowImage("Test Window3", img);
                //CvInvoke.cvShowImage("Test Window3", divImg);
                divImg2 = divImg2.Resize(0.5, INTER.CV_INTER_LINEAR);

                CvInvoke.cvShowImage("Test Window4", divImg2);
                CvInvoke.cvShowImage("histogram", histogram);

                edge.Draw(new LineSegment2D(new Point(300 / 4, 0), new Point(300 / 4, 300)), new Gray(125), 1);
                edge.Draw(new LineSegment2D(new Point(300 / 4 * 2, 0), new Point(300 / 4 * 2, 300)), new Gray(125), 1);
                edge.Draw(new LineSegment2D(new Point(300 / 4 * 3, 0), new Point(300 / 4 * 3, 300)), new Gray(125), 1);
                edge.Draw(new LineSegment2D(new Point(0, 300 / 4), new Point(300, 300 / 4)), new Gray(125), 1);
                edge.Draw(new LineSegment2D(new Point(0, 300 / 4 * 2), new Point(300, 300 / 4 * 2)), new Gray(125), 1);
                edge.Draw(new LineSegment2D(new Point(0, 300 / 4 * 3), new Point(300, 300 / 4 * 3)), new Gray(125), 1);
                CvInvoke.cvShowImage("edge", edge);

                /*
                Rectangle rect = new Rectangle(0, 0, 50, 50);
                Image<Gray, byte> testCrop = new Image<Gray, byte>(rect.Size);
                testCrop = img.Copy(rect).Convert<Gray, byte>();
                CvInvoke.cvShowImage("Test crop", testCrop);*/
            }

            Matrix<float> trainClasses = new Matrix<float>(Tmax - 2, 1);
            trainClasses[0, 0] = 30;
            trainClasses[1, 0] = 30;
            trainClasses[2, 0] = 30;
            trainClasses[3, 0] = 30;
            trainClasses[4, 0] = 40;
            trainClasses[5, 0] = 40;
            trainClasses[6, 0] = 40;

            Matrix<float> trainData2 = trainData.RemoveRows(7, 9);
            //MessageBox.Show(trainData2.Rows.ToString());


            SVM model = new SVM();
            bool trained = model.Train(trainData2, trainClasses, null, null, p);

            // MessageBox.Show(trained.ToString());
            MessageBox.Show(model.Predict(trainData.GetRow(7)).ToString());
            MessageBox.Show(model.Predict(trainData.GetRow(8)).ToString());
        }

        public void getSign()
        {

        }

        public void trainFeutureReconizer()
        {

        }
    }
}