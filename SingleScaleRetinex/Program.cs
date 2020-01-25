using System;
using Emgu.CV;
using Emgu.CV.Structure;

namespace SingleScaleRetinex
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.Write("Path: ");
            var path = Console.ReadLine();

            var image = new Image<Bgr, byte>(path.Replace("\"", ""));

            System.Diagnostics.Process.Start(image.ApplySSR(80));
            //System.Diagnostics.Process.Start(image.ApplyMSR(
            //    Enumerable.Repeat(1.0 / 3, 3),
            //    new[] { 12, 80, 250 }
            //));
            //System.Diagnostics.Process.Start(image.ApplyMSRCR(
            //    Enumerable.Repeat(1.0 / 3, 3),
            //    new[] { 12, 80, 250 },
            //    30, -6,
            //    125, 46
            //));
        }
    }
}
