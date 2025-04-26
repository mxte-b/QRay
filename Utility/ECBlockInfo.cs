using QRay.Utility.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QRay.Utility
{
    public class ECBlockInfo
    {
        public int Version { get; set; }
        public ErrorCorrectionLevel ECLevel { get; set; }
        public int TotalDataCodewords { get; set; }
        public int TotalECCodewords 
        { 
            get
            {
                return (Group1Blocks + Group2Blocks) * ECCodewordsPerBlock;
            }
        }
        public int TotalBlocks
        {
            get
            {
                return (Group1Blocks + Group2Blocks);
            }
        }
        public int ECCodewordsPerBlock { get; set; }
        public int Group1Blocks { get; set; }
        public int Group1DataCodewords { get; set; }
        public int Group2Blocks { get; set; }
        public int Group2DataCodewords { get; set; }
        public int RequiredBits
        {
            get
            {
                Console.WriteLine($"Total: {TotalDataCodewords}");
                return TotalDataCodewords * 8;
            }
        }

        public ECBlockInfo(List<string> data)
        {
            string[] versionAndEC = data[0].Split('-');
            Version = int.Parse(versionAndEC[0]);
            ECLevel = (ErrorCorrectionLevel)versionAndEC[1][0];

            TotalDataCodewords = int.Parse(data[1]);
            ECCodewordsPerBlock = int.Parse(data[2]);
            Group1Blocks = int.Parse(data[3]);
            Group1DataCodewords = int.Parse(data[4]);
            Group2Blocks = string.IsNullOrWhiteSpace(data[5]) ? 0 : int.Parse(data[5]);
            Group2DataCodewords = string.IsNullOrWhiteSpace(data[6]) ? 0 : int.Parse(data[6]);
        }
    }
}
