using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QRay.Utility.Enums
{
    public enum ErrorCorrectionLevel
    {
        L = 0x4C, // Recovers  7% of the payload
        M = 0x4D, // Recovers 15% of the payload
        Q = 0x51, // Recovers 25% of the payload
        H = 0x48, // Recovers 30% of the payload
    }
}
