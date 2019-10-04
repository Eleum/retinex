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
        /// <paramref name="img">Image to process</paramref>
        /// <paramref name="gaussKernelDim">Gaussian kernel dimension</paramref>
        /// <paramref name="sigma">Width of the Gaussian kerner</paramref>
        /// </summary>
        public static string ApplySSR(this Image<Rgb, byte> img, int gaussKernelDim, int sigma)
        {
            var scales = new[] { 10, 50, 80 };
            var channels = img.Split();
            var data = img.Data;
            var dataCopy = (byte[,,])data.Clone();

            for (int channel = 0; channel < channels.Length; channel++)
            {
                //for (int sc = 0; sc < scales.Length; sc++)
                //{
                    var kernel = CreateGaussKernel(sigma, gaussKernelDim);
                    FlipMatrixByDimensions(kernel, 0, 1);

                    var convolved = ConvolveValues(dataCopy, kernel, channel);

                    for (int i = 0; i < img.Rows; i++)
                    {
                        for (int j = 0; j < img.Cols; j++)
                        {
                            var value = Math.Log(data[i, j, channel]) - Math.Log(convolved[i, j, channel]);
                            //var value = Math.Log(data[i, j, channel] / convolved[i, j, channel]);
                            value = value * 255 + 127.5;

                            data[i, j, channel] = (byte)value;
                        }
                    }
                //}
            }

            //var c = ConvolveValues(dataCopy, kernel, 0);
            //c = ConvolveValues(c, kernel, 1);
            //c = ConvolveValues(c, kernel, 2);

            //var image = new Image<Rgb, byte>(c.GetUpperBound(0) + 1, c.GetUpperBound(1) + 1)
            //{
            //    Data = c
            //};
            //image.Save($"aaa.jpg");

            var savePath = $"{DateTime.Now.ToString("HH_mm_")}123.jpg";
            img.Save(savePath);

            return savePath;
        }

        public static string SingleScaleRetinex(this Image<Rgb, byte> img, int gaussianKernelSize, double sigma)
        {
            var radius = gaussianKernelSize / 2;
            var kernelSize = 2 * radius + 1;

            var ycc = img.Convert<Ycc, byte>();

            // Формируем ядро Гауссиана
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

            // Работаем только с яркостным каналом
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

            var image = ycc.Convert<Bgr, byte>();

            var savePath = $"{DateTime.Now.ToString("HH_mm_")}1234.jpg";
            image.Save(savePath);

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

        private static byte[,,] ConvolveValues(byte[,,] input, double[,] kernel, int channel)
        {
            var height = input.GetLength(0);
            var width = input.GetLength(1);

            int sizeY = kernel.GetLength(0), sizeX = kernel.GetLength(1);

            var offsetY = sizeY / 2;
            var offsetX = sizeX / 2;

            var convolved = (byte[,,])input.Clone();

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

                    convolved[y, x, channel] = (byte)sum;
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

        private static T[][] ToJaggedArrayRows<T>(this T[,] arr)
        {
            return Enumerable.Range(0, arr.GetUpperBound(0) + 1)
                             .Select(i => Enumerable.Range(0, arr.GetUpperBound(1) + 1)
                                                    .Select(j => arr[i, j])
                                                    .ToArray())
                             .ToArray();
        }

        private static T[][] ToJaggedArrayCols<T>(this T[,] arr)
        {
            return Enumerable.Range(0, arr.GetUpperBound(1) + 1)
                             .Select(j => Enumerable.Range(0, arr.GetUpperBound(0) + 1)
                                                    .Select(i => arr[j, i])
                                                    .ToArray())
                             .ToArray();
        }
    }
}

