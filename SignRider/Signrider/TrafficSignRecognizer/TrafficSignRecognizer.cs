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
    public class TrafficSignRecognizer
    {
        public TrafficSignRecognizer()
        {
            Segmenter = new ColourSegmenter();
        }

        public ColourSegmenter Segmenter { get; private set; }
    }
}
