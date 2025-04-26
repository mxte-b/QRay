using QRay.Utility.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QRay.Utility
{
    public class ResourceDatabase
    {
        private static ResourceDatabase? _instance = null;
        private static readonly object _instanceLock = new object();

        public List<Tuple<int, int>> RemainderBits { get; set; }
        public List<ECBlockInfo> ECBlockInfos { get; set; } = new List<ECBlockInfo>();

        public Dictionary<int, BitArray> VersionInfos { get; set; } = new Dictionary<int, BitArray>();
        public Dictionary<int, List<int>> AlignmentLocations { get; set; } = new Dictionary<int, List<int>>();
        public Dictionary<ErrorCorrectionLevel, List<ushort>> CharacterCapacities { get; set; } = new Dictionary<ErrorCorrectionLevel, List<ushort>>();
        public Dictionary<ErrorCorrectionLevel, List<BitArray>> FormatInfos { get; set; } = new Dictionary<ErrorCorrectionLevel, List<BitArray>>();
        public Dictionary<EncodingMode, int[]> CCILengths { get; set; } = new Dictionary<EncodingMode, int[]>();
        public Dictionary<int, int> LogTable { get; set; } = new Dictionary<int, int>();
        public Dictionary<int, int> AntiLogTable { get; set; } = new Dictionary<int, int>();

        private ResourceDatabase()
        {
            // Character Capacity Table
            foreach (string line in ResourceLoader.LoadEmbedded("QRay.Resources.QRCapacities.csv", ResourceLoaderOptions.SkipHeaders))
            {
                List<string> data = line.Split(',').ToList();

                ErrorCorrectionLevel ecLevel = (ErrorCorrectionLevel)data[0][0]; // First character is error correction Level
                data.RemoveRange(0, 1);

                // Converting from string to uint16
                List<ushort> capacities = new List<ushort>();
                foreach (string s in data)
                {
                    capacities.Add(Convert.ToUInt16(s));
                }

                CharacterCapacities[ecLevel] = capacities;
            }

            // CCI Length Table
            CCILengths = new Dictionary<EncodingMode, int[]>()
            {
                [EncodingMode.Numeric] = [10, 12, 14],
                [EncodingMode.Alphanumeric] = [9, 11, 13],
                [EncodingMode.Byte] = [8, 16, 16],
                [EncodingMode.Kanji] = [8, 10, 12],
            };

            // Error Correction and Block Informations Table
            foreach (string line in ResourceLoader.LoadEmbedded("QRay.Resources.ECBlockInfos.csv", ResourceLoaderOptions.SkipHeaders))
            {
                List<string> data = line.Split(',').ToList();
                ECBlockInfos.Add(new ECBlockInfo(data));
            }

            // Log and Anti-log Table (used for GF256)
            foreach (string line in ResourceLoader.LoadEmbedded("QRay.Resources.LogTable.csv", ResourceLoaderOptions.SkipHeaders))
            {
                string[] data = line.Split(',');
                LogTable.Add(Convert.ToUInt16(data[0]), Convert.ToUInt16(data[1]));
            }

            foreach (string line in ResourceLoader.LoadEmbedded("QRay.Resources.AntiLogTable.csv", ResourceLoaderOptions.SkipHeaders))
            {
                string[] data = line.Split(',');
                AntiLogTable.Add(Convert.ToUInt16(data[0]), Convert.ToUInt16(data[1]));
            }

            // Remainder Bits Table
            RemainderBits =
            [
                new(1, 0),
                new(2, 7),
                new(7, 0),
                new(8, 0),
                new(14, 3),
                new(21, 4),
                new(28, 3),
                new(35, 0)
            ];

            // Alignment Locations Table
            foreach (string line in ResourceLoader.LoadEmbedded("QRay.Resources.AlignmentLocations.csv", ResourceLoaderOptions.SkipHeaders))
            {
                string[] data = line.Split(',');
                List<int> locations = data[1].Split(';').ToList().ConvertAll(x => int.Parse(x));
                AlignmentLocations.Add(int.Parse(data[0]), locations);
            }

            // Format Information Table
            foreach (string line in ResourceLoader.LoadEmbedded("QRay.Resources.FormatInfos.csv", ResourceLoaderOptions.SkipHeaders))
            {
                string[] data = line.Split(',');
                List<BitArray> infos = data[1].Split(";").Select(x => new BitArray(x.Select(x => x == '1').ToArray())).ToList();
                FormatInfos.Add((ErrorCorrectionLevel)data[0][0], infos);
            }

            // Version Information Table
            foreach (string line in ResourceLoader.LoadEmbedded("QRay.Resources.VersionInfos.csv", ResourceLoaderOptions.SkipHeaders))
            {
                string[] data = line.Split(',');
                BitArray info = new BitArray(data[1].Select(x => x == '1').ToArray());
                VersionInfos.Add(int.Parse(data[0]), info);
            }
        }

        public static ResourceDatabase Instance
        {
            get
            {
                lock (_instanceLock)
                {
                    _instance ??= new ResourceDatabase();
                    return _instance;
                }
            }
        }
    }
}
