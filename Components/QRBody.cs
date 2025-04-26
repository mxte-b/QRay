using QRay.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QRay.Components
{
    public static class QRBody
    {

        public static void WriteBodyToBuffer(BitBuffer buffer, string payload)
        {
            List<byte> encodedData = FormatPayloadUTF8(payload).ToList();
            foreach (byte b in encodedData)
            {
                buffer.Write(b, 8);
            }
        }

        /// <summary>  
        /// Encodes the given payload into a UTF-8 byte array.  
        /// </summary>  
        /// <param name="payload">The string payload to be encoded.</param>  
        /// <returns>A byte array containing the UTF-8 encoded payload.</returns>  
        private static byte[] FormatPayloadUTF8(string payload)
        {
            UTF8Encoding encoder = new UTF8Encoding();
            return encoder.GetBytes(payload);
        }
    }
}
