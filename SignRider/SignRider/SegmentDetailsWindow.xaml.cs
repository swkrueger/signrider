using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

using MahApps.Metro.Controls;
using Signrider.Views;
using Signrider.ViewModels;

namespace Signrider
{
    /// <summary>
    /// Interaction logic for SegmentDetailsWindow.xaml
    /// </summary>
    public partial class SegmentDetailsWindow : MetroWindow
    {
        public SegmentDetailsWindow(SegmentDetailsView view, SegmentDetailsViewModel model)
        {
            InitializeComponent();
            this.DataContext = model;
            this.Content = view;
            this.Title = "Segment Details: " + model.Name;
            this.ShowActivated = true;
        }
    }
}
