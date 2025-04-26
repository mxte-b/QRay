using QRay.Utility.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace QRay.Utility
{
    public struct Cell
    {
        public bool Value;
        public CellAccess Access;
        public CellType Type;
        public bool IsData => Type == CellType.Data;
        public bool IsWritable => Access == CellAccess.ReadWrite;
        public byte AsByte => (byte)(Value ? 0 : 255);
        public Cell(bool value, CellAccess access, CellType type)
        {
            Value = value; 
            Access = access;
            Type = type;
        }
        public Cell Clone()
        {
            return new Cell(Value, Access, Type);
        }
    }
}
