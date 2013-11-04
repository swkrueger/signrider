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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;

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
            IsBusyLoadingSegments = false;
            selectedIndex = -1;
            ShowAllSegments = false;
        }
        #endregion

        #region Members
        private Photo _photo;
        private bool isActive;
        private Image<Bgr, Byte> image;
        private Image<Bgr, Byte> resizedImage;
        private Image<Bgr, Byte> canvasBackground;
        private int selectedIndex;
        private ObservableCollection<ViewModels.SegmentViewModel> segmentViews = new ObservableCollection<ViewModels.SegmentViewModel>();
        private bool showAllSegments;
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

        public ObservableCollection<ViewModels.SegmentViewModel> SegmentViews
        {
            get
            {
                return segmentViews;
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

        public int SelectedIndex {
            get
            {
                return selectedIndex;
            }
            set
            {
                if (selectedIndex != value)
                {
                    selectedIndex = value;
                    if (IsActive)
                        redrawCanvas();
                }
            }
        }

        public bool ShowAllSegments {
            get
            {
                return showAllSegments;
            }
            set
            {
                if (showAllSegments != value)
                {
                    showAllSegments = value;
                    if (IsActive)
                        redrawCanvas();
                }
            }
        }

        public bool IsBusyLoadingImage { get; private set; }
        public bool IsBusyLoadingCanvas { get; private set; }
        public bool IsBusyLoadingSegments { get; private set; }
        public string SegmentLoadingStatusString { get; set; }
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
                else
                {
                    loadCanvas();
                    loadSegments();
                }
            });

            bw.RunWorkerAsync();
        }

        private Bgr contourColor = new Bgr(51, 255, 153);
        private Bgr selectedContourColor = new Bgr(153, 51, 255);
        private int contourThickness = 2;
        private int selectedContourThickness = 4;

        private BGRImage drawContoursOnImage(BGRImage background)
        {
            BGRImage imageWithContour = background.Copy();
            for (int i = 0; i < SegmentViews.Count(); i++)
            {
                if (!ShowAllSegments && SegmentViews[i].IsGarbage)
                    continue;

                Point[] contour = SegmentViews[i].Segment.contour;
                Point[] scaledContour = new Point[contour.Length];
                for (int j = 0; j < contour.Length; ++j)
                {
                    scaledContour[j].X = (int)(contour[j].X / ((double)this.image.Width / background.Width));
                    scaledContour[j].Y = (int)(contour[j].Y / ((double)this.image.Height / background.Height));
                }

                if (SelectedIndex == i)
                    imageWithContour.DrawPolyline(scaledContour, true, selectedContourColor, selectedContourThickness);
                else
                    imageWithContour.DrawPolyline(scaledContour, true, contourColor, contourThickness);
            }
            return imageWithContour;
        }

        private void redrawCanvas()
        {
            BGRImage canvasImage = drawContoursOnImage(canvasBackground);
            Canvas = EmguToWpfImageConverter.ToBitmapSource(canvasImage);
        }

        private void loadCanvas()
        {
            IsBusyLoadingCanvas = true;
            canvasBackground = resizedImage;
            redrawCanvas();
            IsBusyLoadingCanvas = false;
        }

        private void loadSegments()
        {
            SegmentLoadingStatusString = "";
            IsBusyLoadingSegments = true;

            BackgroundWorker bw = new BackgroundWorker();

            // this allows our worker to report progress during work
            bw.WorkerReportsProgress = true;

            // what to do in the background thread
            bw.DoWork += new DoWorkEventHandler(
            delegate(object o, DoWorkEventArgs args)
            {
                BackgroundWorker b = o as BackgroundWorker;

                List<ViewModels.SegmentViewModel> newSegmentViews = new List<ViewModels.SegmentViewModel>();

                b.ReportProgress(0, "Segmentizing...");
                List<ColourSegment> colourSegments = TrafficSignRecognizer.Segmenter.determineColourSegments(image);

                b.ReportProgress(50, "Recognizing...");
                foreach (ColourSegment colourSegment in colourSegments)
                {
                    ViewModels.SegmentViewModel newSegmentViewModel =
                        new ViewModels.SegmentViewModel(
                            new Models.Segment(colourSegment)
                        );

                    newSegmentViews.Add(newSegmentViewModel);
                }

                args.Result = newSegmentViews;
            });

            // what to do when progress changed (update the progress bar for example)
            bw.ProgressChanged += new ProgressChangedEventHandler(
            delegate(object o, ProgressChangedEventArgs args)
            {
                SegmentLoadingStatusString = (string)args.UserState;
            });

            // what to do when worker completes its task (notify the user)
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(
            delegate(object o, RunWorkerCompletedEventArgs args)
            {
                if (isActive)
                {
                    List<ViewModels.SegmentViewModel> newSegmentViews =
                        (List<ViewModels.SegmentViewModel>)args.Result;
                    IsBusyLoadingSegments = false;

                    if (!TrafficSignRecognizer.isTrained())
                        ShowAllSegments = true;
                    else
                    {
                        ShowAllSegments = true;
                        foreach (ViewModels.SegmentViewModel view in newSegmentViews)
                            if (view.Segment.shape != SignShape.Garbage)
                            {
                                ShowAllSegments = false;
                                break;
                            }
                    }

                    foreach (ViewModels.SegmentViewModel view in newSegmentViews)
                        segmentViews.Add(view);

                    redrawCanvas();
                }
                else
                {
                    unload();
                }
            });

            bw.RunWorkerAsync();
        }

        private void unload()
        {
            if (IsBusyLoadingImage || IsBusyLoadingCanvas || IsBusyLoadingSegments)
                return;
            // TODO: Call unload after backgroundWorker finishes

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

            segmentViews.Clear();
        }
    }
}
