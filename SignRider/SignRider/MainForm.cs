using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;

namespace SignRider
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            doStuff();
        }

    private void doStuff()
    {
        FeatureRecognizer test = new FeatureRecognizer();
        test.doTest();
            
            //Create an image of 400x200 of Blue color
            using (Image<Bgr, Byte> img = new Image<Bgr, byte>(400, 200, new Bgr(255, 0, 0)))
            {
                //Create the font
                MCvFont f = new MCvFont(FONT.CV_FONT_HERSHEY_COMPLEX, 1.0, 1.0);

                //Draw "Hello, world." on the image using the specific font
                img.Draw("Hello, world", ref f, new Point(10, 80), new Bgr(0, 255, 0));

                //Show the image using ImageViewer from Emgu.CV.UI
                ImageViewer.Show(img, "Test Window");
            }
            //ImageViewer.Show(test.getImg(), "Test Window");
        }
    }
}
