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

        public static Image<Gray, Byte> stripBorder(Image<Gray, Byte> image, Gray threshold)
        {
            bool found;

            int left;
            found = false;
            for (left = 0; left < image.Cols; left++)
            {
                for (int r = 0; r < image.Rows && !found; ++r)
                    if (image[r, left].Intensity >= threshold.Intensity)
                        found = true;
                if (found) break;
            }

            int right;
            found = false;
            for (right = image.Cols - 1; right >= 0; right--)
            {
                for (int r = 0; r < image.Rows && !found; ++r)
                    if (image[r, right].Intensity >= threshold.Intensity)
                        found = true;
                if (found) break;
            }

            int top;
            found = false;
            for (top = 0; top < image.Rows; top++)
            {
                for (int c = 0; c < image.Cols && !found; ++c)
                    if (image[top, c].Intensity >= threshold.Intensity)
                        found = true;
                if (found) break;
            }

            int bottom;
            found = false;
            for (bottom = image.Rows - 1; bottom >= 0; bottom--)
            {
                for (int c = 0; c < image.Cols && !found; ++c)
                    if (image[bottom, c].Intensity >= threshold.Intensity)
                        found = true;
                if (found) break;
            }

            if (right < left)
            {
                left = 0;
                right = 0;
            }

            if (bottom < top)
            {
                top = 0;
                bottom = 0;
            }

            return image.Copy(new System.Drawing.Rectangle(left, top, right-left + 1, bottom-top + 1));
        }
    }
}
