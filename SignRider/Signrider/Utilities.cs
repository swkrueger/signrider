using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Emgu.CV;
using Emgu.CV.Structure;

namespace Signrider
{
    public static class Utilities
    {
        public static string[] GetFiles(string sourceFolder, string filters, System.IO.SearchOption searchOption)
        {
            return filters.Split('|').SelectMany(filter => System.IO.Directory.GetFiles(sourceFolder, filter, searchOption)).ToArray();
        }

        public static Image<Bgra, Byte> addAlphaToBgrImage(Image<Bgr, Byte> bgrImage, Image<Gray, Byte> alphaImage)
        {
            Image<Gray, Byte>[] channels = bgrImage.Split();
            Image<Bgra, Byte> imageWithAlpha = new Image<Bgra,byte>(bgrImage.Width, bgrImage.Height);
            CvInvoke.cvMerge(channels[0], channels[1], channels[2], alphaImage, imageWithAlpha);

            for (int i = 0; i < 3; ++i) channels[i].Dispose();
            
            return imageWithAlpha;
        }
    }
}
