using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QRay.Utility.Enums
{
    public enum CellAccess
    {
        ReadWrite,      // Cells that can be written to
        ReadOnly,       // Read-only cells
        ProtectedWrite, // Cells where data should not be written, but patterns can overwrite these
        Reserved        // Reserved cells
    }
}
