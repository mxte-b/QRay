using QRay.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections;
using System.Diagnostics;
using QRay.Utility.Enums;

namespace QRay.Components
{
    public class MatrixBuilder
    {
        private Cell[,] Grid;
        private List<Cell[,]> Masks = new List<Cell[,]>();
        private Cell[,]? Final;

        private int Size;
        private int Version;
        private ErrorCorrectionLevel ErrorCorrectionLevel;

        private ResourceDatabase Database = ResourceDatabase.Instance;
        private QRRenderOptions RenderOptions;

        private List<int>? AlignmentLocations;
        private List<BitArray> FormatInfos;
        private BitArray? VersionInfo;

        private BitArray Bits;
        public MatrixBuilder(int version, ErrorCorrectionLevel ecLevel, BitArray bits, QRRenderOptions renderOptions)
        {
            RenderOptions = renderOptions;

            // Calculating the size of the QR Code
            int size = 21 + 4 * (version - 1);

            Bits = bits;
            Grid = new Cell[size, size];
            Size = size;
            Version = version;
            ErrorCorrectionLevel = ecLevel;

            // Getting the alignment pattern locations
            if (Version > 1)
            {
                AlignmentLocations = Database.AlignmentLocations[Version];
            }

            // Getting the format and version informations
            FormatInfos = Database.FormatInfos[ErrorCorrectionLevel];
            if (Version >= 7)
            {
                VersionInfo = Database.VersionInfos[Version];
            }
        }

        public void BuildMatrix()
        {
            // Drawing function patterns and timing patterns
            AddTimingPatterns();
            AddAllFinders();
            AddSeparators();
            if (Version > 1)
            {
                AddAlignmentPatterns();
            }
            AddDarkModule();

            // Reserving certain areas
            ReserveFormatInfo();
            if (Version >= 7)
            {
                ReserveVersionInfo();
            }

            // Placing data bits
            PlaceDataBits();

            // Placing the version information, if necessary
            if (Version >= 7)
            {
                PlaceVersionInfo();
            }

            // Applying the masks
            ApplyMasks();

            // Placing the format information
            PlaceFormatInfo();

            // Choosing the best mask
            int bestMask = PenaltyEvaluator.Evaluate(Masks);
            Final = Masks[bestMask];
        }

        private void AddTimingPatterns()
        {
            for (int i = 0; i < Size; i++)
            {
                Grid[6, i] = new Cell(i % 2 == 0, CellAccess.ProtectedWrite, CellType.Timing);
                Grid[i, 6] = new Cell(i % 2 == 0, CellAccess.ProtectedWrite, CellType.Timing);
            }
        }

        private void AddAllFinders()
        {
            // Top-Left Finder
            AddFinder(0, 0);

            // Top-Right Finder
            AddFinder(Size - 7, 0);

            // Bottom-Left Finder
            AddFinder(0, Size - 7);
        }

        private void AddFinder(int startX, int startY)
        {
            for (int y = 0; y < 7; y++)
            {
                for (int x = 0; x < 7; x++)
                {
                    int absX = startX + x;
                    int absY = startY + y;

                    bool isWhite = (y == 1 || y == 5 || x == 1 || x == 5) && (x >= 1 && x <= 5 && y >= 1 && y <= 5);

                    Grid[absX, absY] = new Cell(!isWhite, CellAccess.ReadOnly, CellType.Finder);
                }
            }
        }

        private void AddSeparators()
        {
            for (int i = 0; i < 8; i++)
            {
                // Top-Left Separator
                Grid[7, i] = new Cell(false, CellAccess.ReadOnly, CellType.Separator);
                Grid[i, 7] = new Cell(false, CellAccess.ReadOnly, CellType.Separator);


                // Top-Right Separator
                Grid[7, Size - 8 + i] = new Cell(false, CellAccess.ReadOnly, CellType.Separator);
                Grid[i, Size - 8] = new Cell(false, CellAccess.ReadOnly, CellType.Separator);


                // Bottom-Left Separator
                Grid[Size - 8, i] = new Cell(false, CellAccess.ReadOnly, CellType.Separator);
                Grid[Size - 8 + i, 7] = new Cell(false, CellAccess.ReadOnly, CellType.Separator);
            }
        }

        private void AddAlignmentPatterns()
        {
            if (AlignmentLocations == null)
            {
                throw new InvalidOperationException("Placing alignment patterns in this QR Code is not possible due to the version being 1.");
            }

            // I use two for loops to include all possible combinations of the coordinates
            for (int i = 0; i < AlignmentLocations.Count; i++)
            {
                for (int j = 0; j < AlignmentLocations.Count; j++)
                {
                    int x = AlignmentLocations[j];
                    int y = AlignmentLocations[i];

                    // If the coordinate overlaps any non-writable cell, we skip - these are function patterns
                    if (Grid[y, x].Access == CellAccess.ReadOnly) continue;

                    AddAlignmentPattern(x, y);
                }
            }
        }

        private void AddAlignmentPattern(int centerX, int centerY)
        {
            for (int y = 0; y < 5; y ++)
            {
                for (int x = 0; x < 5; x++)
                {
                    int absX = centerX + x - 2;
                    int absY = centerY + y - 2;

                    bool isWhite = (y == 1 || y == 3 || x == 1 || x == 3) && (x > 0 && x < 4 && y > 0 && y < 4);

                    Grid[absY, absX] = new Cell(!isWhite, CellAccess.ReadOnly, CellType.Finder);
                }
            }
        }

