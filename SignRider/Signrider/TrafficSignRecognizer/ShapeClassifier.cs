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
        Garbage,
        Circle,
        Octagon,
        Rectangle,
        TriangleUp,
        TriangleDown
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
        private const int featureVectorResolution = 128;
        private bool featureInterpolate = true;

        // Preprocessor settings
        private bool preprocessorResize = true;
        private bool preprocessorAddBorder = true;
        private bool preprocessorOpenFilter = false;
        private bool preprocessorPyr = true;
        private bool preprocessorBlur = true;
        private bool preprocessorStripBorder = true;
        private bool preprocessorConvexHullFill = false;

        // Debugging output settings
        private bool debugDisplayDebugImages = false;
        private bool debugPauseOnImageShow = true;

        // Private class variables
        private struct ShapeSvm
        {
            public SVM model;
            public List<SignShape> shapes;
            public bool isTrained;
        }
        private SVMParams SvmParameters;
        private ShapeSvm[] svms;

        public bool isTrained { get; private set; }

        public ShapeClassifier()
        {
            isTrained = false;

            // SVM settings
            // TODO: Play with SVM parameters
            SvmParameters = new SVMParams();
            SvmParameters.SVMType = Emgu.CV.ML.MlEnum.SVM_TYPE.C_SVC;
            SvmParameters.KernelType = Emgu.CV.ML.MlEnum.SVM_KERNEL_TYPE.POLY;
            SvmParameters.Gamma = 3;
            SvmParameters.Degree = 3;
            SvmParameters.C = 1;
            SvmParameters.TermCrit = new MCvTermCriteria(100, 0.00001);

            // Obtain the number of colour classes
            int numSignColours = Enum.GetValues(typeof(SignColour)).Length;

            // Create an empty SVM model for each colour
            svms = new ShapeSvm[numSignColours];
            for (int i = 0; i < numSignColours; ++i)
            {
                svms[i].model = new SVM();
                svms[i].isTrained = false;
                svms[i].shapes = new List<SignShape>();
            }

            // Define which shapes are used for each sign colour
            svms[(int)SignColour.RED].shapes.AddRange(
                new SignShape[] {
                    SignShape.Garbage,
                    SignShape.Circle,
                    SignShape.Octagon,
                    SignShape.TriangleUp,
                    SignShape.TriangleDown
                }
            );

            svms[(int)SignColour.BLUE].shapes.AddRange(
                new SignShape[] {
                    SignShape.Garbage,
                    SignShape.Circle,
                    SignShape.Rectangle
                }
            );
        }

        private void addDebugImage(List<DebugImage> debugImages, IImage image, string title = "Test Window")
        {
            if (debugImages != null)
                debugImages.Add(new DebugImage(image, title));

            if (debugDisplayDebugImages)
            {
                CvInvoke.cvShowImage(title, image.Ptr);
                if (debugPauseOnImageShow)
                {
                    CvInvoke.cvWaitKey(0);
                    CvInvoke.cvDestroyWindow(title);
                }
            }
        }

        public SignShape classify(GrayImage binaryImage, SignColour colour, List<DebugImage> debugImages = null)
        {
            int[] featureVector = extractDtbFeatures(binaryImage, debugImages);
            if (isTrained)
            {
                Matrix<float> data = new Matrix<float>(Array.ConvertAll<int, float>(featureVector, Convert.ToSingle));
                return (SignShape)svms[(int)colour].model.Predict(data);
            }
            else
            {
                return SignShape.Garbage;
            }
        }

        public void train(List<ShapeExample> examples)
        {
            int numSignColours = Enum.GetValues(typeof(SignColour)).Length;
            bool newIsTrained = true;

            // Calculate training data features
            Matrix<float>[] trainData = new Matrix<float>[numSignColours];
            Matrix<float>[] trainClasses = new Matrix<float>[numSignColours];

            for (int colourIdx = 0; colourIdx < numSignColours; colourIdx++)
            {
                int numColourExamples = 0;
                for (int i = 0; i < examples.Count; i++)
                    if (svms[colourIdx].shapes.Contains(examples[i].shape))
                        ++numColourExamples;

                trainData[colourIdx] = new Matrix<float>(numColourExamples, featureVectorDimension);
                trainClasses[colourIdx] = new Matrix<float>(numColourExamples, 1);

                int matrixIdx = 0;
                for (int i = 0; i < examples.Count; i++)
                {
                    if (svms[colourIdx].shapes.Contains(examples[i].shape))
                    {
                        ShapeExample example = examples[i];
                        int[] featureVector = extractDtbFeatures(example.image);
                        for (int j = 0; j < featureVectorDimension; j++)
                            trainData[colourIdx][matrixIdx, j] = featureVector[j];

                        trainClasses[colourIdx][matrixIdx, 0] = (int)example.shape;
                        ++matrixIdx;
                    }
                }

                if (numColourExamples > 2)
                {
                    // Train SVM models
                    svms[colourIdx].isTrained = svms[colourIdx].model.Train(
                        trainData[colourIdx],
                        trainClasses[colourIdx],
                        null,
                        null,
                        SvmParameters
                        );
                }

                newIsTrained = newIsTrained && svms[colourIdx].isTrained;
            }

            isTrained = newIsTrained;
        }

        private GrayImage preprocessImage(GrayImage origImage, List<DebugImage> debugImages = null)
        {

            GrayImage image = origImage;

            addDebugImage(debugImages, image, "Original image");

            // Resize to intermediate size
            if (preprocessorResize)
                image = image.Resize(128, 128, INTER.CV_INTER_CUBIC);

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

                addDebugImage(debugImages, image, "After Pyr");
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

                addDebugImage(debugImages, image, "After Open");
            }

            // Blur
            if (preprocessorBlur)
            {
                image = image.SmoothGaussian(5);

                addDebugImage(debugImages, image, "After Blur");
            }

            // Threshold
            image._ThresholdBinary(new Gray(100), new Gray(255));

            addDebugImage(debugImages, image, "After thresholding");

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

                addDebugImage(debugImages, image, "After hull");
            }

            // Crop border
            if (preprocessorStripBorder)
            {
                image =  Utilities.stripBorder(image, new Gray(100));

                addDebugImage(debugImages, image, "After crop");
            }

            return image;
        }

        private int[] getEdgeIndices(Matrix<Byte> mat, int threshold, bool reverseDirection)
        {
            int[] indices = new int[mat.Rows];

            if (!reverseDirection)
            {
                int prev = -1;
                for (int i = 0; i < mat.Rows; i++)
                {
                    int firstNonZero = 0;
                    while (firstNonZero < mat.Cols && mat[i, firstNonZero] < threshold)
                        ++firstNonZero;

                    // Interpolate
                    if (featureInterpolate && prev != -1 && firstNonZero > mat.Cols*8/10)
                        firstNonZero = prev;

                    indices[i] = firstNonZero;
                    prev = firstNonZero;
                }
            }
            else
            {
                int prev = -1;
                for (int i = 0; i < mat.Rows; i++)
                {
                    int firstNonZero = 0;
                    while (firstNonZero < mat.Cols && mat[i, mat.Cols - firstNonZero - 1] < threshold)
                        ++firstNonZero;

                    // Interpolate
                    if (featureInterpolate && prev != -1 && firstNonZero > mat.Cols*8/10)
                        firstNonZero = prev;

                    indices[i] = firstNonZero;
                    prev = firstNonZero;
                }
            }
            return indices;
        }

        private Image<Gray, Byte> plotFeatureVector(int[] signature)
        {
            Debug.WriteLine(String.Join(" ", signature));
            Debug.Flush();

            int numDimensions = signature.Length;
            int resolution = featureVectorResolution;

            float plotXScale = 5;
            float plotYScale = 1;
            Image<Gray, Byte> plot =
                new Image<Gray, Byte>(
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
                    new Gray(255),
                    1
                    );
            }

            return plot;
        }

        // Calculate the feature vector
        private int[] extractDtbFeatures(GrayImage image, List<DebugImage> debugImages = null)
        {
            // TODO: Rather use http://ieeexplore.ieee.org/stamp/stamp.jsp?tp=&arnumber=4290143

            GrayImage processedImage = preprocessImage(image, debugImages);

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

            if (debugImages != null)
            {
                Image<Gray, Byte> plot = plotFeatureVector(signature);
                addDebugImage(debugImages, plot, "Feature Vector");
            }

            return signature;
        }

        public void exportModels(string exportDir)
        {
            foreach (SignColour colour in Enum.GetValues(typeof(SignColour)))
            {
                string modelPath = Path.Combine(exportDir, "SignShape-" + colour.ToString() + ".xml");
                if (svms[(int)colour].isTrained)
                    svms[(int)colour].model.Save(modelPath);
            }
        }

        public void importModels(string importDir)
        {
            isTrained = true;

            foreach (SignColour colour in Enum.GetValues(typeof(SignColour)))
            {
                string modelPath = Path.Combine(importDir, "SignShape-" + colour.ToString() + ".xml");

                if (File.Exists(modelPath))
                {
                    svms[(int)colour].model.Load(modelPath);
                    svms[(int)colour].isTrained = true;
                }
                else
                {
                    isTrained = false;
                }
            }
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
