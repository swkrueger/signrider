using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.IO;
using System.Collections;
using System.Diagnostics;
using MicroMvvm;

using MahApps.Metro.Controls;

namespace Signrider
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        HomeMenuView homeMenu;

        public MainWindow()
        {
            InitializeComponent();

            homeMenu = new HomeMenuView();

            goHome();

            // Setup debugging output
            Debug.AutoFlush = true;
        }

        public void goHome()
        {
            MainView.Content = homeMenu;
        }

        // public UserControl _mainView;
        // public UserControl MainView {
        //     get { return _mainView; }
        //     private set
        //     {
        //         if (value != _mainView)
        //         {
        //             _mainView = value;
        //         }
        //     }
        // }

        private void photosListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int numSelected = photosListBox.SelectedItems.Count;
            if (numSelected == 1)
            {
                PhotoViewModel selected = (PhotoViewModel)photosListBox.SelectedItem;
                MainView.Content = new Views.PhotoView(selected);
                // Yay!
            }
        }
    }

}
