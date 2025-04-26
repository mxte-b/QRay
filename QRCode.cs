using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using QRay.Components;
using QRay.Utility;
using System.Reflection.Emit;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using QRay.Utility.Enums;

namespace QRay
{
    public class QRCode
    {
        private QRMetadata Metadata;
        private QRMessage FinalMessage;
        private MatrixBuilder Builder;

        private ResourceDatabase Database= ResourceDatabase.Instance;
        private QRRenderOptions RenderOptions;

        private BitBuffer Buffer = new BitBuffer();
        private ECBlockInfo BlockInfo;
        private ReedSolomon RS;

        private List<List<List<int>>> GroupedData = new List<List<List<int>>>();
        private List<List<List<int>>> GroupedECCodewords = new List<List<List<int>>>();
        public QRCode(string payload, char errorCorrection, QRRenderOptions? renderOptions = null)
        {
            // Setting the rendering options
            RenderOptions = renderOptions ?? QRRenderOptions.Default;

            // Creating the QR Code metadata  
            Metadata = new QRMetadata(payload, errorCorrection);
            Metadata.WriteDataToBuffer(Buffer);

            // Getting the block informations  
            BlockInfo = Database.ECBlockInfos.First(x => x.Version == Metadata.Version && x.ECLevel == Metadata.ErrorCorrectionLevel);

            // Writing the encoded payload to the buffer  
            QRBody.WriteBodyToBuffer(Buffer, payload);

            // Applying padding to the buffer  
            ApplyPadding();

            // Grouping codewords  
            GroupCodewords();

            // Creating the Reed-Solomon helper class instance for error correction  
            RS = new ReedSolomon(BlockInfo.ECCodewordsPerBlock);

            // Generating error correction codewords  
            GroupedECCodewords = RS.ApplyErrorCorrection(GroupedData);

            // Constructing the final message  
            FinalMessage = new QRMessage(GroupedData, GroupedECCodewords, BlockInfo, Metadata.Version);

            // Generating the QR Code Matrix  
            Builder = new MatrixBuilder(Metadata.Version, Metadata.ErrorCorrectionLevel, FinalMessage.AsBits, RenderOptions);
            Builder.BuildMatrix();
        }

        /// <summary>  
        /// Adds padding bits to the QR code's BitBuffer to meet the required bit length specified in the QR specification.  
        /// This method ensures that the buffer is properly aligned and padded with alternating bytes as per the QR spec.  
        /// </summary>  
        /// <exception cref="InvalidOperationException">  
        /// Thrown when the buffer length is not a multiple of 8 during padding alignment or  
        /// when the final buffer length does not match the required bits.  
        /// </exception>  
        public void ApplyPadding()
        {
            // Add missing bits to match the required bits in the spec  
            if (Buffer.Length < Metadata.RequiredBits)
            {
                int missingBits = Metadata.RequiredBits - Buffer.Length;

                // Add terminator if necessary  
                int terminatorLength = Math.Min(missingBits, 4);
                if (terminatorLength == 0) return;

                Buffer.Write(0, terminatorLength, BitBufferOptions.AutoFlush);

                missingBits = Metadata.RequiredBits - Buffer.Length;
                if (missingBits == 0) return;

                if (Buffer.Length % 8 != 0)
                {
                    throw new InvalidOperationException("Buffer length is not a multiple of 8, padding alignment issue detected.");
                }

                // These are the padding bytes defined in the QR spec for padding.  
                // They are added in an alternating way.  
                byte[] paddingBytes = [0xEC, 0x11];
                int missingBytes = missingBits / 8;

                for (int i = 0; i < missingBytes; i++)
                {
                    int index = i % 2 != 0 ? 1 : 0;
                    Buffer.Write(paddingBytes[index], 8);
                }

                if (Buffer.Length != Metadata.RequiredBits)
                {
                    throw new InvalidOperationException("Buffer length does not match the required bits.");
                }
            }
        }

        public void GroupCodewords()
        {
            GroupedData.Clear();
            int byteIndex = 0;

            // Group 1
            List<List<int>> group = new List<List<int>>();
            for (int i = 0; i < BlockInfo.Group1Blocks; i++)
            {
                group.Add(Buffer.GetIntegers(byteIndex, byteIndex + BlockInfo.Group1DataCodewords));
                byteIndex += BlockInfo.Group1DataCodewords;
            }

            // Appending Group 1 to the body
            GroupedData.Add(group);

            // Group 2 (if necessary)
            if (BlockInfo.Group2Blocks == 0) return;

            group = new List<List<int>>();
            for (int i = 0; i < BlockInfo.Group2Blocks; i++)
            {
                group.Add(Buffer.GetIntegers(byteIndex, byteIndex + BlockInfo.Group2DataCodewords));
                byteIndex += BlockInfo.Group2DataCodewords;
            }

            // Appending Group 2 to the body
            GroupedData.Add(group);
        }

        public Image<Rgb24> RenderImage()
        {
            return Builder.Render();
        }
    }
}
