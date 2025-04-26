using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QRay.Utility
{
    public static class LoggerHelper
    {
        public static string FormatGroupedData(List<List<List<int>>> groupedData)
        {
            StringBuilder sb = new StringBuilder();

            // Iterate through each group
            for (int groupIndex = 0; groupIndex < groupedData.Count; groupIndex++)
            {
                sb.AppendLine($"Group {groupIndex + 1}:");

                // Iterate through blocks in the group
                for (int blockIndex = 0; blockIndex < groupedData[groupIndex].Count; blockIndex++)
                {
                    sb.AppendLine($"  Block {groupIndex + 1},{blockIndex + 1}:");

                    // Iterate through the byte data in each block
                    var block = groupedData[groupIndex][blockIndex];
                    sb.AppendLine($"    Codewords: [{string.Join(", ", block)}]");
                    sb.AppendLine($"    Codewords: [{string.Join(", ", block.Select(b => b.ToString("X")))}]");
                }

                sb.AppendLine(); // Empty line after each group
            }

            return sb.ToString();
        }
    }

}
