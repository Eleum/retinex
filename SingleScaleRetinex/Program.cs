using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
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

            var image = new Image<Rgb, byte>(path.Replace("\"", ""));
            System.Diagnostics.Process.Start(image.ApplySSR(7, 80));
        }
    }
}
