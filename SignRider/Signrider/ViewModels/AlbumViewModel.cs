using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Forms;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using MicroMvvm;
using PropertyChanged;
using System.IO;

namespace Signrider
{
    [ImplementPropertyChanged]
    public class AlbumViewModel
    {
        #region Construction
        public AlbumViewModel()
        {
            isBusyLoading = false;
            isBusyTraining = false;
            isTrained = false;
        }
        #endregion

        #region Members
        ObservableCollection<PhotoViewModel> _photos = new ObservableCollection<PhotoViewModel>();
        #endregion

        #region Properties
        public ObservableCollection<PhotoViewModel> photos
        {
            get
            {
                return _photos;
            }
        }

        public int loadingProgress { get; private set; }
        public bool isBusyLoading { get; private set; }
        public bool isBusyTraining { get; private set; }
        public bool isTrained { get; private set; }
        #endregion

        #region Private Functions
        void loadPhotos(string[] filenames)
        {
            loadingProgress = 0;
            isBusyLoading = true;

            BackgroundWorker bw = new BackgroundWorker();

            // this allows our worker to report progress during work
            bw.WorkerReportsProgress = true;

            // what to do in the background thread
            bw.DoWork += new DoWorkEventHandler(
            delegate(object o, DoWorkEventArgs args)
            {
                BackgroundWorker b = o as BackgroundWorker;

                int numFiles = filenames.Count();
                int counter = 0;

                foreach (String filename in filenames)
                {
                    PhotoViewModel newPhoto =
                        new PhotoViewModel(
                            new Photo(filename)
                        );
                    Action<PhotoViewModel> addMethod = photos.Add;
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(addMethod, newPhoto);
                    counter++;
                    b.ReportProgress((int)(counter * 100 / numFiles));
                }
            });

            // what to do when progress changed (update the progress bar for example)
            bw.ProgressChanged += new ProgressChangedEventHandler(
            delegate(object o, ProgressChangedEventArgs args)
            {
                this.loadingProgress = args.ProgressPercentage;
            });

            // what to do when worker completes its task (notify the user)
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(
            delegate(object o, RunWorkerCompletedEventArgs args)
            {
                isBusyLoading = false;
            });

            bw.RunWorkerAsync();
        }
        #endregion

        #region Commands
        public void trainFromDirectory()
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

            if (folderBrowserDialog.ShowDialog() != DialogResult.OK) return;

            string trainDir = folderBrowserDialog.SelectedPath;
            string shapeTrainDir = System.IO.Path.Combine(trainDir, "Shapes");
            string featureTrainDir = System.IO.Path.Combine(trainDir, "Features");

            if (!Directory.Exists(shapeTrainDir))
            {
                System.Windows.MessageBox.Show("Shape training directory not found! Please put shape training images in " + shapeTrainDir);
                return;
            }

            if (!Directory.Exists(featureTrainDir))
            {
                System.Windows.MessageBox.Show("Feature training directory not found! Please put shape training images in " + featureTrainDir);
                return;
            }

            isBusyTraining = true;

            bool success = TrafficSignRecognizer.train(shapeTrainDir, featureTrainDir);
            if (!success)
            {
                // TODO: Descriptive error message
                System.Windows.MessageBox.Show("Training failed");
            }

            isBusyTraining = false;

            isTrained = TrafficSignRecognizer.isTrained();
        }

        public bool canTrainFromDirectory() { return !isBusyLoading; }

        public ICommand trainFromDirectoryCommand
        {
            get { return new RelayCommand(trainFromDirectory, canTrainFromDirectory); }
        }

        void loadImageFromFile()
        {
            // Create an instance of the open file dialog box.
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // Set filter options and filter index.
            openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.png, *.bmp, *.ppm) | *.jpg; *.jpeg; *.png; *.bmp; *.ppm";
            openFileDialog.RestoreDirectory = true;
            openFileDialog.Multiselect = true;

            // Call the ShowDialog method to show the dialog box and 
            // process input if the user clicked OK.
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                loadPhotos(openFileDialog.FileNames);
            }
        }

        bool canLoadImageFromFile() { return !isBusyLoading; }
        public ICommand loadImageFromFileCommand
        {
            get { return new RelayCommand(loadImageFromFile, canLoadImageFromFile); }
        }

        #endregion
    }
}
