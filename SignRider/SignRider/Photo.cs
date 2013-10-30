using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace Signrider
{
    class Photo
    {
        #region Construction
        public Photo(string path)
        {
            // TODO: Exception
            this.path = path;
            this.title = System.IO.Path.GetFileName(path);
        }

        #endregion
        #region Properties
        /// <summary>
        /// The photo, which may consist of zero or more traffic signs
        /// </summary>
        public Image<Bgr, Byte> image {
            get
            {
                return new Image<Bgr, byte>(path);
            }
        }

        public string title { get; private set; }
        public string path { get; private set; }
        #endregion

        public Image<Bgr, Byte> generateThumbnail(int width, int height)
        {
            return image.Resize(width, height, INTER.CV_INTER_LINEAR, true);
        }
    }
}
