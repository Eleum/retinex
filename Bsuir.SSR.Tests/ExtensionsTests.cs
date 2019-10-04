using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SingleScaleRetinex;

namespace Bsuir.SSR.Tests
{
    [TestClass]
    public class ExtensionsTests
    {
        [TestMethod]
        public void ConvolutionTest()
        {
            var kernel = new double[,]
            {
                { -1, -2, -1 },
                { 0, 0, 0 },
                { 1, 2, 1 },
            };

            var matrix = new int[,,]
            {
                { 
                    { 1 },
                    { 2 },
                    { 3 }
                },
                {
                    { 4 },
                    { 5 },
                    { 6 }
                },
                {
                    { 7 },
                    { 8 },
                    { 9 }
                }
            };

            Extensions.FlipMatrixByDimensions(kernel, 0, 1);

            var actual = Extensions.ConvolveValuesTest(matrix, kernel, 0);
            var expected = new int[,,]
            {
                {
                    { -13 },
                    { -20 },
                    { -17 }
                },
                {
                    { -18 },
                    { -24 },
                    { -18 }
                },
                {
                    { 13 },
                    { 20 },
                    { 17 }
                }
            };

            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
