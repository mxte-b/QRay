using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QRay.Utility
{
    public static class Mask
    {
        private static List<Func<int, int, bool>> Patterns = new()
        {
            (row, col) => (row + col) % 2 == 0,
            (row, col) => row % 2 == 0,
            (row, col) => col % 3 == 0,
            (row, col) => (row + col) % 3 == 0,
            (row, col) => (row / 2 + col / 3) % 2 == 0,
            (row, col) => ((row * col) % 2) + ((row * col) % 3) == 0,
            (row, col) => (((row * col) % 2) + ((row * col) % 3)) % 2 == 0,
            (row, col) => (((row + col) % 2) + ((row * col) % 3)) % 2 == 0
        };

        public static void ApplyPattern(Cell[,] grid, int patternId)
        {
            Func<int, int, bool> pattern = Patterns[patternId];

            for (int y = 0; y < grid.GetLength(0); y++)
            {
                for (int x = 0; x < grid.GetLength(1); x++)
                {
                    if (!grid[y, x].IsData) continue;
                    grid[y, x].Value ^= pattern(y, x);
                }
            }
        }
    }
}
