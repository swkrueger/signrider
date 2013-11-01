using Signrider.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Emgu.CV;
using Emgu.CV.Structure;
using MahApps.Metro.Controls;

namespace Signrider.Views
{
    /// <summary>
    /// Interaction logic for SegmentDetailsView.xaml
    /// </summary>
    public partial class SegmentDetailsView : UserControl
    {
        public SegmentDetailsViewModel viewModel;
        public SegmentDetailsView(SegmentDetailsViewModel viewModel)
        {
            InitializeComponent();
            this.viewModel = viewModel;
            this.DataContext = viewModel;
        }

        private void Image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var imageControl = ((System.Windows.Controls.Image)sender);

            DebugImage debugImage = (DebugImage)imageControl.DataContext;

            CvInvoke.cvShowImage(debugImage.Name, debugImage.Image.Ptr);
        }
    }
}
