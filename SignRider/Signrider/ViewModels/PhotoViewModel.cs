using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MicroMvvm;
using PropertyChanged;

using Emgu.CV;
using Emgu.CV.Structure;
using System.ComponentModel;
using System.Windows.Media.Imaging;

using BGRImage = Emgu.CV.Image<Emgu.CV.Structure.Bgr, System.Byte>;
using GrayImage = Emgu.CV.Image<Emgu.CV.Structure.Gray, System.Byte>;

namespace Signrider
{
    [ImplementPropertyChanged]
    public class PhotoViewModel : NotificationObject
    {
        #region Constants
        private int canvasWidth = 1920;
        private int canvasHeight = 1080;
        private int thumbnailWidth = 128;
        private int thumbnailHeight = 128;
        #endregion

        #region Construction
        public PhotoViewModel(Photo photo)
        {
            this.photo = photo;
            IsBusyLoadingImage = false;
            IsBusyLoadingCanvas = false;
        }
        #endregion

        #region Members
        Photo _photo;
        bool isActive;
        private Image<Bgr, Byte> image;
        private Image<Bgr, Byte> resizedImage;
        #endregion

        #region Properties
        public Image<Bgr, Byte> thumbnail { get; private set; }
        public BitmapSource Canvas { get; private set; }

        public Photo photo {
            get
            {
                return _photo;
            }
            private set
            {
                _photo = value;

                // Create thumbnail
                thumbnail = _photo.generateThumbnail(thumbnailWidth, thumbnailHeight);
            }
        }

        public bool IsActive
        {
            get
            {
                return isActive;
            }
            set
            {
                if (isActive != value)
                {
                    isActive = value;

                    if (isActive) load();
                    else unload();
                }
            }
        }

        public bool IsBusyLoadingImage { get; private set; }
        public bool IsBusyLoadingCanvas { get; private set; }
        #endregion

        private void load()
        {
            loadImage();
        }

        private void loadImage()
        {
            IsBusyLoadingImage = true;
            IsBusyLoadingCanvas = true;

            BackgroundWorker bw = new BackgroundWorker();

            // what to do in the background thread
            bw.DoWork += new DoWorkEventHandler(
            delegate(object o, DoWorkEventArgs args)
            {
                BackgroundWorker b = o as BackgroundWorker;
                image = photo.getImage();
                resizedImage = image.Resize(canvasWidth, canvasHeight, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR, true);
            });

            // what to do when worker completes its task (notify the user)
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(
            delegate(object o, RunWorkerCompletedEventArgs args)
            {
                IsBusyLoadingImage = false;

                if (!isActive) unload();
                else loadCanvas();
            });

            bw.RunWorkerAsync();
        }

        private void loadCanvas()
        {
            IsBusyLoadingCanvas = true;
            Canvas = EmguToWpfImageConverter.ToBitmapSource(resizedImage);
            IsBusyLoadingCanvas = false;
        }

        private void unload()
        {
            if (image != null)
            {
                image.Dispose();
                image = null;
            }

            if (resizedImage != null)
            {
                resizedImage.Dispose();
                resizedImage = null;
            }

            Canvas = null;
        }
    }
}
