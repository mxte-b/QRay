using QRay.Utility;
using QRay.Utility.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QRay.Components
{
    public class QRMessage
    {
        public BitArray AsBits;

        private BitBuffer Buffer;
        private ECBlockInfo BlockInfo;
        private ResourceDatabase Database = ResourceDatabase.Instance;
        public QRMessage(List<List<List<int>>> data, List<List<List<int>>> errorCodewords, ECBlockInfo blockInfo, int version)
        {
            BlockInfo = blockInfo;

            // Interleaving the data and error correction codewords
            List<byte> interleaved = InterleaveData(data, errorCodewords);
            
            // Constructing a buffer to store the data
            Buffer = BitBuffer.FromBytes(interleaved);

            // Adding remainder bits if necessary
            int remainderBits = Database.RemainderBits.Last(x => x.Item1 <= version).Item2;
            if (remainderBits > 0)
            {
                AddRemainderBits(remainderBits);
            }

            // Converting the buffer into a list of boolean values
            AsBits = Buffer.GetBoolBuffer();
        }

        private List<byte> InterleaveData(List<List<List<int>>> data, List<List<List<int>>> errorCodewords)
        {
            List<byte> interleaved = new List<byte>();
            int maxCodewords = Math.Max(BlockInfo.Group1DataCodewords, BlockInfo.Group2DataCodewords);

            if (BlockInfo.Group1Blocks == 1 && BlockInfo.Group2Blocks == 0)
            {
                // Since there is only 1 block, we just append the data
                // and error correction blocks after each other and return.
                interleaved.AddRange(data[0][0].ConvertAll(x => (byte)x));
                interleaved.AddRange(errorCodewords[0][0].ConvertAll(x => (byte)x));

                return interleaved;
            }

            // Interleaving the data codewords
            for (int i = 0; i < maxCodewords; i++)
            {
                // Selecting from group 1's blocks
                if (i < BlockInfo.Group1DataCodewords)
                {
                    for (int b = 0; b < BlockInfo.Group1Blocks; b++)
                    {
                        interleaved.Add((byte)data[0][b][i]);
                    }
                }

                // Selecting from group 2's blocks
                for (int b = 0; b < BlockInfo.Group2Blocks; b++)
                {
                    interleaved.Add((byte)data[1][b][i]);
                }
            }

            // Interleaving the error correction codewords
            for (int i = 0; i < BlockInfo.ECCodewordsPerBlock; i++)
            {
                // Selecting from group 1's blocks
                for (int b = 0; b < BlockInfo.Group1Blocks; b++)
                {
                    interleaved.Add((byte)errorCodewords[0][b][i]);
                }

                // Selecting from group 2's blocks
                for (int b = 0; b < BlockInfo.Group2Blocks; b++)
                {
                    interleaved.Add((byte)errorCodewords[1][b][i]);
                }
            }
            Logger.Log(String.Join(" ", interleaved.Select(x => x.ToString("X2"))), LogLevel.Debug);
            return interleaved;
        }

        private void AddRemainderBits(int bits)
        {
            Buffer.Write(0, bits);
        }
    }
}
