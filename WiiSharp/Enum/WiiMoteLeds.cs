using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnarcLabs.WiiSharp.Enum
{
    [Flags]
    public enum WiimoteLeds
    {
        None = 0,
        Led1 = 0x1,
        Led2 = 0x2,
        Led3 = 0x4,
        Led4 = 0x8,
        All = 0xF,
    }
}
