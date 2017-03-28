using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lotrek.WiiSharp.Enum
{
    [Flags]
    public enum WiimoteFlags : int
    {
        Smoothing = 0x1,
        Continuous = 0x2,
        OrientThreshold = 0x4,
        Init = Smoothing | OrientThreshold,
    }
}
