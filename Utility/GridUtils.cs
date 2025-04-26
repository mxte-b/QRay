using QRay.Utility.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QRay.Utility
{
    public static class GridUtils
    {
        /// <summary>
        /// Creates a deep copy of a two-dimensional array of <see cref="Cell"/> objects.
        /// </summary>
        /// <param name="source">The source 2D array of <see cref="Cell"/> objects to copy.</param>
        /// <returns>A new 2D array of <see cref="Cell"/> objects that is a deep copy of the source array.</returns>
        /// <remarks>
        /// Each <see cref="Cell"/> in the source array is cloned to ensure that the new array
        /// contains independent copies of the original objects.
        /// </remarks>
        public static Cell[,] DeepCopy(Cell[,] source)
        {
            int rows = source.GetLength(0);
            int cols = source.GetLength(1);

            Cell[,] Copy = new Cell[rows, cols];

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    Copy[y, x] = source[y, x].Clone();
                }
            }

            return Copy;
        }

        /// <summary>  
        /// Determines whether the specified coordinates (x, y) are within the bounds of a grid.  
        /// </summary>  
        /// <param name="x">The x-coordinate to check.</param>  
        /// <param name="y">The y-coordinate to check.</param>  
        /// <param name="width">The width of the grid.</param>  
        /// <param name="height">The height of the grid.</param>  
        /// <returns>True if the coordinates are within the bounds of the grid; otherwise, false.</returns>  
        public static bool InBounds(int x, int y, int width, int height)
        {
            return x >= 0 && y >= 0 && x < width && y < height;
        }


        /// <summary>  
        /// Determines whether the specified coordinates (x, y) are within the bounds of the given 2D grid.  
        /// </summary>  
        /// <param name="x">The x-coordinate to check.</param>  
        /// <param name="y">The y-coordinate to check.</param>  
        /// <param name="grid">The 2D array of <see cref="Cell"/> objects representing the grid.</param>  
        /// <returns>True if the coordinates are within the bounds of the grid; otherwise, false.</returns>  
        /// <remarks>  
        /// This method calculates the grid's dimensions and ensures that the provided coordinates  
        /// are non-negative and less than the grid's width and height.  
        /// </remarks>  
        public static bool InBounds(int x, int y, Cell[,] grid)
        {
            int height = grid.GetLength(0);
            int width = grid.GetLength(1);

            return x >= 0 && y >= 0 && x < width && y < height;
        }

        /// <summary>  
        /// Retrieves the neighbouring <see cref="Cell"/> objects around a specified position in a 2D grid.  
        /// </summary>  
        /// <param name="grid">The 2D array of <see cref="Cell"/> objects representing the grid.</param>  
        /// <param name="x">The x-coordinate of the target cell.</param>  
        /// <param name="y">The y-coordinate of the target cell.</param>  
        /// <returns>A list of <see cref="Cell"/> objects that are neighbours of the specified cell.</returns>  
        /// <remarks>  
        /// This method considers all eight possible neighbours (diagonal, horizontal, and vertical)  
        /// and excludes the cell at the specified coordinates.  
        /// Only neighbors within the bounds of the grid are included in the result.  
        /// </remarks>  
        public static Dictionary<Direction, Cell?> GetAllNeighbours(Cell[,] grid, int x, int y)
        {
            Dictionary<Direction, Cell?> neighbors = new Dictionary<Direction, Cell?>();

            // Check each neighbour if in bounds  
            int height = grid.GetLength(0);
            int width = grid.GetLength(1);

            // Cyclical order
            int[] dx = { -1, 0, 1, 1, 1, 0, -1, -1 };
            int[] dy = { -1, -1, -1, 0, 1, 1, 1, 0 };

            for (int i = 0; i < 8; i++)
            {
                // Calculating the current coordinates
                int currentY = y + dy[i];
                int currentX = x + dx[i];

                // If the neighbor is in bounds of the grid, we add it to the list  
                if (InBounds(currentX, currentY, width, height))
                {
                    neighbors.Add((Direction)i, grid[currentY, currentX]);
                }
                else
                {
                    neighbors.Add((Direction)i, null);
                }
            }

            return neighbors;
        }

        public static Cell? GetNeighbour(Cell[,] grid, int x, int y, Direction dir)
        {
            // Calculating relative coordinate from direction
            int dy = (int)dir / 3 - 1;
            int dx = (int)dir % 3 - 1;

            int currentY = y + dy;
            int currentX = x + dx;

            if (InBounds(currentX, currentY, grid))
            {
                return grid[currentY, currentX];
            }

            return null;
        }

        public static bool IsSameValue(bool expected, List<Cell?> cells)
        {
            foreach (Cell? cell in cells)
            {
                if (!cell.HasValue || cell.Value.Value != expected)
                {
                    return false;
                }
            }

            return true;
        }
    }
}