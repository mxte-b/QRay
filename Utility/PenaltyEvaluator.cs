using QRay.Utility.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace QRay.Utility
{
    public static class PenaltyEvaluator
    {
        public static int Evaluate(List<Cell[,]> masks)
        {
            // We track the penalties for all masks
            List<int> penalties = new List<int>();

            for (int i = 0; i < 8; i++)
            {
                Cell[,] grid = masks[i];

                // Holds the combined penalties accross the conditions
                int penalty = 0;

                // Evaluating the conditions
                penalty += ModuleLines(grid);
                penalty += ModuleBlocks(grid);
                penalty += FinderPatterns(grid);
                penalty += ModuleBalance(grid);

                // We push the penalty to the list
                penalties.Add(penalty);
            }

            return penalties.IndexOf(penalties.Min());
        }

        private static int ModuleLines(Cell[,] grid)
        {
            int penalty = 0;

            // Horizontal line evaluation
            for (int y = 0; y < grid.GetLength(0); y++)
            {
                bool bit = false;       // This will hold the bit value of the line

                int consecutive = 0;    // This holds how many modules the line is comprised of

                for (int x = 0; x < grid.GetLength(1); x++)
                {
                    if (grid[y, x].Value == bit)
                    {
                        consecutive++;

                        if (consecutive == 5)
                        {
                            penalty += 3;
                        }

                        if (consecutive > 5)
                        {
                            penalty++;
                        }
                    }
                    else
                    {
                        bit = !bit;
                        consecutive = 1;
                    }
                }
            }

            // Vertical line evaluation
            for (int x = 0; x < grid.GetLength(1); x++)
            {
                bool bit = false;       // This will hold the bit value of the line

                int consecutive = 0;    // This holds how many modules the line is comprised of

                for (int y = 0; y < grid.GetLength(0); y++)
                {
                    if (grid[y, x].Value == bit)
                    {
                        consecutive++;

                        if (consecutive == 5)
                        {
                            penalty += 3;
                        }

                        if (consecutive > 5)
                        {
                            penalty++;
                        }
                    }
                    else
                    {
                        bit = !bit;
                        consecutive = 1;
                    }
                }
            }

            return penalty;
        }

        private static int ModuleBlocks(Cell[,] grid)
        {
            int penalty = 0;

            // Loop through all modules
            for (int y = 0; y < grid.GetLength(0) - 1; y++)
            {
                for (int x = 0; x < grid.GetLength(1) - 1; x++)
                {
                    // Accessing the current cell's value
                    bool current = grid[y, x].Value;

                    if (grid[y, x + 1].Value == current && grid[y + 1, x].Value == current && grid[y + 1, x + 1].Value == current)
                    {
                        penalty += 3;
                    }
                }
            }

            return penalty;
        }

        private static int FinderPatterns(Cell[,] grid)
        {
            int penalty = 0;

            bool[] pattern = [true, false, true, true, true, false, true, false, false, false, false];
            bool[] reverse = pattern.Reverse().ToArray();

            int L = pattern.Length;

            // Loop through all cells horizontally sliding a window of length L
            for (int y = 0; y < grid.GetLength(0); y++)
            {
                for (int x = 0; x <= grid.GetLength(1) - L; x++)
                {
                    bool matchForward = true;
                    bool matchBackward = true;

                    // We check if there is a pattern match in the window
                    for (int i = 0; i < L; i++)
                    {
                        bool value = grid[y, x + i].Value;

                        if (matchForward && value != pattern[i]) matchForward = false;
                        if (matchBackward && value != reverse[i]) matchBackward = false;

                        if (!matchForward && !matchBackward) break;
                    }

                    // If there was a match in the window, we add a penalty of 40
                    if (matchForward || matchBackward)
                    {
                        penalty += 40;
                    }
                }
            }

            // Loop through all cells vertically sliding a window of length L
            for (int x = 0; x < grid.GetLength(1); x++)
            {
                for (int y = 0; y <= grid.GetLength(0) - L; y++)
                {
                    bool matchForward = true;
                    bool matchBackward = true;

                    // We check if there is a pattern match in the window
                    for (int i = 0; i < L; i++)
                    {
                        bool value = grid[y + i, x].Value;

                        if (matchForward && value != pattern[i]) matchForward = false;
                        if (matchBackward && value != reverse[i]) matchBackward = false;

                        if (!matchForward && !matchBackward) break;
                    }

                    // If there was a match in the window, we add a penalty of 40
                    if (matchForward || matchBackward)
                    {
                        penalty += 40;
                    }
                }
            }

            return penalty;
        }

        private static int ModuleBalance(Cell[,] grid)
        {

            Cell[] flat = grid.Cast<Cell>().ToArray();

            int dark = flat.Count(x => x.Value);
            
            int ratio = 100 * dark / flat.Length;
            int mod = ratio % 5;

            // Multiples of 5
            int prev = ratio - mod;
            int next = 5 - mod + ratio;

            return Math.Min( Math.Abs(prev - 50) / 5, Math.Abs(next - 50) / 5 ) * 10;
        }
    }
}
