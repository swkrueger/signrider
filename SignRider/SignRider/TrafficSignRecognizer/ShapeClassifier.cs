using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Diagnostics;

using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.ML;
using Emgu.CV.ML.Structure;

using GrayImage = Emgu.CV.Image<Emgu.CV.Structure.Gray, System.Byte>;
using System.IO;

namespace Signrider
{
    public enum SignShape
    {
        Circle,
        Octagon,
        Rectangle,
        TriangleUp,
        TriangleDown,
        Garbage
    };

    public struct ShapeExample
    {
        public string name;
        public GrayImage image;
        public SignColour colour;
        public SignShape shape;
    }

    public class ShapeClassifier
    {
        // Feature vector settings
        private const int featureVectorDimension = 128;
        private const int featureVectorResolution = 32;

        // Preprocessor settings
        private bool preprocessorResize = true;
        private bool preprocessorAddBorder = true;
        private bool preprocessorOpenFilter = false;
        private bool preprocessorPyr = true;
        private bool preprocessorBlur = true;
        private bool preprocessorStripBorder = true;
        private bool preprocessorConvexHullFill = false;

        // Debugging output settings
        private bool debugPlotFeatureVector = false;
        private bool debugPauseOnImageShow = true;
        private bool debugPreprocessor = false;

        // Private class variables
        private SVM SvmModel = new SVM();
        private SVMParams SvmParameters;

        public bool isTrained { get; private set; }

        public ShapeClassifier()
        {
            isTrained = false;

            // SVM settings
            // TODO: Play with SVM parameters
            SvmParameters = new SVMParams();
            SvmParameters.KernelType = Emgu.CV.ML.MlEnum.SVM_KERNEL_TYPE.LINEAR;
            SvmParameters.SVMType = Emgu.CV.ML.MlEnum.SVM_TYPE.C_SVC;
            SvmParameters.C = 1;
            SvmParameters.TermCrit = new MCvTermCriteria(100, 0.00001);
        }

        private void showImage(IntPtr image, string title = "Test Window")
        {
            CvInvoke.cvShowImage(title, image);
            if (debugPauseOnImageShow)
            {
                CvInvoke.cvWaitKey(0);
                CvInvoke.cvDestroyWindow(title);
            }
        }

        public SignShape classify(GrayImage binaryImage)
        {
            int[] featureVector = extractDtbFeatures(binaryImage);
            Matrix<float> data = new Matrix<float>(Array.ConvertAll<int,float>(featureVector, Convert.ToSingle));
            return (SignShape)SvmModel.Predict(data);
        }

        public void train(List<ShapeExample> examples)
        {
            Matrix<float> trainData = new Matrix<float>(examples.Count, featureVectorDimension);
            Matrix<float> trainClasses = new Matrix<float>(examples.Count, 1);

            // Calculate training data features
            for (int i = 0; i < examples.Count; i++)
            {
                ShapeExample example = examples[i];

                int[] featureVector = extractDtbFeatures(example.image);
                for (int j = 0; j < featureVectorDimension; j++)
                    trainData[i, j] = featureVector[j];

                trainClasses[i,0] = (int) example.shape;
            }

            // Train SVM model
            isTrained = SvmModel.TrainAuto(
                trainData,
                trainClasses,
                null,
                null,
                SvmParameters.MCvSVMParams,
                5
                );
        }

        private GrayImage preprocessImage(GrayImage origImage)
        {

            GrayImage image = origImage;

            if (debugPreprocessor)
                showImage(image, "Original image");

            // Resize to intermediate size
            if (preprocessorResize)
                image = image.Resize(108, 108, INTER.CV_INTER_CUBIC);

            // Create border
            if (preprocessorAddBorder)
            {
                const int borderWidth = 10;
                GrayImage tempImage = new GrayImage(
                    image.Width + 2 * borderWidth,
                    image.Height + 2 * borderWidth);

                CvInvoke.cvCopyMakeBorder(
                    image,
                    tempImage,
                    new System.Drawing.Point(borderWidth, borderWidth),
                    BORDER_TYPE.CONSTANT, new MCvScalar(0)
                    );
                image = tempImage;
            }

            // Filter out noise
            if (preprocessorPyr)
            {
                image = image.PyrDown().PyrUp();

                if (debugPreprocessor)
                    showImage(image, "After Pyr");
            }

            /// Eliminate small gaps
            if (preprocessorOpenFilter)
            {
                // Option 1
                image._Erode(5);
                image._Dilate(5);

                // // Option 2: Morph open
                // StructuringElementEx element = new StructuringElementEx(5, 5, 0, 0, CV_ELEMENT_SHAPE.CV_SHAPE_ELLIPSE);
                // image._MorphologyEx(element, CV_MORPH_OP.CV_MOP_OPEN, 2);

                if (debugPreprocessor)
                    showImage(image, "After Open");
            }

            // Blur
            if (preprocessorBlur)
            {
                image = image.SmoothGaussian(5);

                if (debugPreprocessor)
                    showImage(image, "After Blur");
            }

            // Threshold
            image._ThresholdBinary(new Gray(100), new Gray(255));

            if (debugPreprocessor)
                showImage(image, "After thresholding");

            if (preprocessorConvexHullFill)
            {
                // TODO: Check FindContour parameters
                // var contour = image.FindContours(CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE, RETR_TYPE.CV_RETR_CCOMP);
                GrayImage filledImage = image;
                for (var contour = image.FindContours(CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE, RETR_TYPE.CV_RETR_CCOMP); contour != null; contour = contour.HNext)
                {
                    Seq<Point> pointsSeq = contour.GetConvexHull(ORIENTATION.CV_CLOCKWISE);
                    Point[] points = pointsSeq.ToArray();
                    filledImage.FillConvexPoly(points, new Gray(255));
                }
                image = filledImage;

                if (debugPreprocessor)
                    showImage(image, "After hull");
            }

            // Crop border
            if (preprocessorStripBorder)
            {
                // TODO: Use faster and more robust method
                var contour = image.FindContours();
                if (contour != null)
                {
                    image = image.Copy(contour.BoundingRectangle);
                }

                if (debugPreprocessor)
                    showImage(image, "After crop");
            }

            return image;
        }

