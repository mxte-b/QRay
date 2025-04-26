using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using QRay.Utility.Enums;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Drawing;
using QRay.Components;

namespace QRay.Utility
{
    public static class ImageHelper
    {
        public static Image<Rgb24> FromCells(Cell[,] Grid, QRRenderOptions renderOptions)
        {
            int width = Grid.GetLength(0);
            int height = Grid.GetLength(1);

            // Calculate full image size with respect to the quiet zone size and scale
            int imageWidth = (width + 2 * renderOptions.QuietZoneSize) * renderOptions.Scale;
            int imageHeight = (height + 2 * renderOptions.QuietZoneSize) * renderOptions.Scale;

            Image<Rgb24> image = new Image<Rgb24>(imageWidth, imageHeight);

            // Fill the image with the color white
            FillWhite(image, imageHeight);

            int radius = renderOptions.Scale / 2;

            image.Mutate(ctx =>
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        // Skip white cells - they are already filled
                        Cell cell = Grid[y, x];

                        if (!cell.Value && renderOptions.ColorOptions == CellColoringOptions.None) continue;

                        Rgb24 color = renderOptions.ColorOptions switch
                        {
                            CellColoringOptions.None => new(cell.AsByte, cell.AsByte, cell.AsByte),
                            CellColoringOptions.Debug => GetCellColor(cell),
                            _ => throw new ArgumentException("Unknown coloring option.")
                        };


                        // Calculate the coordinates with scale & quiet zone
                        int cx = (x + renderOptions.QuietZoneSize) * renderOptions.Scale;
                        int cy = (y + renderOptions.QuietZoneSize) * renderOptions.Scale;

                        IPath shape = GetShape(renderOptions, cell.Type, cx, cy, radius);

                        ctx.Fill(color, shape);
                    }
                }
            });

            return image;
        }

        /// <summary>  
        /// Fills the entire image with white pixels.  
        /// </summary>  
        /// <param name="image">The <see cref="Image{L8}"/> to fill with white pixels.</param>  
        /// <param name="height">The height of the image in pixels.</param>  
        private static void FillWhite(Image<Rgb24> image, int height)
        {
            image.Mutate(ctx => ctx.Fill(Color.White));
        }

        private static Rgb24 GetCellColor(Cell cell)
        {
            return cell.Type switch
            {
                CellType.None => new Rgb24(200, 200, 200),

                CellType.Finder => new Rgb24(0, 0, cell.AsByte),

                CellType.Separator => new Rgb24(255, 255, 255),

                CellType.Dark => new Rgb24(0, 0, cell.AsByte),

                CellType.Timing => new Rgb24(0, cell.AsByte, 0),

                CellType.Data => new Rgb24(cell.AsByte, cell.AsByte, cell.AsByte),

                CellType.Format => new Rgb24(cell.AsByte, cell.AsByte, 0),

                CellType.Version => new Rgb24(cell.AsByte, 0, 255),

                _ => new Rgb24(0, 0, 0)
            };
        }

        private static IPath GetShape(QRRenderOptions renderOptions, CellType type, int cx, int cy, float r)
        {
            CellShape shape;

            if (!renderOptions.ShapeMap.ContainsKey(type))
            {
                shape = type == CellType.Finder ? CellShape.Square : renderOptions.DefaultShape;
            }
            else
            {
                shape = renderOptions.ShapeMap[type];
            }

            return shape switch
            {
                CellShape.Square => new RectangularPolygon(cx, cy, r * 2, r * 2),
                CellShape.Circle => new EllipsePolygon(cx + r, cy + r, r * 0.8f),
                _ => throw new NotImplementedException("This shape is currently not supported by the renderer.")
            };
        }
    }
}
