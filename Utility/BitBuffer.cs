using QRay.Utility.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace QRay.Utility
{
    public class BitBuffer
    {
        public int Length = 0;
        private List<byte> Buffer = new List<byte>();
        private byte CurrentByte = 0;
        private int WritePosition = 0;

        /// <summary>
        /// Writes a specified number of bits from an integer value to the buffer, with an optional auto-flush behavior.
        /// </summary>
        /// <param name="value">The integer value to write to the buffer.</param>
        /// <param name="bits">The number of bits to write from the value.</param>
        /// <param name="option">
        /// Specifies additional behavior for writing to the buffer. If set to <see cref="BitBufferOptions.AutoFlush"/>,
        /// the buffer will be flushed after writing the bits, and padding will be added to complete the byte if necessary.
        /// </param>
        /// <remarks>
        /// This method writes the specified number of bits from the provided integer value to the buffer.
        /// If the <paramref name="option"/> is set to <see cref="BitBufferOptions.AutoFlush"/>, the buffer will be flushed
        /// after writing, and the total length will account for any padded bits required to complete the last byte.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the number of bits specified is less than 1 or greater than 32.
        /// </exception>
        public void Write(int value, int bits, BitBufferOptions option = BitBufferOptions.None)
        {
            for (int i = bits - 1; i >= 0; i--)
            {
                int bit = value >> i & 1;

                // Set bit at current position
                CurrentByte |= (byte)(bit << 7 - WritePosition);

                WritePosition++;
                Length++;

                if (WritePosition == 8)
                {
                    Flush();
                }
            }

            if (option == BitBufferOptions.AutoFlush && WritePosition > 0)
            {
                Flush();
                Length += 8 - WritePosition;
            }
        }

        /// <summary>
        /// Flushes the current byte to the buffer and resets the write position.
        /// </summary>
        /// <remarks>
        /// This method adds the current byte being written to the internal buffer,
        /// resets the <see cref="CurrentByte"/> to 0, and sets the <see cref="WritePosition"/> to 0.
        /// It is typically called when a byte is fully written or when explicitly flushing the buffer.
        /// </remarks>
        public void Flush()
        {
            Buffer.Add(CurrentByte);
            CurrentByte = 0;
            WritePosition = 0;
        }

        /// <summary>
        /// Retrieves a range of bytes from the buffer.
        /// </summary>
        /// <param name="start">The starting index of the range (inclusive).</param>
        /// <param name="end">The ending index of the range (exclusive).</param>
        /// <returns>A list of bytes representing the specified range.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the start or end index is out of bounds of the buffer.
        /// </exception>
        public List<byte> GetBytes(int start, int end)
        {
            List<byte> buffer = GetBuffer();
            if (start < 0 || end > buffer.Count)
            {
                throw new ArgumentOutOfRangeException("The index was out of bounds of the BitBuffer.");
            }

            return buffer[start..end];
        }

        /// <summary>
        /// Retrieves a range of bytes converted to integers from the buffer.
        /// </summary>
        /// <param name="start">The starting index of the range (inclusive).</param>
        /// <param name="end">The ending index of the range (exclusive).</param>
        /// <returns>A list of ints representing the specified range.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the start or end index is out of bounds of the buffer.
        /// </exception>
        public List<int> GetIntegers(int start, int end)
        {
            List<byte> buffer = GetBuffer();
            if (start < 0 || end > buffer.Count)
            {
                throw new ArgumentOutOfRangeException("The index was out of bounds of the BitBuffer.");
            }

            return buffer[start..end].ConvertAll(x => (int)x);
        }

        /// <summary>  
        /// Creates a new BitBuffer instance from a list of bytes.  
        /// </summary>  
        /// <param name="bytes">The list of bytes to initialize the BitBuffer with.</param>  
        /// <returns>A new BitBuffer instance containing the provided bytes.</returns>  
        /// <remarks>  
        /// This method initializes a BitBuffer with the given list of bytes.  
        /// The buffer's length is set to the total number of bits in the provided bytes.  
        /// </remarks>  
        public static BitBuffer FromBytes(List<byte> bytes)
        {
            BitBuffer buffer = new BitBuffer();

            buffer.Buffer = new List<byte>(bytes);
            buffer.Length = bytes.Count * 8;

            return buffer;
        }

        /// <summary>
        /// Retrieves the current state of the buffer, including all written bytes and the current byte being written to.
        /// </summary>
        /// <returns>
        /// A list of bytes representing the buffer's state. If there are unwritten bits in the current byte,
        /// it will be included in the output as the last byte.
        /// </returns>
        public List<byte> GetBuffer()
        {
            List<byte> output = new List<byte>(Buffer);

            // Flush current byte
            if (WritePosition > 0)
            {
                output.Add(CurrentByte);
            }

            return output;
        }

        /// <summary>
        /// Retrieves the current state of the buffer as a list of boolean values, where each bit is represented as a boolean.
        /// </summary>
        /// <returns>
        /// A list of boolean values representing the bits in the buffer. Each bit is converted to a boolean,
        /// where `true` represents a bit value of 1 and `false` represents a bit value of 0.
        /// </returns>
        /// <remarks>
        /// This method processes the internal buffer byte by byte, converting each bit into a boolean value.
        /// It also handles any partially written byte at the end of the buffer, ensuring that all bits written
        /// to the buffer are included in the output.
        /// </remarks>
        public BitArray GetBoolBuffer()
        {
            int totalBits = Length;
            int bitIndex = 0;

            BitArray output = new BitArray(totalBits);

            // We use the private Buffer property here, because we need to explicitly
            // handle the partially filled byte at the end. The use of GetBuffer() would
            // automatically align the partially filled byte to 8 bits, which we don't need.
            for (int i = 0; i < Buffer.Count; i++)
            {
                for (int j = 7; j >= 0; j--)
                {
                    output[bitIndex++] = (Buffer[i] >> j & 1) == 1;
                }
            }

            // Explicit handling of the partially filled byte
            if (WritePosition > 0)
            {
                for (int j = 7; j >= 8 - WritePosition; j--)
                {
                    output[bitIndex++] = (CurrentByte >> j & 1) == 1;
                }
            }

            return output;
        }

        /// <summary>
        /// Converts the current state of the BitBuffer into a human-readable binary string representation.
        /// </summary>
        /// <returns>
        /// A string representing the binary content of the buffer, with bits grouped into bytes.
        /// Includes information about the number of non-padded and padded bits.
        /// </returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            int bitIndex = 0;
            List<byte> buffer = GetBuffer();

            // Read all buffer bytes first
            foreach (byte b in buffer)
            {
                for (int i = 7; i >= 0; i--)
                {
                    sb.Append((b >> i & 1) == 1 ? "1" : "0");
                    bitIndex++;
                    //if (bitIndex % 8 == 0) sb.Append(" ");
                    //if (bitIndex == 4) sb.Append(" | "); // After mode
                    //if (bitIndex == 12) sb.Append(" | "); // After CCI
                }
            }

            sb.AppendLine($"\nNon-padded bits: {Length}\tPadded bits: {buffer.Count * 8}");
            return sb.ToString().TrimEnd();
        }
    }
}