        private int[] getEdgeIndices(Matrix<Byte> mat, int threshold, bool reverseDirection)
        {
            int[] indices = new int[mat.Rows];

            if (!reverseDirection)
            {
                for (int i = 0; i < mat.Rows; i++)
                {
                    int firstNonZero = 0;
                    while (firstNonZero < mat.Cols && mat[i, firstNonZero] < threshold)
                        ++firstNonZero;
                    indices[i] = firstNonZero;
                }
            }
            else
            {
                for (int i = 0; i < mat.Rows; i++)
                {
                    int firstNonZero = 0;
                    while (firstNonZero < mat.Cols && mat[i, mat.Cols - firstNonZero - 1] < threshold)
                        ++firstNonZero;
                    indices[i] = firstNonZero;
                }
            }
            return indices;
        }

        private void plotFeatureVector(int[] signature)
        {
            Debug.WriteLine(String.Join(" ", signature));
            Debug.Flush();

            int numDimensions = signature.Length;
            int resolution = featureVectorResolution;

            float plotXScale = 5;
            float plotYScale = 1;
            Image<Gray, float> plot =
                new Image<Gray, float>(
                    (int)(numDimensions * plotXScale),
                    (int)(resolution * plotYScale)
                );

            for (int i = 0; i < numDimensions - 1; ++i)
            {
                plot.Draw(
                    new LineSegment2DF(
                        new PointF(i * plotXScale, (resolution - signature[i]) * plotYScale),
                        new PointF((i+1) * plotXScale, (resolution - signature[i+1]) * plotYScale)
                    ),
                    new Gray(1),
                    1
                    );
            }

            showImage(plot, "Feature Vector");
        }

        // Calculate the feature vector
        private int[] extractDtbFeatures(GrayImage image)
        {
            // TODO: Rather use http://ieeexplore.ieee.org/stamp/stamp.jsp?tp=&arnumber=4290143

            GrayImage processedImage = preprocessImage(image);

            int numDimensions = featureVectorDimension;
            int resolution = featureVectorResolution;
            int sideLength = numDimensions/4;

            Matrix<Byte> columns = new Matrix<Byte>(resolution, sideLength);
            Matrix<Byte> rows = new Matrix<Byte>(sideLength, resolution);
            processedImage.Resize(sideLength, resolution, INTER.CV_INTER_CUBIC).CopyTo(columns);
            processedImage.Resize(resolution, sideLength, INTER.CV_INTER_CUBIC).CopyTo(rows);
            Matrix<Byte> columnsT = columns.Transpose();

            int[] signature = new int[numDimensions];

            getEdgeIndices(columnsT, 127, false).CopyTo(signature, 0);                // Top
            getEdgeIndices(rows, 127, true).CopyTo(signature, sideLength);            // Right
            getEdgeIndices(columnsT, 127, true).CopyTo(signature, sideLength * 2);    // Bottom
            getEdgeIndices(rows, 127, false).CopyTo(signature, sideLength * 3);       // Left

            if (debugPlotFeatureVector)
                plotFeatureVector(signature);

            return signature;
        }

        public static List<ShapeExample> extractExamplesFromDirectory(string directory)
        {
            List<ShapeExample> examples = new List<ShapeExample>();

            if (System.IO.Directory.Exists(directory) == false)
                return examples;

            foreach (SignShape shape in Enum.GetValues(typeof(SignShape)))
            {
                string shapeDir = Path.Combine(directory, shape.ToString());

                if (System.IO.Directory.Exists(shapeDir))
                {
                    string[] files = Utilities.GetFiles(shapeDir, ".jpg|*.png", SearchOption.AllDirectories);

                    foreach (string file in files)
                    {
                        string[] nameTokens = Path.GetFileNameWithoutExtension(file).Split('_');
                        if (nameTokens.Count() < 3)
                            continue;

                        string signColourString = nameTokens[nameTokens.Count() - 2];
                        SignColour colour;

                        if (Enum.IsDefined(typeof(SignColour), signColourString))
                            colour = (SignColour)Enum.Parse(typeof(SignColour), signColourString);
                        else
                            continue;

                        ShapeExample example = new ShapeExample();

                        // TODO: Catch exception
                        example.image = new Image<Gray, Byte>(file);
                        example.shape = shape;
                        example.colour = colour;
                        example.name = file;

                        Debug.WriteLine("Loaded training example " + file);
                        Debug.Flush();

                        examples.Add(example);
                    }
                }
            }

            return examples;
        }

    }
}
