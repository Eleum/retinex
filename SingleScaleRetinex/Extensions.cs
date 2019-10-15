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
    public static string ApplySSR(this Image<Bgr, byte> img, int gaussKernelDim, int sigma)
        {
            // to work only with luminance channel
            var image = img.Convert<Bgr, double>();

            //MyFilter(ref image, sigma);

            //Environment.Exit(0);

            //var gray = new Image<Gray, byte>(ycc.Size);
            //CvInvoke.cvSetImageCOI(ycc, 1);
            //CvInvoke.cvCopy(ycc, gray, IntPtr.Zero);

            //var data = image.Data;

            var A = new Image<Bgr, double>(image.Size);
            var AA = new Image<Bgr, double>(image.Size);
            var imgA = new Image<Bgr, double>(image.Size);
            var imgB = new Image<Bgr, double>(image.Size);
            var imgC = new Image<Bgr, double>(image.Size);

            //CvInvoke.cvCopy(image, imgLog, IntPtr.Zero);
            //CvInvoke.cvCopy(image, imgConvLog, IntPtr.Zero);
            //CvInvoke.cvCopy(image, imgSub, IntPtr.Zero);

            CvInvoke.cvConvertScale(image, imgA, 1, 0);
            CvInvoke.Log(imgA, imgB);

            A = image.Clone();
            AA = image.Clone();

            //Filter(ref A, sigma);
            MyFilter(ref A, sigma);

            //var kernel = CreateGaussKernel(sigma, gaussKernelDim);
            //FlipMatrixByDimensions(kernel, 0, 1);
            //var convolved = new Image<Bgr, double>(ConvolveValues(AA.Data, kernel));
            //CvInvoke.cvConvertScale(convolved, imgA, 1, 0);

            CvInvoke.cvConvertScale(A, imgA, 1, 0);
            CvInvoke.Log(imgA, imgC);

            CvInvoke.Subtract(imgB, imgC, imgA);

            CvInvoke.cvConvertScale(imgA, image, 128, 128);

            var ptrImg = img.Ptr;
            var Aptr = A.Ptr;
            var ptrA = imgA.Ptr;
            var ptrB = imgB.Ptr;
            var ptrC = imgC.Ptr;

            CvInvoke.cvReleaseImage(ref ptrImg);
            CvInvoke.cvReleaseImage(ref Aptr);
            CvInvoke.cvReleaseImage(ref ptrA);
            CvInvoke.cvReleaseImage(ref ptrB);
            CvInvoke.cvReleaseImage(ref ptrC);

            var savePath = $"{DateTime.Now.ToString("HH_mm_")}123.jpg";
            image.Save(savePath);

            return savePath;


            //CvInvoke.Log(image, imgConvLog);

            //var imageConvolved = new Image<Bgr, double>(convolved);

            //CvInvoke.Subtract(imgLog, imgConvLog, imgSub);

            //for (int channel = 0; channel < 3; channel++)
            //{
            //    var convolved = ConvolveValues(data, kernel, channel);

            //    for (int i = 0; i < img.Rows; i++)
            //    {
            //        for (int j = 0; j < img.Cols; j++)
            //        {
            //            var value = Math.Log(data[i, j, channel]) - Math.Log(convolved[i, j, channel]);
            //            //value = value * 128 + 128;

            //            //if (value > 255)
            //            //    value = 255;
            //            //if (value < 0)
            //            //    value = 0;

            //            data[i, j, channel] = (byte)value;
            //        }
            //    }
            //}

            //CvInvoke.cvCopy(gray, ycc, IntPtr.Zero);
            //CvInvoke.cvSetImageCOI(ycc, 0);

            //img = ycc.Convert<Bgr, byte>();

            //CvInvoke.cvConvertScale(imgSub, image, 128, 128);

            //var savePath = $"{DateTime.Now.ToString("HH_mm_")}123.jpg";
            //image.Save(savePath);

            //return savePath;
        }

        public static void MyFilter(ref Image<Bgr, double> img, int sigma)
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

                //var sub1 = new Image<Bgr, double>(img1.Width / 2, img1.Height / 2);
                //Downsample(img1, ref sub1);

                MyFilter(ref sub, sigma/2);

                CvInvoke.Resize(sub, img, img.Size);
                CvInvoke.cvReleaseImage(ref subPtr);
            }
        }

        private static void Downsample(Image<Bgr, double> src, ref Image<Bgr, double> dest)
        {
            var arr = new double[dest.Data.GetLength(0), dest.Data.GetLength(1), 3];

            var kernel = GetGaussPyramidKernel();
            var data = ConvolveValues(src.Data, kernel, true);

            for (int channel = 0; channel < dest.NumberOfChannels; channel++)
            {
                for (int i = 0, row = 0; i < dest.Data.GetLength(1); i++)
                {
                    if ((i + 1) % 2 == 0)
                        continue;

                    for (int j = 0, col = 0; j < dest.Data.GetLength(0); j++)
                    {
                        if ((j + 1) % 2 == 0)
                            continue;

                        arr[col++, row, channel] = data[j, i, channel];
                    }

                    row++;
                }
            }

            dest.Data = arr;
        }

        private static void Filter(ref Image<Bgr, double> img, int sigma)
        {
            if (sigma > 300) sigma = 300;

            var filterSize = (int)Math.Floor(sigma * 6 * 0.5);
            filterSize = filterSize * 2 + 1;

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
                Filter(ref sub, sigma/2);
                CvInvoke.Resize(sub, img, img.Size);
                CvInvoke.cvReleaseImage(ref subPtr);
            }
        }

        public static Image<Bgr, double> FilterGaussian(Image<Bgr, double> img, int sigma)
        {
            var kernel = CreateGaussKernel(sigma);
            FlipMatrixByDimensions(kernel, 0, 1);
            return new Image<Bgr, double>(ConvolveValues(img.Data, kernel));
        }

        public static string ApplyMSR(this Image<Bgr, byte> img, int gaussKernelDim)
        {
            var scales = new[] { 15, 80, 250 };
            var channels = img.Split();
            var data1 = img.Data;

            var ycc = img.Convert<Ycc, byte>();
            var data = ycc.Data;

            var diffs = new double[3];

            for (int channel = 0; channel < 1; channel++)
            {
                var convolutions = CreateConvolutions(data, CreateKernels(scales, gaussKernelDim), channel);

                for (int i = 0; i < img.Rows; i++)
                {
                    for (int j = 0; j < img.Cols; j++)
                    {
                        for (var scale = 0; scale < scales.Length; scale++)
                        {
                            var value = Math.Log(data[i, j, channel]) - Math.Log(convolutions[scale][i, j, channel]);
                            diffs[scale] = value;
                        }

                        var val = diffs.Sum(x => x / 3) * 255 + 127.5;

                        //if (val > 255)
                        //    val = 255;
                        if (val < 0)
                            val = Math.Log(0.01);

                        data[i, j, channel] = (byte)val;
                    }
                }
            }

            img = ycc.Convert<Bgr, byte>();

            var savePath = $"{DateTime.Now.ToString("HH_mm_")}123.jpg";
            img.Save(savePath);

            return savePath;
        }

        private static double[][,] CreateKernels(int[] scales, int gaussKernelDim)
        {
            var kernels = new List<double[,]>();

            for (int i = 0; i < scales.Length; i++)
            {
                var kernel = CreateGaussKernel(scales[i], gaussKernelDim);
                FlipMatrixByDimensions(kernel, 0, 1);
                kernels.Add(kernel);
            }

            return kernels.ToArray();
        }

        private static double[][,,] CreateConvolutions(byte[,,] data, double[][,] kernels, int channel)
        {
            //var convolutions = new List<double[,,]>();

            //foreach (var kernel in kernels)
            //{
            //    var convolved = ConvolveValues(data, kernel, channel);
            //    convolutions.Add(convolved);
            //}

            //return convolutions.ToArray();
            return null;
        }

        public static string SingleScaleRetinex(this Image<Bgr, byte> img, int gaussianKernelSize, double sigma)
        {
            var radius = gaussianKernelSize / 2;
            var kernelSize = 2 * radius + 1;

            var ycc = img.Convert<Ycc, byte>();

            var sum = 0f;
            var gaussKernel = new float[kernelSize * kernelSize];
            for (int i = -radius, k = 0; i <= radius; i++, k++)
            {
                for (int j = -radius; j <= radius; j++)
                {
                    var val = (float)Math.Exp(-(i * i + j * j) / (sigma * sigma));
                    gaussKernel[k] = val;
                    sum += val;
                }
            }
            for (int i = 0; i < gaussKernel.Length; i++)
                gaussKernel[i] /= sum;

            var gray = new Image<Gray, byte>(ycc.Size);
            CvInvoke.cvSetImageCOI(ycc, 1);
            CvInvoke.cvCopy(ycc, gray, IntPtr.Zero);

            // Размеры изображения
            var width = img.Width;
            var height = img.Height;

            var bmp = gray.Bitmap;
            var bitmapData = bmp.LockBits(new Rectangle(Point.Empty, gray.Size), ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);

            unsafe
            {
                for (var y = 0; y < height; y++)
                {
                    var row = (byte*)bitmapData.Scan0 + y * bitmapData.Stride;
                    for (var x = 0; x < width; x++)
                    {
                        var color = row + x;

                        float val = 0;

                        for (int i = -radius, k = 0; i <= radius; i++, k++)
                        {
                            var ii = y + i;
                            if (ii < 0) ii = 0; if (ii >= height) ii = height - 1;

                            var row2 = (byte*)bitmapData.Scan0 + ii * bitmapData.Stride;
                            for (int j = -radius; j <= radius; j++)
                            {
                                var jj = x + j;
                                if (jj < 0) jj = 0; if (jj >= width) jj = width - 1;

                                val += *(row2 + jj) * gaussKernel[k];

                            }
                        }

                        var newColor = 127.5 + 255 * Math.Log(*color / val);
                        if (newColor > 255)
                            newColor = 255;
                        else if (newColor < 0)
                            newColor = 0;
                        *color = (byte)newColor;
                    }
                }
            }
            bmp.UnlockBits(bitmapData);

            CvInvoke.cvCopy(gray, ycc, IntPtr.Zero);
            CvInvoke.cvSetImageCOI(ycc, 0);

            img = ycc.Convert<Bgr, byte>();

            var savePath = $"{DateTime.Now.ToString("HH_mm_")}SSR.jpg";
            img.Save(savePath);

            return savePath;
        }

        public static void FlipMatrixByDimensions(double[,] arr, params int[] dimensions)
        {
            foreach (var dimension in dimensions)
            {
                if (dimension > arr.Rank)
                {
                    throw new ArgumentOutOfRangeException();
                }

                DoMatrixFlip(arr, dimension);
            }
        }

        private static void DoMatrixFlip(double[,] arr, int dimension)
        {
            for (int i = 0; i < arr.GetLength(dimension); i++)
            {
                var rowLength = arr.Length / arr.GetLength(dimension);

                if (dimension > 0)
                {
                    for (int j = 0; j < rowLength / 2; j++)
                    {
                        var temp = arr[i, j];
                        arr[i, j] = arr[i, rowLength - 1 - j];
                        arr[i, rowLength - 1 - j] = temp;
                    }
                }
                else
                {
                    for (int j = 0; j < rowLength / 2; j++)
                    {
                        var temp = arr[j, i];
                        arr[j, i] = arr[rowLength - 1 - j, i];
                        arr[rowLength - 1 - j, i] = temp;
                    }
                }
            }
        }

        private static double[,] CreateGaussKernel(int sigma, int size = 5)
        {
            var gaussKernel = new double[size, size];
            var radius = size / 2;

            var sum = 0.0;

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    var val = Math.Exp(-(Math.Pow(x - radius, 2) + Math.Pow(y - radius, 2)) / (2 * Math.PI * Math.Pow(sigma, 2)));
                    gaussKernel[x, y] = val;
                    sum += val;
                }
            }

            // normalize the kernel
            // divide values in the Gaussian curve by the total area under the curve, so that the values add up to 1
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    gaussKernel[x, y] /= sum;
                }
            }

            return gaussKernel;
        }

        private static double[,] GetGaussPyramidKernel()
        {
            return new double[,]
            {
                { 1, 4, 6, 4, 1},
                { 4, 16, 24, 16, 4},
                { 6, 24, 36, 24, 6},
                { 4, 16, 24, 16, 4},
                { 1, 4, 6, 4, 1},
            };
        }

        private static double[,,] ConvolveValues(double[,,] input, double[,] kernel, bool useGaussPyramid = false, int channel = -1)
        {
            var height = input.GetLength(0);
            var width = input.GetLength(1);

            int sizeY = kernel.GetLength(0), sizeX = kernel.GetLength(1);

            var offsetY = sizeY / 2;
            var offsetX = sizeX / 2;

            var convolved = new double[input.GetLength(0), input.GetLength(1), 3];

            // to keep data of channels which are not processed
            Array.Copy(input, convolved, input.GetLength(0) * input.GetLength(1));

            for (int ch = (channel == -1 ? 0 : channel); ch < (channel == -1 ? 3 : channel + 1); ch++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        var sum = 0.0;

                        // [kernel] values -- coords in matrix assuming [0,0] is kernel center
                        // [i, j] -- kernel 0-based coords
                        for (int kernelY = -offsetY, j = 0; kernelY <= offsetY; kernelY++, j++)
                        {
                            for (int kernelX = -offsetX, i = 0; kernelX <= offsetX; kernelX++, i++)
                            {
                                var inputY = y + kernelY;
                                var inputX = x + kernelX;

                                if (inputX < 0 || inputY < 0)
                                    continue;

                                if (inputX >= width || inputY >= height)
                                    continue;

                                sum += input[inputY, inputX, ch] * kernel[j, i] * (useGaussPyramid ? 1.0/256 : 1);
                            }
                        }

                        convolved[y, x, ch] = sum;
                    }
                }
            }

            return convolved;
        }

        public static int[,,] ConvolveValuesTest(int[,,] input, double[,] kernel, int channel)
        {
            var height = input.GetLength(0);
            var width = input.GetLength(1);

            int sizeY = kernel.GetLength(0), sizeX = kernel.GetLength(1);

            var offsetY = sizeY / 2;
            var offsetX = sizeX / 2;

            var convolved = (int[,,])input.Clone();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var sum = 0.0;

                    // [kernel] values -- coords in matrix according to kernel
                    // [i, j] -- kernel 0-based coords
                    for (int kernelY = -offsetY, j = 0; kernelY <= offsetY; kernelY++, j++)
                    {
                        for (int kernelX = -offsetX, i = 0; kernelX <= offsetX; kernelX++, i++)
                        {
                            var inputY = y + kernelY;
                            var inputX = x + kernelX;

                            if (inputX < 0 || inputY < 0)
                                continue;

                            if (inputX >= width || inputY >= height)
                                continue;

                            sum += input[inputY, inputX, channel] * kernel[j, i];
                        }
                    }

                    convolved[y, x, channel] = (int)sum;
                }
            }

            return convolved;
        }
    }
}

