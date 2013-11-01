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
            // TODO: Set button visibility
            setMainView(homeMenu);
        }

        public void setMainView(UserControl newView)
        {
            MainView.Content = newView;
        }

        private void photosListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int numSelected = photosListBox.SelectedItems.Count;
            if (numSelected == 1)
            {
                PhotoViewModel selected = (PhotoViewModel)photosListBox.SelectedItem;
                setMainView(new Views.PhotoView(selected));
            }
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            goHome();
        }
    }

}
