using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroMvvm;
using PropertyChanged;
using System.Windows.Media.Imaging;

namespace Signrider.ViewModels
{
    [ImplementPropertyChanged]
    public class SegmentViewModel
    {
        #region Construction
        public SegmentViewModel(Models.Segment segment)
        {
            this.Segment = segment;
        }
        #endregion

        #region Members
        #endregion

        #region Properties
        public Models.Segment Segment { get; private set; }
        public BitmapSource SignImage { get; private set; }
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
