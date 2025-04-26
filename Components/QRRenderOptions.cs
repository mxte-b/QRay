using QRay.Utility.Enums;
using SixLabors.ImageSharp.Drawing.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QRay.Components
{
    public class QRRenderOptions
    {
        public Dictionary<CellType, CellShape> ShapeMap;
        public CellShape DefaultShape;
        public CellColoringOptions ColorOptions;
        public int QuietZoneSize = 4;
        public int Scale = 10;

        // Default option
        public static QRRenderOptions Default = new QRRenderOptions();
        public QRRenderOptions()
        {
            ShapeMap = new Dictionary<CellType, CellShape>();
            ColorOptions = CellColoringOptions.None;
        }

        public QRRenderOptions(CellColoringOptions coloring, Dictionary<CellType, CellShape> map, int quiet = 4, int scale = 10)
        {
            ShapeMap = map;
            ColorOptions = coloring;
            QuietZoneSize = quiet;
            Scale = scale;
        }

        public QRRenderOptions(CellColoringOptions coloring, int quiet = 4, int scale = 10)
        {
            ShapeMap = new Dictionary<CellType, CellShape>();
            ColorOptions = coloring;
            QuietZoneSize = quiet;
            Scale = scale;
        }

        public void SetDefaultCellShape(CellShape shape)
        {
            DefaultShape = shape;
        }
        public void SetShape(CellType type, CellShape shape)
        {
            if (type == CellType.Finder && shape == CellShape.Circle)
            {
                throw new InvalidOperationException("Please refrain from using dots for the finder pattern. This could lead to unreadable QR Codes.");
            }
            ShapeMap[type] = shape;
        }
    }
}
