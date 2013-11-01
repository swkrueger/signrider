using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

using Point = System.Drawing.Point;

namespace Signrider
{
    public static class TrafficSignRecognizer
    {
        static public ColourSegmenter Segmenter = new ColourSegmenter();
        static public FeatureRecognizer FeatureRecognizer = new FeatureRecognizer();
        static public ShapeClassifier ShapeClassifier = new ShapeClassifier();

        static public bool isTrained()
        {
            return FeatureRecognizer.isTrained && ShapeClassifier.isTrained;
        }

        static public bool train(string shapeTrainDir, string featureTrainDir)
        {
            // Retrieve shape training images
            List<ShapeExample> shapeTrainExamples = ShapeClassifier.extractExamplesFromDirectory(shapeTrainDir);

            if (shapeTrainExamples.Count() < 2)
                return false;

            // Perform shape training
            TrafficSignRecognizer.ShapeClassifier.train(shapeTrainExamples);

            // Retrieve shape training images
            List<FeatureExample> featureTrainExamples = FeatureRecognizer.extractExamplesFromDirectory(featureTrainDir);

            if (shapeTrainExamples.Count() < 2)
                return false;

            TrafficSignRecognizer.FeatureRecognizer.train(featureTrainExamples);

            return true;
        }
    }
}
