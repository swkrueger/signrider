using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MicroMvvm;
using PropertyChanged;

using Emgu.CV;
using Emgu.CV.Structure;

using BGRImage = Emgu.CV.Image<Emgu.CV.Structure.Bgr, System.Byte>;
using GrayImage = Emgu.CV.Image<Emgu.CV.Structure.Gray, System.Byte>;
using System.Collections.ObjectModel;

namespace Signrider.ViewModels
{
    [ImplementPropertyChanged]
    public class SegmentDetailsViewModel : SegmentViewModel
    {
        #region Constants
        #endregion

        #region Construction
        public SegmentDetailsViewModel(Models.Segment segment) : base(segment)
        {
            this.ColourSegmentationImages = new ObservableCollection<DebugImage>();
            this.ShapeClassifierImages = new ObservableCollection<DebugImage>();
            this.FeatureRecognitionImages = new ObservableCollection<DebugImage>();

            ColourSegmentationImages.Add(new DebugImage(segment.bgrImage, "RGB image"));
            ColourSegmentationImages.Add(new DebugImage(segment.binaryImage, "Binary image"));

            // if (TrafficSignRecognizer.ShapeClassifier.isTrained)
            //     this.shape = TrafficSignRecognizer.ShapeClassifier.classify(this.binaryImage);
            // else
            //     this.shape = SignShape.Garbage;

            if (segment.shape != SignShape.Garbage && TrafficSignRecognizer.FeatureRecognizer.isTrained) {
                List<DebugImage> images = new List<DebugImage>();
                TrafficSignRecognizer.FeatureRecognizer.recognizeSign(segment.bgrImage, segment.binaryImage, segment.shape, images);
                foreach (DebugImage image in images)
                    FeatureRecognitionImages.Add(image);
            }
        }
        #endregion

        #region Members
        public ObservableCollection<DebugImage> ColourSegmentationImages { get; private set; }
        public ObservableCollection<DebugImage> ShapeClassifierImages { get; private set; }
        public ObservableCollection<DebugImage> FeatureRecognitionImages { get; private set; }
        #endregion

        #region Properties
        public string Name { get; set; }
        #endregion

        #region Private Functions
        #endregion
    }
}
