using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroMvvm;
using Emgu.CV;
using Emgu.CV.Structure;

namespace Signrider
{
    class PhotoViewModel : NotificationObject
    {
        #region Construction
        public PhotoViewModel(Photo photo)
        {
            this.photo = photo;
        }
        #endregion

        #region Members
        Photo _photo;
        #endregion

        #region Properties
        public Photo photo {
            get
            {
                return _photo;
            }
            private set
            {
                _photo = value;

                // Create thumbnail
                thumbnail = _photo.generateThumbnail(128, 128);
            }
        }
        public Image<Bgr, Byte> thumbnail { get; private set; }
        #endregion
    }
}
