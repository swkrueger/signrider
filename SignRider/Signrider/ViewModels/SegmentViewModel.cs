using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroMvvm;
using PropertyChanged;
using System.Windows.Media.Imaging;

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
            
            this.Thumbnail = EmguToWpfImageConverter.ToBitmapSource(
                segment.bgrImage.Resize(thumbnailWidth, thumbnailHeight, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR, true)
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
                    return "Untrained";

                string shapeStringCamel = Segment.shape.ToString();

                return Regex.Replace(shapeStringCamel, "(\\B[A-Z])", " $1"); 
            }
        }

        public string TypeString {
            get {
                if (TrafficSignRecognizer.FeatureRecognizer.isTrained == false)
                    return "Untrained";

                string typeStringCamel = Segment.type.ToString();

                return Regex.Replace(typeStringCamel, "(\\B[A-Z])", " $1"); 
            }
        }
        #endregion

        #region Private Functions
        #endregion
    }
}
