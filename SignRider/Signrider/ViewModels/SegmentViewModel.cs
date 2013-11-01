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
        #endregion

        #region Private Functions
        #endregion
    }
}
