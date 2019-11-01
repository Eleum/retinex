using Emgu.CV;
using Emgu.CV.Bioinspired;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SingleScaleRetinex
{
    class Program
    {
        static void Main(string[] args)
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
