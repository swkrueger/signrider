using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

using BGRImage = Emgu.CV.Image<Emgu.CV.Structure.Bgr, System.Byte>;
using GrayImage = Emgu.CV.Image<Emgu.CV.Structure.Gray, System.Byte>;

namespace Signrider.Models
{
    // //-> class managing a found segment
    // public class ColourSegment : IDisposable
    // {
    //     public Image<Bgr, byte> rgbCrop { get; set; }
    //     public Image<Gray, byte> binaryCrop { get; set; }
    //     public SignColour colour { get; set; }

    //     public ColourSegment(Image<Bgr, byte> rgbCrop, Image<Gray, byte> binaryCrop, SignColour colour)
    //     {
    //         this.rgbCrop = rgbCrop;
    //         this.binaryCrop = binaryCrop;
    //         this.colour = colour;
    //     }

    //     public void Dispose()
    //     {
    //         rgbCrop.Dispose();
    //         binaryCrop.Dispose();
    //     }
    // }

    public class Segment
    {
        public BGRImage bgrImage { get; private set; }
        public GrayImage binaryImage { get; private set; }
        public SignColour colour { get; private set; }
        public SignShape shape { get; private set; }
        public SignType type { get; private set; }

        public Segment(ColourSegment colourSegment)
        {
            this.bgrImage = colourSegment.rgbCrop;
            this.binaryImage = colourSegment.binaryCrop;
            this.colour = colourSegment.colour;

            this.type = SignType.Garbage;

            if (TrafficSignRecognizer.ShapeClassifier.isTrained)
                this.shape = TrafficSignRecognizer.ShapeClassifier.classify(this.binaryImage);
            else
                this.shape = SignShape.Garbage;

            if (TrafficSignRecognizer.FeatureRecognizer.isTrained) {
                this.type = TrafficSignRecognizer.FeatureRecognizer.recognizeSign(bgrImage, binaryImage, shape);
            }
        }
    }
}
