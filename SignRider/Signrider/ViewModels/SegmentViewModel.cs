using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroMvvm;
using PropertyChanged;
using System.Windows.Media.Imaging;

using Emgu.CV;
using Emgu.CV.Structure;

using BGRImage = Emgu.CV.Image<Emgu.CV.Structure.Bgr, System.Byte>;
using GrayImage = Emgu.CV.Image<Emgu.CV.Structure.Gray, System.Byte>;
using System.Text.RegularExpressions;

namespace Signrider.ViewModels
{
    [ImplementPropertyChanged]
    public class SegmentViewModel
    {
        #region Constants
        private int thumbnailWidth = 128;
        private int thumbnailHeight = 128;
        #endregion

        #region Construction
        public SegmentViewModel(Models.Segment segment)
        {
            this.Segment = segment;

            Image<Bgra, Byte> rgbaImage = Utilities.addAlphaToBgrImage(segment.bgrImage, segment.binaryImage);

            this.Thumbnail = EmguToWpfImageConverter.ToBitmapSource(
                rgbaImage.Resize(thumbnailWidth, thumbnailHeight, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR, true)
            );
            this.Thumbnail.Freeze();
        }
        #endregion

        #region Members
        #endregion

        #region Properties
        public Models.Segment Segment { get; private set; }
        public BitmapSource SignImage { get; private set; }
        public BitmapSource Thumbnail { get; private set; }
        public bool IsGarbage
        {
            get
            {
                return Segment.shape == SignShape.Garbage || Segment.type == SignType.Garbage;
            }
        }

        public string ColourString
        {
            get
            {
                return Segment.colour.ToString();
            }
        }

        public string ShapeString {
            get {
                if (TrafficSignRecognizer.ShapeClassifier.isTrained == false)
                    return "Not trained";

                string shapeStringCamel = Segment.shape.ToString();

                return Regex.Replace(shapeStringCamel, "(\\B[A-Z0-9])", " $1");
            }
        }

        public string TypeString {
            get {
                if (TrafficSignRecognizer.FeatureRecognizer.isTrained == false)
                    return "Not trained";

                string typeStringCamel = Segment.type.ToString();

                return Regex.Replace(typeStringCamel, @"(?<a>(?<!^)((?:[A-Z][a-z])|(?:(?<!^[A-Z]+)[A-Z0-9]+(?:(?=[A-Z][a-z])|$))|(?:[0-9]+)))", @" ${a}");
            }
        }
        #endregion

        #region Private Functions
        #endregion
    }
}
