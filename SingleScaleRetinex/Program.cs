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

            //var path = Console.ReadLine();
            //var image = new Image<Rgb, byte>(path.Replace("\"", ""));

            var path = Console.ReadLine();
            if (path == "1")
                path = @"C:\Users\Dmitry Vasilevich\Desktop\24.jpg";

            var image = new Image<Rgb, byte>(path.Replace("\"", ""));

            //image.ApplySSR(7, 80);

            var proc = System.Diagnostics.Process.Start(image.ApplySSR(5, 80));
            var proc1 = System.Diagnostics.Process.Start(Extensions.SingleScaleRetinex(image, 5, 80));
        }
    }
}
