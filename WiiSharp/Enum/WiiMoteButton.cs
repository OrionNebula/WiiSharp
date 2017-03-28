using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lotrek.WiiSharp.Enum
{
    [Flags]
    public enum WiimoteButton : ushort
    {
        Two = 0x1,
        One = 0x2,
        B = 0x4,
        A = 0x8,
        Minus = 0x10,
        ZAccelBit6 = 0x20,
        ZAccelBit7 = 0x40,
        Home = 0x80,
        Left = 0x100,
        Right = 0x200,
        Down = 0x400,
        Up = 0x800,
        Plus = 0x1000,
        ZAccelBit4 = 0x2000,
        ZAccelBit5 = 0x4000,
        Unknown = 0x8000,
        All = 0x1F9F
    }
}
