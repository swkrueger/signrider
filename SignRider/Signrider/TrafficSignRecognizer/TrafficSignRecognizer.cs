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
    }
}
