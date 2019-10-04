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
            // to work only with luminance channel
            var ycc = img.Convert<Ycc, byte>();

            var scales = new[] { 10, 50, 80 };
            //var channels = ycc.Split();
            var data = ycc.Data;

            for (int channel = 0; channel < 1; channel++)
            {
                var kernel = CreateGaussKernel(sigma, gaussKernelDim);
                FlipMatrixByDimensions(kernel, 0, 1);

                var convolved = ConvolveValues(data, kernel, channel);

                for (int i = 0; i < img.Rows; i++)
                {
                    for (int j = 0; j < img.Cols; j++)
                    {
                        var value = Math.Log(data[i, j, channel]) - Math.Log(convolved[i, j, channel]);
                        value = value * 255 + 127.5;

                        data[i, j, channel] = (byte)value;
                    }
                }
            }

            img = ycc.Convert<Rgb, byte>();

            var savePath = $"{DateTime.Now.ToString("HH_mm_")}123.jpg";
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
    }
}

