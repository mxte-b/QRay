using QRay.Utility;
using QRay.Utility.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QRay.Components
{
    public class QRMetadata
    {
        public const string ValidLevelsRegExp = "^[LMQH]$";             // RegEx expression for valid levels.

        public ErrorCorrectionLevel ErrorCorrectionLevel { get; set; }  // Error correction level in bytes
        public int Version { get; set; }                                // Version of this QR Code
        public EncodingMode Mode { get; set; } = EncodingMode.Byte;     // Encoding mode
        public int CharacterCapacity { get; set; }                      // Character capacity
        public int CharacterCount { get; set; }                         // Character count
        public int CCILength { get; set; }                              // Character count indicator's length in bits
        public int RequiredBits { get; set; }                           // Required number of bits in total

        private ResourceDatabase database = ResourceDatabase.Instance;

        /// <summary>
        /// Initializes a new instance of the QRMetadata class.
        /// This constructor determines the error correction level, QR code version, 
        /// and the length of the Character Count Indicator (CCI) based on the provided payload, 
        /// error correction level, and resource database.
        /// </summary>
        /// <param name="payload">The string payload to encode in the QR code.</param>
        /// <param name="errorCorrection">The character representing the error correction level.</param>
        /// <param name="database">The ResourceDatabase containing necessary data.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the provided error correction level is invalid or the payload is too long to encode.
        /// </exception>
        public QRMetadata(string payload, char errorCorrection)
        {
            DetermineECLevel(errorCorrection);
            DetermineVersion(payload);
            DetermineCCILength();
            DetermineRequiredBits();
        }

        /// <summary>
        /// Determines the error correction level based on the provided character.
        /// Throws an exception if the provided level is not valid.
        /// </summary>
        /// <param name="errorCorrection">The character representing the error correction level.</param>
        /// <exception cref="ArgumentException">Thrown when the provided error correction level is not valid.</exception>
        public void DetermineECLevel(char errorCorrection)
        {
            ErrorCorrectionLevel ecLevel = (ErrorCorrectionLevel)errorCorrection;

            // Checking the validity of the provided error correction level
            if (!Enum.IsDefined(typeof(ErrorCorrectionLevel), ecLevel))
            {
                throw new ArgumentException($"The following error correction level is not a valid one: {errorCorrection}");
            }

            ErrorCorrectionLevel = ecLevel;
        }

        /// <summary>
        /// Determines the appropriate QR code version based on the payload length and the error correction level.
        /// Uses the provided ResourceDatabase to find the smallest version that can accommodate the payload.
        /// Throws an exception if the payload is too long for any version.
        /// </summary>
        /// <param name="payload">The string payload to encode in the QR code.</param>
        /// <param name="database">The ResourceDatabase containing character capacity data for each version and error correction level.</param>
        /// <exception cref="ArgumentException">Thrown when the payload is too long to encode with the given parameters.</exception>
        public void DetermineVersion(string payload)
        {
            UTF8Encoding encoder = new UTF8Encoding();
            int charCount = encoder.GetByteCount(payload);
            int index = database.CharacterCapacities[ErrorCorrectionLevel].FindIndex(x => x >= charCount);

            if (index == -1) throw new ArgumentException("The payload is too long to encode with these parameters.");

            Version = index + 1;
            CharacterCapacity = database.CharacterCapacities[ErrorCorrectionLevel][index];
            CharacterCount = charCount;
        }

        /// <summary>
        /// Determines the length of the Character Count Indicator (CCI) based on the QR code version and encoding mode.
        /// The CCI length is determined by dividing the QR code versions into three ranges:
        /// - Versions 1 to 9
        /// - Versions 10 to 26
        /// - Versions 27 to 40
        /// The method uses the provided ResourceDatabase to retrieve the CCI length for the current encoding mode and version range.
        /// </summary>
        /// <param name="database">The ResourceDatabase containing CCI length data for each encoding mode and version range.</param>
        public void DetermineCCILength()
        {
            int rangeIndex = Version <= 9 ? 0 : Version <= 26 ? 1 : 2;
            CCILength = database.CCILengths[Mode][rangeIndex];
        }

        /// <summary>
        /// Determines the required number of bits for the QR code based on the version and error correction level.
        /// This method retrieves the appropriate ECBlockInfo from the ResourceDatabase and assigns the RequiredBits property.
        /// </summary>
        public void DetermineRequiredBits()
        {
            RequiredBits = database.ECBlockInfos.First(x => x.Version == Version && x.ECLevel == ErrorCorrectionLevel).RequiredBits;
        }

        /// <summary>
        /// Writes the QR code metadata to the provided BitBuffer.
        /// This includes the encoding mode and the character count, 
        /// which are written using the specified number of bits.
        /// </summary>
        /// <param name="buffer">The BitBuffer to write the data into.</param>
        public void WriteDataToBuffer(BitBuffer buffer)
        {
            buffer.Write((byte)Mode, 4); // Write the encoding mode using 4 bits
            buffer.Write(CharacterCount, CCILength); // Write the character count using the CCI length
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("\n<#> QR Code Metadata <#>");
            sb.AppendLine($" - Error Correction Level: {ErrorCorrectionLevel}");
            sb.AppendLine($" - Encoding Mode: {Mode}");
            sb.AppendLine($" - Character Capacity: {CharacterCapacity}");
            sb.AppendLine($" - Character Count: {CharacterCount}");
            sb.AppendLine($" - CCI Length: {CCILength}");
            sb.AppendLine($" - Required Bits: {RequiredBits}");
            return sb.ToString().TrimEnd();
        }
    }
}
