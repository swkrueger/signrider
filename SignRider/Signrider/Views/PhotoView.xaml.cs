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

namespace Signrider.Views
{
    /// <summary>
    /// Interaction logic for PhotoView.xaml
    /// </summary>
    public partial class PhotoView : UserControl
    {
        private PhotoViewModel photoViewModel;
        public PhotoView(PhotoViewModel viewModel)
        {
            InitializeComponent();
            this.photoViewModel = viewModel;
            this.DataContext = viewModel;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Loaded");
            photoViewModel.IsActive = true;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Unloaded");
            photoViewModel.IsActive = false;
        }
    }
}
