using QRay.Components;
using QRay.Utility;
using QRay.Utility.Enums;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Schema;

namespace QRay
{
    internal class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine(QRayLogo.V2);
            Console.WriteLine($"{"QR Code Encoder - QRay 1.0", 30}\n{"Made by mxte_b", 24}\n\n");

            // Retrieving data to be encoded and desired error correction level
            string payload = ConsoleInput.GetInput<string>("<#> Text to encode:");
            char errorCorrection = ConsoleInput.GetInput<char>("<#> Error Correction level (L, M, Q, H):", QRMetadata.ValidLevelsRegExp);

            // Assigning custom shapes to cells
            QRRenderOptions renderOptions = new QRRenderOptions(CellColoringOptions.None);
            renderOptions.Scale = 20;
            renderOptions.SetDefaultCellShape(CellShape.Circle); // Note: The default shape for finder patterns is always CellShape.Square

            // Creating the QR Code
            QRCode qr = new(payload, errorCorrection, renderOptions);

            // Rendering the image
            Image<Rgb24> image = qr.RenderImage();

            // Saving the image
            image.Save("output.png");

            // Opening the image
            Process.Start(new ProcessStartInfo("output.png") { UseShellExecute = true });

            //ReedSolomon rs = new ReedSolomon(30);
            //var ecc = rs.Apply([7, 23, 86, 151, 50, 6, 86, 230, 150, 210, 226, 4, 70, 246, 230]);
            //Console.WriteLine(String.Join(" ", ecc.Select(x => x.ToString("X2"))));
            //Console.WriteLine($"SHOULD BE");
            //Console.WriteLine("92 9E 47 D1 16 1E 88 E4 F7 99 EC 5A 0A 24 AB AD 9A 5D EA 0F F8 0D 18 FB 08 03 AF 45 19 63\n\n");

            //Console.WriteLine(String.Join(" ", ecc));
            //Console.WriteLine($"SHOULD BE");
            //Console.WriteLine("146 158 71 209 22 30 136 228 247 153 236 90 10 36 171 173 154 93 234 15 248 13 24 251 8 3 175 69 25 99");
        }
    }
}
