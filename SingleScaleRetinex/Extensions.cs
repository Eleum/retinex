using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SingleScaleRetinex
{
    public static class Extensions
    {

        /// <summary>
        /// Applies Single Scale Retinex to the image
        /// <paramref name="image">Image to process</paramref>
        /// <paramref name="gaussKernelDim">Gaussian kernel dimension</paramref>
        /// <paramref name="sigma">Width of the Gaussian kerner</paramref>
        /// </summary>
        public static string ApplySSR(this Image<Bgr, byte> img, int sigma)
        {
            // double for more precision
            var image = img.Convert<Bgr, double>();

            var imgHelper = new Image<Bgr, double>(image.Size);
            var imgLogImage = new Image<Bgr, double>(image.Size);
            var imgLogConvolved = new Image<Bgr, double>(image.Size);

            CvInvoke.cvConvertScale(image, imgHelper, 1, 0);
            CvInvoke.Log(imgHelper, imgLogImage);

            var imgBlurred = image.Clone();
            ApplyFilter(ref imgBlurred, sigma);

            CvInvoke.cvConvertScale(imgBlurred, imgHelper, 1, 0);
            CvInvoke.Log(imgHelper, imgLogConvolved);

            CvInvoke.Subtract(imgLogImage, imgLogConvolved, imgHelper);

            CvInvoke.cvConvertScale(imgHelper, image, 128, 128);

            var ptrImg = img.Ptr;
            var ptrBlurred = imgBlurred.Ptr;
            var ptrHelper = imgHelper.Ptr;
            var ptrLogImage = imgLogImage.Ptr;
            var ptrLogConvolved = imgLogConvolved.Ptr;

            CvInvoke.cvReleaseImage(ref ptrImg);
            CvInvoke.cvReleaseImage(ref ptrBlurred);
            CvInvoke.cvReleaseImage(ref ptrHelper);
            CvInvoke.cvReleaseImage(ref ptrLogImage);
            CvInvoke.cvReleaseImage(ref ptrLogConvolved);

            var savePath = $"{DateTime.Now.ToString("HH_mm_")}SSR_output.jpg";
            image.Save(savePath);

            return savePath;
        }

        public static void ApplyFilter(ref Image<Bgr, double> img, int sigma)
        {
            if (sigma > 300)
                sigma = 300;

            var filterSize = (sigma * 6) + 1;

            if (filterSize < 3)
                return;

            if (filterSize < 10)
            {
                CvInvoke.GaussianBlur(img, img, new Size(), filterSize, filterSize);
            }
            else
            {
                if (img.Height < 2 || img.Width < 2)
                    return;

                var sub = new Image<Bgr, double>(img.Width / 2, img.Height / 2);
                var subPtr = sub.Ptr;
                CvInvoke.PyrDown(img, sub);

                ApplyFilter(ref sub, sigma / 2);

                CvInvoke.Resize(sub, img, img.Size);
                CvInvoke.cvReleaseImage(ref subPtr);
            }
        }
    }
}

