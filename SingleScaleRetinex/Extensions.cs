using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
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
        /// </summary>
        /// <param name="img">Image to process</param>
        /// <param name="sigma">Width of the Gaussian kerner</param>
        /// <returns></returns>
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
            QuickFilter(ref imgBlurred, sigma);

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

            var savePath = $"{DateTime.Now.ToString("HH_mm_")}output_SSR.jpg";
            image.Save(savePath);

            return savePath;
        }

        public static string ApplyMSR(this Image<Bgr, byte> img, IEnumerable<double> weights, IEnumerable<int> sigmas)
        {
            var image = img.Convert<Bgr, double>();

            var imgHelper = new Image<Bgr, double>(image.Size);
            var imgLogImage = new Image<Bgr, double>(image.Size);
            var imgLogConvolved = new Image<Bgr, double>(image.Size);

            var helperPtr = imgHelper.Ptr;
            var logPtr = imgLogImage.Ptr;
            var cLogPtr = imgLogConvolved.Ptr;

            CvInvoke.cvConvertScale(image, imgHelper, 1, 0);
            CvInvoke.Log(imgHelper, imgLogImage);

            // normalization
            var sum = weights.Sum();
            if (weights.Sum() != 1.0)
                CvInvoke.cvConvertScale(image, image, sum, 0);

            for (int i = 0; i < weights.Count(); i++)
            {
                var helper = image.Clone();
                var ptr = helper.Ptr;

                QuickFilter(ref helper, sigmas.ElementAt(i));

                CvInvoke.cvConvertScale(helper, imgHelper, 1, 0);
                CvInvoke.Log(imgHelper, imgLogConvolved);
                CvInvoke.cvReleaseImage(ref ptr);

                CvInvoke.cvConvertScale(imgLogConvolved, imgLogConvolved, weights.ElementAt(i), 0);
                CvInvoke.Subtract(imgLogImage, imgLogConvolved, imgLogImage);
            }

            CvInvoke.cvConvertScale(imgLogImage, image, 128, 128);

            CvInvoke.cvReleaseImage(ref helperPtr);
            CvInvoke.cvReleaseImage(ref logPtr);
            CvInvoke.cvReleaseImage(ref cLogPtr);

            var savePath = $"{DateTime.Now.ToString("HH_mm_")}output_MSR.jpg";
            image.Save(savePath);

            return savePath;
        }

        public static string ApplyMSRCR(this Image<Bgr, byte> img, IEnumerable<double> weights, IEnumerable<int> sigmas, int gain, int offset, double restorationFactor, double colorGain)
        {
            var image = img.Convert<Bgr, double>();

            var imgHelper = new Image<Bgr, double>(image.Size);
            var imgLogImage = new Image<Bgr, double>(image.Size);
            var imgLogConvolved = new Image<Bgr, double>(image.Size);

            var imgChannelB = new Image<Gray, double>(image.Size);
            var imgChannelG = new Image<Gray, double>(image.Size);
            var imgChannelR = new Image<Gray, double>(image.Size);
            var imgChannelHelper = new Image<Gray, double>(image.Size);

            var helperPtr = imgHelper.Ptr;
            var logPtr = imgLogImage.Ptr;
            var cLogPtr = imgLogConvolved.Ptr;
            var channelBPtr = imgChannelB.Ptr;
            var channelGPtr = imgChannelG.Ptr;
            var channelRPtr = imgChannelR.Ptr;

            CvInvoke.cvConvertScale(image, imgHelper, 1, 0);
            CvInvoke.Log(imgHelper, imgLogImage);

            // normalization
            var sum = weights.Sum();
            if (weights.Sum() != 1.0)
                CvInvoke.cvConvertScale(image, image, sum, 0);

            for (int i = 0; i < weights.Count(); i++)
            {
                var helper = image.Clone();
                var ptr = helper.Ptr;

                QuickFilter(ref helper, sigmas.ElementAt(i));

                CvInvoke.cvConvertScale(helper, imgHelper, 1, 0);
                CvInvoke.Log(imgHelper, imgLogConvolved);
                CvInvoke.cvReleaseImage(ref ptr);

                CvInvoke.cvConvertScale(imgLogConvolved, imgLogConvolved, weights.ElementAt(i), 0);
                CvInvoke.Subtract(imgLogImage, imgLogConvolved, imgLogImage);
            }

            if (image.NumberOfChannels > 1)
            {
                var imgChannels = img.Split();

                CvInvoke.cvConvertScale(imgChannels[0], imgChannelB, 1, 0);
                CvInvoke.cvConvertScale(imgChannels[1], imgChannelG, 1, 0);
                CvInvoke.cvConvertScale(imgChannels[2], imgChannelR, 1, 0);

                foreach (var channel in imgChannels)
                {
                    var ptr = channel.Ptr;
                    CvInvoke.cvReleaseImage(ref ptr);
                }

                CvInvoke.Add(imgChannelB, imgChannelG, imgChannelHelper);
                CvInvoke.Add(imgChannelHelper, imgChannelR, imgChannelHelper);

                // normalization
                CvInvoke.Divide(imgChannelB, imgChannelHelper, imgChannelB, restorationFactor);
                CvInvoke.Divide(imgChannelG, imgChannelHelper, imgChannelG, restorationFactor);
                CvInvoke.Divide(imgChannelR, imgChannelHelper, imgChannelR, restorationFactor);

                CvInvoke.cvConvertScale(imgChannelB, imgChannelB, 1, 1);
                CvInvoke.cvConvertScale(imgChannelG, imgChannelG, 1, 1);
                CvInvoke.cvConvertScale(imgChannelR, imgChannelR, 1, 1);

                CvInvoke.Log(imgChannelB, imgChannelB);
                CvInvoke.Log(imgChannelG, imgChannelG);
                CvInvoke.Log(imgChannelR, imgChannelR);

                var channels = imgLogImage.Split();

                CvInvoke.Multiply(channels[0], imgChannelB, channels[0], colorGain);
                CvInvoke.Multiply(channels[1], imgChannelG, channels[1], colorGain);
                CvInvoke.Multiply(channels[2], imgChannelR, channels[2], colorGain);

                CvInvoke.Merge(new VectorOfMat(channels[0].Mat, channels[1].Mat, channels[2].Mat), imgLogImage.Mat);

                foreach (var channel in channels)
                {
                    var ptr = channel.Ptr;
                    CvInvoke.cvReleaseImage(ref ptr);
                }
            }

            CvInvoke.cvConvertScale(imgLogImage, image, gain, offset);

            CvInvoke.cvReleaseImage(ref helperPtr);
            CvInvoke.cvReleaseImage(ref logPtr);
            CvInvoke.cvReleaseImage(ref cLogPtr);
            CvInvoke.cvReleaseImage(ref channelBPtr);
            CvInvoke.cvReleaseImage(ref channelGPtr);
            CvInvoke.cvReleaseImage(ref channelRPtr);

            var savePath = $"{DateTime.Now.ToString("HH_mm_")}output_MSR.jpg";
            image.Save(savePath);

            return savePath;
        }

        public static void QuickFilter(ref Image<Bgr, double> img, int sigma)
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

                QuickFilter(ref sub, sigma / 2);

                CvInvoke.Resize(sub, img, img.Size);
                CvInvoke.cvReleaseImage(ref subPtr);
            }
        }
    }
}