        private void AddDarkModule()
        {
            Grid[9 + 4 * Version, 8] = new Cell(true, CellAccess.ReadOnly, CellType.Dark);
        }

        private void ReserveFormatInfo()
        {
            // Reserving near the top left finder
            for (int i = 0; i < 9; i ++)
            {
                if (Grid[8, i].IsWritable)
                {
                    Grid[8, i] = new Cell(false, CellAccess.Reserved, CellType.Format);
                }

                if (Grid[i, 8].IsWritable)
                {
                    Grid[i, 8] = new Cell(false, CellAccess.Reserved, CellType.Format);
                }
            }

            // Reserving under the top right finder and beside the bottom left finder
            for (int i = 0; i < 8; i++)
            {
                if (Grid[8, Size - i - 1].IsWritable)
                {
                    Grid[8, Size - i - 1] = new Cell(false, CellAccess.Reserved, CellType.Format);
                }

                if (Grid[Size - i - 1, 8].IsWritable)
                {
                    Grid[Size - i - 1, 8] = new Cell(false, CellAccess.Reserved, CellType.Format);
                }
            }
        }

        private void ReserveVersionInfo()
        {
            // Start coordinates
            int sx = 0;
            int sy = Size - 11;

            for (int i = 0; i < 18; i++)
            {
                int row = i / 6;
                int col = i % 6;

                Grid[sy + row, sx + col] = new Cell(false, CellAccess.Reserved, CellType.Version);
                Grid[sx + col, sy + row] = new Cell(false, CellAccess.Reserved, CellType.Version);
            }
        }

        private void PlaceDataBits()
        {
            // Number of placed bits
            int placedBits = 0;

            // Defining booleans
            // up: The direction of movement
            // leftModule: Whether to place the bit 1 module to the left (leftModule - zag)
            bool up = true;
            bool leftModule = false;

            // Starting from bottom left Corner
            int x = Size - 1;
            int y = Size - 1;

            while (placedBits != Bits.Count)
            {
                int currentX = x - (leftModule ? 1 : 0);

                if (Grid[y, currentX].IsWritable)
                {
                    Grid[y, currentX] = new Cell(Bits[placedBits], CellAccess.ReadWrite, CellType.Data);
                    placedBits++;
                }

                if (leftModule)
                {
                    // Check if a direction change should be made
                    if (up ? y == 0 : y == Size - 1)
                    {
                        x = x - 2;

                        // Skip vertical timing pattern
                        if (x == 6) x--;

                        up = !up;
                        leftModule = !leftModule;

                        continue;
                    }

                    // Update Y position if necessary and the leftModule variable
                    y += up ? -1 : 1;
                }

                leftModule = !leftModule;
            }
        }

        private void PlaceFormatInfo()
        {
            // Going through
            for (int maskId = 0; maskId < 8; maskId++)
            {
                Cell[,] mask = Masks[maskId];
                BitArray bits = FormatInfos[maskId];

                // Placing the first 7 bits
                for (int i = 0; i < 7; i++)
                {
                    // Calculating the X coordinates (we skip the timing pattern)
                    int x = i > 5 ? i + 1 : i;

                    // Getting the current bit
                    bool bit = bits[i];

                    // Setting the cells' values to this bit
                    mask[8, x].Value = bit;
                    mask[Size - 1 - i, 8].Value = bit;
                }

                // Placing the remaining bits
                for (int i = 0; i < 8; i++)
                {
                    // Calculating the Y coordinates (we skip the timing pattern)
                    int y = i > 1 ? i + 1 : i;

                    // Getting the current bit
                    bool bit = bits[7 + i];

                    // Setting the cells' values to this bit
                    mask[8 - y, 8].Value = bit;
                    mask[8, Size - 8 + i].Value = bit;
                }
            }
        }

        private void PlaceVersionInfo()
        {
            if (VersionInfo == null)
            {
                throw new InvalidOperationException("Placing version info in this QR Code is not possible due to the version being smaller than 7.");
            }

            int sx = 0;
            int sy = Size - 11;

            for (int i = 0; i < 18; i++)
            {
                int row = i % 3;
                int col = i / 3;

                bool bit = VersionInfo[17 - i];

                Grid[sy + row, sx + col].Value = bit;
                Grid[sx + col, sy + row].Value = bit;
            }
        }

        private void ApplyMasks()
        {
            // Creating all 8 masks
            for (int maskId = 0; maskId < 8; maskId++)
            {
                // Creating a copy of the grid
                Cell[,] copy = GridUtils.DeepCopy(Grid);

                // Applying the mask to the grid
                Mask.ApplyPattern(copy, maskId);

                // Appending the mask to the list
                Masks.Add(copy);
            }
        }

        public Image<Rgb24> Render()
        {
            if (Final == null)
            {
                throw new InvalidOperationException("QR matrix has not been built. Call `Builder.BuildMatrix()` before rendering.");
            }

            return ImageHelper.FromCells(Final, RenderOptions);
        }
    }
}
